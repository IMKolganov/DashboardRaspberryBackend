using Microsoft.AspNetCore.Mvc;
using DashboardRaspberryBackend.Services.Interfaces;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : BaseApiController
{
    private readonly ITemperatureService _temperatureService;
    private readonly ISoilMoistureService _soilMoistureService;

    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, ITemperatureService temperatureService,
        ISoilMoistureService soilMoistureService)
        : base(logger, httpClientFactory, configuration)
    {
        _temperatureService = temperatureService;
        _soilMoistureService = soilMoistureService;
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<IActionResult> GetTemperatureAndHumidify()
    {
        var data = await _temperatureService.GetTemperatureAndHumidifyData();
        return Ok(data);
    }
    
    [HttpGet("GetSoilMoisture")]
    public async Task<IActionResult> GetSoilMoisture(int sensorId = 0, bool withoutMSMicrocontrollerManager = false)
    {
        var data = await _soilMoistureService.GetSoilMoistureData(sensorId, withoutMSMicrocontrollerManager);
        return Ok(data);
    }
}
