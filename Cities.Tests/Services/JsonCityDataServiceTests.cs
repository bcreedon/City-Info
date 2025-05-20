// Cities.Tests/Services/JsonCityDataServiceTests.cs
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Cities.Api.Services;
using Cities.Api.Models;
using System.Text.Json;
using System.IO; // Make sure System.IO is included
using System; // For IDisposable and Guid
using System.Collections.Generic; // For List<>

namespace Cities.Tests.Services;

public class JsonCityDataServiceTests : IDisposable
{
    private readonly Mock<ILogger<JsonCityDataService>> _mockLogger;
    private readonly Mock<IHostEnvironment> _mockHostEnvironment;
    // Remove _testDataPath as it's likely redundant now
    // private readonly string _testDataPath = "test_cities.json";
    private readonly string _tempDataDirectory; // Make readonly after assignment

    public JsonCityDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<JsonCityDataService>>();
        _mockHostEnvironment = new Mock<IHostEnvironment>();
        _tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); // Assign base temp path

        // --- Start of Corrected Logic ---

        // 1. Create the base temporary directory first
        Directory.CreateDirectory(_tempDataDirectory);

        // 2. Setup the mock IHostEnvironment to use this temp directory
        //    The service looks for "Data/cities.json" relative to this path.
        _mockHostEnvironment.Setup(m => m.ContentRootPath).Returns(_tempDataDirectory);

        // 3. Define the sample data
        var sampleCities = new List<CityData>
        {
            new() { Id = "test1", Name = "Test City One", State = "TS", SummerHighFahrenheit = 80, WinterLowFahrenheit = 30, ElevationFeet = 100, Population = 10000, TimeZone = "America/New_York" },
            new() { Id = "test2", Name = "Test City Two", State = "TS", SummerHighFahrenheit = 90, WinterLowFahrenheit = 40, ElevationFeet = 200, Population = 20000, TimeZone = "America/Los_Angeles" }
        };
        var jsonContent = JsonSerializer.Serialize(sampleCities);

        // 4. Define the full path for the subdirectory and the target file
        //    The service expects a "Data" folder containing "cities.json"
        var targetSubDirectory = Path.Combine(_tempDataDirectory, "Data");
        var targetFilePath = Path.Combine(targetSubDirectory, "cities.json"); // This is the file the service needs

        // 5. ***Create the subdirectory BEFORE trying to write the file***
        Directory.CreateDirectory(targetSubDirectory);

        // 6. Write the JSON file to the correct location
        File.WriteAllText(targetFilePath, jsonContent);

        // --- End of Corrected Logic ---

        // Note: The previous potentially problematic lines writing "test_cities.json"
        // and the duplicate Directory.CreateDirectory call are removed.
    }

    // ... Rest of your tests (GetCityByNameAsync_CityExists_ReturnsCityData, etc.) ...

    public void Dispose()
    {
        // Clean up the temporary directory and its contents
        if (Directory.Exists(_tempDataDirectory))
        {
            try
            {
                Directory.Delete(_tempDataDirectory, true);
            }
            catch (Exception ex)
            {
                // Log or output the cleanup error if needed, but don't fail the test run
                Console.WriteLine($"Error cleaning up test directory '{_tempDataDirectory}': {ex.Message}");
            }
        }
        GC.SuppressFinalize(this);
    }
}