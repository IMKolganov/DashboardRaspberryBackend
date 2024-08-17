using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Middleware.Extensions;
using DashboardRaspberryBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient("TemperatureAndHumidifyService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MicroserviceSettings:GetTemperatureAndHumidifyUrl"]);
});
builder.Services.AddHttpClient("SoilMoistureService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MicroserviceSettings:GetSoilMoistureUrl"]);
});


// Register services
builder.Services.AddSingleton<RabbitMqProducer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var requestQueueNames = configuration.GetSection("RabbitMqSettings:RequestQueues").Get<List<string>>();
    return new RabbitMqProducer(requestQueueNames);
});
builder.Services.AddSingleton<RabbitMqConsumer<TemperatureResponse>>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var responseQueueNames = configuration.GetSection("RabbitMqSettings:ResponseQueues").Get<List<string>>();
    return new RabbitMqConsumer<TemperatureResponse>(responseQueueNames);
});
builder.Services.AddScoped<TemperatureService>();

// Register HttpClientFactory
// builder.Services.AddHttpClient();

var app = builder.Build();

var environment = app.Environment.EnvironmentName;

// Configure the HTTP request pipeline.
if(environment == "Docker" || app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();//todo: remove for docker
    app.UseGlobalExceptionHandler();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}
else
{
    app.UseGlobalExceptionHandler();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();