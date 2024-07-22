using Microsoft.AspNetCore.Mvc;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : BaseApiController
{
    private readonly string _msTemperatureAndHumidifyUrl;
    private readonly string _msSoilMoistureUrl;


    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(logger, httpClientFactory, configuration)
    {
        _msTemperatureAndHumidifyUrl = configuration["MicroserviceSettings:GetTemperatureAndHumidifyUrl"];
        _msSoilMoistureUrl = configuration["MicroserviceSettings:GetSoilMoistureUrl"];

        if (string.IsNullOrWhiteSpace(_msTemperatureAndHumidifyUrl))
        {
            throw new ArgumentException("Microservice GetTemperatureAndHumidifyUrl is not configured.");
        }

        _msSoilMoistureUrl = configuration["MicroserviceSettings:GetSoilMoistureUrl"];

        if (string.IsNullOrWhiteSpace(_msSoilMoistureUrl))
        {
            throw new ArgumentException("Microservice GetSoilMoistureUrl is not configured.");
        }
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<IActionResult> GetTemperatureAndHumidify()
    {
        return await CallExternalMicroserviceAsync(_msTemperatureAndHumidifyUrl + "/get-temperature-and-humidify");
    }

    [HttpGet("GetSoilMoisture")]
    public async Task<IActionResult> GetSoilMoisture()
    {
        return await CallExternalMicroserviceAsync(_msSoilMoistureUrl + "/get-soil-moisture");
    }
}
