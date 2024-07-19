using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseApiController : ControllerBase
{
    protected readonly ILogger<BaseApiController> _logger;
    protected readonly HttpClient _httpClient;
    protected readonly IConfiguration _configuration;

    public BaseApiController(ILogger<BaseApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("MicroserviceClient");
        _configuration = configuration;
    }

    protected async Task<IActionResult> CallExternalMicroserviceAsync(string url)
    {
        try
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                url += "?isDev=1";
            }

            _logger.LogInformation("Calling external microservice at {url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            return new JsonResult(responseData);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = ex.Message;
            _logger.LogError(ex, "An error occurred while calling the external microservice. Message: {Message}", errorMessage);
            return StatusCode(500, new { error = "Failed to call the external microservice", details = errorMessage });
        }
    }

    [HttpGet("healthcheck")]
    public IActionResult HealthCheck()
    {
        _logger.LogInformation("Health check endpoint called in {controller}", GetType().Name);
        return Ok(new { status = "ok", controller = GetType().Name });
    }
}
