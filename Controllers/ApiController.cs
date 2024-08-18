using Microsoft.AspNetCore.Mvc;
using DashboardRaspberryBackend.Services;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : BaseApiController
{
    private readonly TemperatureService _temperatureService;

    public ApiController(ILogger<ApiController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, TemperatureService temperatureService)
        : base(logger, httpClientFactory, configuration)
    {
        _temperatureService = temperatureService;
    }

    [HttpGet("GetTemperatureAndHumidify")]
    public async Task<IActionResult> GetTemperatureAndHumidify()
    {
        var data = await _temperatureService.GetTemperatureAndHumidifyData();
        return Ok(data);
    }
}
