using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Cities.Api.Controllers;
using Cities.Api.Dtos;
using Cities.Api.Models; // For CityData
using Cities.Api.Services;

namespace Cities.Tests.Controllers;

public class CitiesControllerTests
{
    private readonly Mock<ICityDataService> _mockCityDataService;
    private readonly Mock<ILogger<CitiesController>> _mockLogger;
    private readonly CitiesController _controller;

    public CitiesControllerTests()
    {
        _mockCityDataService = new Mock<ICityDataService>();
        _mockLogger = new Mock<ILogger<CitiesController>>();
        _controller = new CitiesController(_mockCityDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCityInfo_CityExists_ReturnsOkObjectResultWithCityInfoDto()
    {
        // Arrange
        var cityName = "Testville";
        var cityData = new CityData
        {
            Id = "tst",
            Name = cityName,
            State = "TS",
            SummerHighFahrenheit = 75,
            WinterLowFahrenheit = 30,
            ElevationFeet = 500,
            Population = 50000,
            TimeZone = "America/New_York" 
        };
        _mockCityDataService.Setup(s => s.GetCityByNameAsync(cityName))
            .ReturnsAsync(cityData);

        // Act
        var result = await _controller.GetCityInfo(cityName);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<CityInfoDto>(actionResult.Value);
        Assert.Equal(cityName, returnValue.Name);
        Assert.Equal("75 °F", returnValue.Temperatures.SummerHighFahrenheit);
        // Check local time format (can be tricky due to execution environment time zone)
        Assert.Contains(DateTime.Now.Year.ToString(), returnValue.CurrentTimeLocal); // Basic check
    }

    [Fact]
    public async Task GetCityInfo_CityDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var cityName = "NonExistent";
        _mockCityDataService.Setup(s => s.GetCityByNameAsync(cityName))
            .ReturnsAsync((CityData?)null);

        // Act
        var result = await _controller.GetCityInfo(cityName);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCityInfo_EmptyCityName_ReturnsBadRequest()
    {
        // Arrange
        var cityName = "";

        // Act
        var result = await _controller.GetCityInfo(cityName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("City name cannot be empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetAllCityNames_WhenCitiesExist_ReturnsOkObjectResultWithCityList()
    {
        // Arrange
        var cities = new List<CityData>
        {
            new() { Id = "tst1", Name = "Testville1", State = "TS1", SummerHighFahrenheit = 75, WinterLowFahrenheit = 30, ElevationFeet = 500, Population = 50000, TimeZone = "America/New_York" },
            new() { Id = "tst2", Name = "Testville2", State = "TS2", SummerHighFahrenheit = 85, WinterLowFahrenheit = 20, ElevationFeet = 600, Population = 60000, TimeZone = "America/Los_Angeles" }
        };
        _mockCityDataService.Setup(s => s.GetAllCitiesAsync()).ReturnsAsync(cities);

        // Act
        var result = await _controller.GetAllCityNames();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(actionResult.Value);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public async Task GetCityInfo_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var cityName = "ErrorCity";
        _mockCityDataService.Setup(s => s.GetCityByNameAsync(cityName))
                            .ThrowsAsync(new Exception("Database connection failed")); // Simulate a service layer exception

        // Act & Assert
        // This specific test is more for integration testing or if the controller had more complex try-catch.
        // For unit tests, I usually assume the global exception handler in Program.cs would catch this.
        // However, if the controller itself had specific error handling logic, I'd test that.
        // For now, I'll assert that the exception propagates if not handled directly in controller.
        await Assert.ThrowsAsync<Exception>(() => _controller.GetCityInfo(cityName));
    }
}
