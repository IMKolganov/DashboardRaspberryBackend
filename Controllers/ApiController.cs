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
    public async Task<IActionResult> GetTemperatureAndHumidify(int sensorId = 1, bool useRandomValuesFotTest = false)
    {
        var data = await _temperatureService.GetTemperatureAndHumidifyData(sensorId, useRandomValuesFotTest);
        return Ok(data);
    }
    
    [HttpGet("GetSoilMoisture")]
    public async Task<IActionResult> GetSoilMoisture(int sensorId = 0, bool useRandomValuesFotTest = false)
    {
        var data = await _soilMoistureService.GetSoilMoistureData(sensorId, useRandomValuesFotTest);
        return Ok(data);
    }
    
    [HttpPost("StartPum")]
    public async Task<IActionResult> StartPum(int pumpId = 0, int pumpDuration = 5, bool useRandomValuesFotTest = false)
    {
        var data = await _pumpService.StartPum(pumpId, pumpDuration, useRandomValuesFotTest);
        return Ok(data);
    }
}
