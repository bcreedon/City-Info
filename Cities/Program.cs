using Cities.Api.Services;
using Serilog; // For logging

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//Serilog config
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File("log/apiLogs.txt", rollingInterval: RollingInterval.Month)
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

// --- Dependency Injection Setup ---
// Register ICityDataService and its implementation JsonCityDataService.
// AddSingleton is appropriate here because JsonCityDataService loads data once and caches it.
// If data were from a database or changed frequently, AddScoped or AddTransient might be better.
builder.Services.AddSingleton<ICityDataService, JsonCityDataService>();
// --- End Dependency Injection Setup ---

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
