using Cities.Api.Models;

namespace Cities.Api.Services;

/// <summary>
/// Interface for accessing city data.
/// </summary>
public interface ICityDataService
{
    /// <summary>
    /// Retrieves a city's data by its name.
    /// The search is case-insensitive.
    /// </summary>
    /// <param name="cityName">The name of the city to retrieve.</param>
    /// <returns>The <see cref="CityData"/> if found; otherwise, null.</returns>
    Task<CityData?> GetCityByNameAsync(string cityName);

    /// <summary>
    /// Retrieves all Cities.Api.
    /// </summary>
    /// <returns>A list of all <see cref="CityData"/>.</returns>
    Task<IEnumerable<CityData>> GetAllCitiesAsync();
}
