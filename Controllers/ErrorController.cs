using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DashboardRaspberryBackend.Controllers;

[ApiController]
[Route("error")]
public class ErrorController : ControllerBase
{
    [HttpGet] // Добавьте атрибут HTTP метода здесь
    [Route("")]
    public IActionResult HandleError()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context?.Error;

        return Problem(detail: exception?.Message, statusCode: (int)HttpStatusCode.InternalServerError);
    }
}
