using System.Net.Sockets;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace DashboardRaspberryBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            _logger.LogInformation("Health check endpoint called");
            return Ok(new { status = "ok" });
        }
    }
}