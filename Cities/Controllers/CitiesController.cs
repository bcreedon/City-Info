using Cities.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;   // For TimeZoneInfo
using Cities.Dtos;
using Cities.Services;

namespace Cities.Controllers;

/// <summary>
/// API controller for retrieving information about U.S. cities.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Base route: /api/cities
public class CitiesController : ControllerBase
{
    private readonly ICityDataService _cityDataService;
    private readonly ILogger<CitiesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CitiesController"/> class.
    /// Dependency Injection provides the ICityDataService and ILogger.
    /// </summary>
    /// <param name="cityDataService">The service for accessing city data.</param>
    /// <param name="logger">The logger for logging controller actions.</param>
    public CitiesController(ICityDataService cityDataService, ILogger<CitiesController> logger)
    {
        _cityDataService = cityDataService ?? throw new ArgumentNullException(nameof(cityDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets information for a specific city by its name.
    /// </summary>
    /// <param name="cityName">The name of the city (e.g., "New York City").</param>
    /// <returns>An <see cref="ActionResult{CityInfoDto}"/> containing the city information if found,
    /// or a <see cref="NotFoundResult"/> if the city is not found.</returns>
    /// <response code="200">Returns the requested city's information.</response>
    /// <response code="404">If the city is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{cityName}")]
    [ProducesResponseType(typeof(CityInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CityInfoDto>> GetCityInfo(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            _logger.LogWarning("City name parameter was null or whitespace.");
            return BadRequest("City name cannot be empty.");
        }

        _logger.LogInformation("Attempting to retrieve information for city: {CityName}", cityName);

        var cityData = await _cityDataService.GetCityByNameAsync(cityName);

        if (cityData == null)
        {
            _logger.LogWarning("City not found: {CityName}", cityName);
            return NotFound($"Information for city '{cityName}' not found.");
        }

        // Map CityData to CityInfoDto (Manual mapping for clarity, AutoMapper could be used in larger projects)
        var cityInfoDto = MapToDto(cityData);

        _logger.LogInformation("Successfully retrieved information for city: {CityName}", cityName);
        return Ok(cityInfoDto);
    }

    /// <summary>
    /// Gets a list of all available cities.
    /// </summary>
    /// <returns>A list of city names and their IDs.</returns>
    /// <response code="200">Returns a list of all cities.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllCityNames()
    {
        _logger.LogInformation("Attempting to retrieve all city names.");
        var cities = await _cityDataService.GetAllCitiesAsync();
        var cityList = cities.Select(c => new { c.Id, c.Name, c.State }).ToList();
        _logger.LogInformation("Retrieved {Count} cities.", cityList.Count);
        return Ok(cityList);
    }


    /// <summary>
    /// Maps a <see cref="CityData"/> object to a <see cref="CityInfoDto"/> object.
    /// This includes formatting and calculating derived data like current local time.
    /// </summary>
    /// <param name="cityData">The source <see cref="CityData"/> object.</param>
    /// <returns>The mapped <see cref="CityInfoDto"/> object.</returns>
    private CityInfoDto MapToDto(CityData cityData)
    {
        string currentTimeLocal = "N/A";
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(cityData.TimeZone);
            var utcNow = DateTime.UtcNow;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);
            currentTimeLocal = localTime.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
        }
        catch (TimeZoneNotFoundException tzex)
        {
            _logger.LogWarning(tzex, "Time zone ID '{TimeZoneId}' for city '{CityName}' not found on this system.", cityData.TimeZone, cityData.Name);
            currentTimeLocal = $"Invalid TimeZoneId: {cityData.TimeZone}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating local time for city: {CityName}", cityData.Name);
        }

        return new CityInfoDto
        {
            Name = cityData.Name,
            State = cityData.State,
            Temperatures = new TemperatureInfo
            {
                SummerHighFahrenheit = $"{cityData.SummerHighFahrenheit} °F",
                WinterLowFahrenheit = $"{cityData.WinterLowFahrenheit} °F"
            },
            Elevation = $"{cityData.ElevationFeet} ft",
            Population = cityData.Population,
            CurrentTimeLocal = currentTimeLocal
        };
    }
}
