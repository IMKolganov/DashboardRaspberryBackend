using Microsoft.AspNetCore.Mvc;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : BaseApiController
{
    private readonly string _getTemperatureAndHumidifyUrl;

    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(logger, httpClientFactory, configuration)
    {
        _getTemperatureAndHumidifyUrl = configuration["MicroserviceSettings:GetTemperatureAndHumidifyUrl"];

        if (string.IsNullOrWhiteSpace(_getTemperatureAndHumidifyUrl))
        {
            throw new ArgumentException("Microservice URL is not configured.");
        }
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<IActionResult> GetTemperatureAndHumidify()
    {
        return await CallExternalMicroserviceAsync(_getTemperatureAndHumidifyUrl + "/get-temperature-and-humidify");
    }
}
