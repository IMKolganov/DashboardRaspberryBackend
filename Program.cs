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
builder.Services.AddHttpClient("MicroserviceClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddControllers();

var app = builder.Build();

var environment = app.Environment.EnvironmentName;

// Configure the HTTP request pipeline.
if(environment == "Docker" || app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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


//docker stop dashboard-raspberry-backend-container || true && \
//docker rm dashboard-raspberry-backend-container || true && \
//docker build -t dashboard-raspberry-backend . && \
//docker run -d -p 3001:80--name dashboard-raspberry-backend-container -e ASPNETCORE_ENVIRONMENT=Docker dashboard-raspberry-backend;

