using System.Text.Json;
using Cities.Api.Models;
using Microsoft.Extensions.Logging; // For logging
using Microsoft.Extensions.Hosting; // For IHostEnvironment

namespace Cities.Api.Services;

/// <summary>
/// Service to retrieve city data from a JSON file.
/// Implements <see cref="ICityDataService"/>.
/// </summary>
public class JsonCityDataService : ICityDataService
{
    private readonly string _jsonFilePath;
    private List<CityData>? _cities; // Cache the data
    private readonly ILogger<JsonCityDataService> _logger;
    private static readonly SemaphoreSlim _fileReadLock = new(1, 1); // To prevent race conditions on first load

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonCityDataService"/> class.
    /// </summary>
    /// <param name="env">The hosting environment, used to find the data file path.</param>
    /// <param name="logger">Logger for logging information and errors.</param>
    public JsonCityDataService(IHostEnvironment env, ILogger<JsonCityDataService> logger)
    {
        // Combine ContentRootPath with the relative path to the JSON file
        _jsonFilePath = Path.Combine(env.ContentRootPath, "Data", "Cities.Api.json");
        _logger = logger;
        _logger.LogInformation("JsonCityDataService initialized. Data path: {FilePath}", _jsonFilePath);
    }

    /// <summary>
    /// Loads city data from the JSON file.
    /// This method is asynchronous and thread-safe for initialization.
    /// </summary>
    private async Task EnsureDataLoadedAsync()
    {
        if (_cities != null && _cities.Any()) return; // Data already loaded

        await _fileReadLock.WaitAsync(); // Acquire lock
        try
        {
            if (_cities != null && _cities.Any()) return; // Double-check after acquiring lock

            _logger.LogInformation("Attempting to load city data from {FilePath}", _jsonFilePath);
            if (!File.Exists(_jsonFilePath))
            {
                _logger.LogError("City data JSON file not found at {FilePath}", _jsonFilePath);
                _cities = new List<CityData>(); // Initialize with empty list to prevent repeated load attempts
                return;
            }

            var jsonString = await File.ReadAllTextAsync(_jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Handles different casing in JSON properties
            };
            _cities = JsonSerializer.Deserialize<List<CityData>>(jsonString, options);
            _logger.LogInformation("Successfully loaded {Count} cities from JSON.", _cities?.Count ?? 0);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing city data from {FilePath}.", _jsonFilePath);
            _cities = new List<CityData>(); // Initialize with empty list on error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while loading city data from {FilePath}.", _jsonFilePath);
            _cities = new List<CityData>(); // Initialize with empty list on error
        }
        finally
        {
            _fileReadLock.Release(); // Release lock
        }
    }

    /// <summary>
    /// Retrieves a city's data by its name using LINQ.
    /// The search is case-insensitive.
    /// </summary>
    /// <param name="cityName">The name of the city to retrieve.</param>
    /// <returns>The <see cref="CityData"/> if found; otherwise, null.</returns>
    public async Task<CityData?> GetCityByNameAsync(string cityName)
    {
        await EnsureDataLoadedAsync();
        if (_cities == null || !_cities.Any())
        {
            _logger.LogWarning("City data is not loaded or is empty when searching for city: {CityName}", cityName);
            return null;
        }

        // Using LINQ's FirstOrDefault for efficient searching.
        // StringComparison.OrdinalIgnoreCase is recommended for case-insensitive comparisons of identifiers.
        var city = _cities.FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase));

        if (city == null)
        {
            _logger.LogInformation("City not found: {CityName}", cityName);
        }
        else
        {
            _logger.LogInformation("City found: {CityName}", cityName);
        }
        return city;
    }

    /// <summary>
    /// Retrieves all Cities.Api.
    /// </summary>
    /// <returns>A list of all <see cref="CityData"/>.</returns>
    public async Task<IEnumerable<CityData>> GetAllCitiesAsync()
    {
        await EnsureDataLoadedAsync();
        return _cities ?? Enumerable.Empty<CityData>();
    }
}
