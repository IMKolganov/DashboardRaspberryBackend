using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using DashboardRaspberryBackend.Models;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _getTemperatureAndHumidifyUrl;

    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("MicroserviceClient");
        _getTemperatureAndHumidifyUrl = configuration["MicroserviceSettings:GetTemperatureAndHumidifyUrl"];

        if (string.IsNullOrWhiteSpace(_getTemperatureAndHumidifyUrl))
        {
            throw new ArgumentException("Microservice URL is not configured.");
        }
    }

    [HttpGet("healthcheck")]
    public IActionResult HealthCheck()
    {
        _logger.LogInformation("Health check endpoint called");
        return Ok(new { status = "ok", controller = GetType().Name });
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<ActionResult<GetTemperatureAndHumidifyResponse>> GetTemperatureAndHumidify()
    {
        try
        {
            var url = _getTemperatureAndHumidifyUrl + "/get-temperature-and-humidify";
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                url += "?isDev=1";
            }

            _logger.LogInformation("Calling external microservice at {url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GetTemperatureAndHumidifyResponse>(responseData);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = ex.Message;
            _logger.LogError(ex, "An error occurred while calling the external microservice. Message: {Message}", errorMessage);
            var errorResponse = new ErrorResponse { Error = "Failed to call the external microservice", Details = errorMessage };
            return StatusCode(500, errorResponse);
        }
    }
}