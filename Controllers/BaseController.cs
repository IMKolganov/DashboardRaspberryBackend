using Microsoft.AspNetCore.Mvc;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseApiController : ControllerBase
{
    protected readonly ILogger<BaseApiController> _logger;
    protected readonly IHttpClientFactory _httpClientFactory;
    protected readonly IConfiguration _configuration;

    public BaseApiController(ILogger<BaseApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected async Task<IActionResult> CallExternalMicroserviceAsync(HttpClient client, string url)
    {
        try
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                url += "?isDev=1";
            }

            _logger.LogInformation("Calling external microservice at {url}", url);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            return new JsonResult(responseData);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = ex.Message;
            _logger.LogError(ex, "An error occurred while calling the external microservice. Message: {Message}", errorMessage);
            return StatusCode(500, new { error = $"Failed to call the external microservice. url: {url}", details = errorMessage });
        }
    }
}
