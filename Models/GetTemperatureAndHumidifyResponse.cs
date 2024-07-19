using System;
namespace DashboardRaspberryBackend.Models;

public class GetTemperatureAndHumidifyResponse
{
    public WebserviceData WebserviceData { get; set; }
    public MicrocontrollerData MicrocontrollerData { get; set; }
}
