﻿using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class TemperatureResponse: IRabbitMqResponse
{
    public Guid RequestId { get; set; }
    public string MethodName { get; set; } = null!;
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public DateTime CreateDate { get; set; }
    public string? ErrorMessage { get; set; }
}
