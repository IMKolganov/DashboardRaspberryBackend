using Microsoft.AspNetCore.Mvc;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : BaseApiController
{
    private readonly string _getTemperatureAndHumidifyUrl;
    private readonly string _getSoilMoistureUrl;


    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(logger, httpClientFactory, configuration)
    {
        _getTemperatureAndHumidifyUrl = configuration["MicroserviceSettings:GetTemperatureAndHumidifyUrl"];
        _getSoilMoistureUrl = configuration["MicroserviceSettings:GetSoilMoistureUrl"];

        if (string.IsNullOrWhiteSpace(_getTemperatureAndHumidifyUrl))
        {
            throw new ArgumentException("Microservice GetTemperatureAndHumidifyUrl is not configured.");
        }

        _getSoilMoistureUrl = configuration["MicroserviceSettings:GetSoilMoistureUrl"];

        if (string.IsNullOrWhiteSpace(_getSoilMoistureUrl))
        {
            throw new ArgumentException("Microservice GetSoilMoistureUrl is not configured.");
        }
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<IActionResult> GetTemperatureAndHumidify()
    {
        return await CallExternalMicroserviceAsync(_getTemperatureAndHumidifyUrl + "/get-temperature-and-humidify");
    }

    [HttpGet("GetSoilMoisture")]
    public async Task<IActionResult> GetSoilMoisture()
    {
        return await CallExternalMicroserviceAsync(_getSoilMoistureUrl + "/get-soil-moisture");
    }
}
