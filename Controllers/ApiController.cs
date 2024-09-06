using Microsoft.AspNetCore.Mvc;
using DashboardRaspberryBackend.Services.Interfaces;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : BaseApiController
{
    private readonly ITemperatureService _temperatureService;
    private readonly ISoilMoistureService _soilMoistureService;
    private readonly IPumpService _pumpService;


    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, ITemperatureService temperatureService,
        ISoilMoistureService soilMoistureService, IPumpService pumpService)
        : base(logger, httpClientFactory, configuration)
    {
        _temperatureService = temperatureService;
        _soilMoistureService = soilMoistureService;
        _pumpService = pumpService;
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<IActionResult> GetTemperatureAndHumidify(bool withoutMSMicrocontrollerManager = false)
    {
        var data = await _temperatureService.GetTemperatureAndHumidifyData(withoutMSMicrocontrollerManager);
        return Ok(data);
    }
    
    [HttpGet("GetSoilMoisture")]
    public async Task<IActionResult> GetSoilMoisture(int sensorId = 0, bool withoutMSMicrocontrollerManager = false)
    {
        var data = await _soilMoistureService.GetSoilMoistureData(sensorId, withoutMSMicrocontrollerManager);
        return Ok(data);
    }
    
    [HttpPost("StartPum")]
    public async Task<IActionResult> StartPum(int pumpId = 0, int seconds = 5, bool withoutMSMicrocontrollerManager = false)
    {
        var data = await _pumpService.StartPum(pumpId, seconds, withoutMSMicrocontrollerManager);
        return Ok(data);
    }
}
