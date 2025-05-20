# US City Info API (.NET 8 Demo)

This project is a demonstration API built with .NET 8 that provides information about major U.S. cities. Data is sourced from an internal JSON file.

This project showcases several modern .NET development practices and features, making it a good example for understanding API development in the .NET ecosystem.

## Features

* **ASP.NET Core Web API:** Built using the latest .NET 8 framework for creating robust APIs.
* **RESTful Endpoints:**
    * `GET /api/cities`: Returns a list of all available cities (ID, Name, State).
    * `GET /api/cities/{cityName}`: Returns detailed information for a specific city.
* **JSON Data Source:** City data is stored in `USCityInfo.Api/Data/cities.Api.json` and loaded at runtime.
* **Dependency Injection (DI):** Utilizes .NET's built-in DI container to manage services like `ICityDataService`.
    * `ICityDataService` is registered as a singleton in `Program.cs`:
        ```csharp
        builder.Services.AddSingleton<ICityDataService, JsonCityDataService>();
        ```
    * And injected into controllers:
        ```csharp
        public CitiesController(ICityDataService cityDataService, ILogger<CitiesController> logger)
        {
            _cityDataService = cityDataService;
            _logger = logger;
        }
        ```
* **LINQ to Objects:** Used in `JsonCityDataService` to query the in-memory list of cities efficiently.
    ```csharp
    // Example from JsonCityDataService.cs
    var city = _cities.FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase));
    ```
* **Asynchronous Programming (`async`/`await`):** All I/O operations (file reading) and controller actions are asynchronous for better performance and scalability.
* **C# Records:** Used for immutable Data Transfer Objects (DTOs) and internal models (`CityData.cs`, `CityInfoDto.cs`).
* **Swagger/OpenAPI Documentation:** Integrated Swagger UI for easy API exploration and testing. Accessible at the application root (`/`) when running locally.
* **.http File:** Includes `city_info_api.http` for quick testing with tools like VS Code's REST Client extension.
* **xUnit for Unit Testing:** Comprehensive unit tests for services and controllers, aiming for high code coverage.
* **Logging:** Uses `Microsoft.Extensions.Logging` for application logging. Configured in `Program.cs`.
* **Configuration:** Uses `IHostEnvironment` to locate the data file relative to the content root.
* **Error Handling:** Basic global error handling and specific `NotFound` / `BadRequest` responses.
* **Current Local Time Calculation:** The API calculates and returns the current local time for the requested city based on its timezone.
* **Nullable Reference Types:** Enabled project-wide for improved null safety.
* **GitHub Actions CI:** A basic continuous integration workflow (`.github/workflows/dotnet.yml`) that builds and tests the project on every push/pull request.

## Prerequisites

* .NET 8 SDK (or newer)
* An IDE like Visual Studio 22 or VS Code

## How to Run

1.  **Clone the repository:**
    ```bash
    git clone <your-repo-url>
    cd CityInfo
    ```
2.  **Navigate to the API project directory:**
    ```bash
    cd CityInfo.Api
    ```
3.  **Run the API:**
    ```bash
    dotnet run
    ```
    The API will typically be available at `https://localhost:7XXX` or `http://localhost:5XXX`. Check the console output for the exact URLs.
4.  **Access Swagger UI:** Open your browser and navigate to the root URL (e.g., `https://localhost:7231/`).

## How to Test (Unit Tests)

1.  **Navigate to the solution root directory:**
    ```bash
    cd <path-to-solution-root>/USCityInfo
    ```
2.  **Run tests:**
    ```bash
    dotnet test
    ```
    This will execute all xUnit tests in the `CityInfo.Tests` project.

## API Endpoints

### Get All Cities

* **URL:** `/api/cities`
* **Method:** `GET`
* **Description:** Retrieves a summary list of all available cities.
* **Success Response (200 OK):**
    ```json
    [
      { "id": "nyc", "name": "New York City", "state": "New York" },
      { "id": "lax", "name": "Los Angeles", "state": "California" }
      // ... more cities
    ]
    ```

### Get City Information by Name

* **URL:** `/api/cities/{cityName}`
* **Method:** `GET`
* **Example URL:** `/api/cities/New%20York%20City`
* **Description:** Retrieves detailed information for the specified city.
* **Success Response (200 OK):**
    ```json
    {
      "name": "New York City",
      "state": "New York",
      "temperatures": {
        "summerHighFahrenheit": "85 °F",
        "winterLowFahrenheit": "26 °F"
      },
      "elevation": "33 ft",
      "population": 8335897,
      "currentTimeLocal": "2024-05-07 14:30:00 -04:00" // Example format
    }
    ```
* **Error Response (404 Not Found):**
    ```json
    // (Content-Type: application/problem+json)
    {
        "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
        "title": "Not Found",
        "status": 404,
        "detail": "Information for city 'NonExistentCity' not found.",
        "traceId": "..."
    }
    // Or a simple string if not using ProblemDetails for all errors
    "Information for city 'NonExistentCity' not found."
    ```

## .NET features demonstrated in this project

This project aims to demonstrate proficiency in several areas critical for modern software development:

* **Clean Architecture Principles:** Separation of concerns with distinct layers (API/Controllers, Services, Models).
* **Testability:** Designing code (especially services) to be easily unit-testable. High code coverage using xUnit.
* **Modern C# and .NET Features:** Utilizing .NET 8, records, `async/await`, LINQ, nullable reference types.
* **Best Practices:** Dependency Injection, structured logging, clear API design with Swagger, proper error handling.
* **Clear Documentation:** This README, along with XML comments in code for Swagger, helps with achieving maintainable and understandable code.

## Potential Future Enhancements

* More sophisticated error handling middleware.
* Input validation for parameters.
* Caching strategies beyond the initial file load.
* Integration with a real database.
* Adding PUT/POST/DELETE endpoints for managing city data (with appropriate security).
* API Versioning.
* Add `Dockerfile` for containerization
