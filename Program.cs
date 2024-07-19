var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Redirect the root URL to /swagger
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();


//docker stop dashboard-raspberry-backend-container || true && \
//docker rm dashboard-raspberry-backend-container || true && \
//docker build -t dashboard-raspberry-backend . && \
//docker run -d -p 3001:80 --name dashboard-raspberry-backend-container -e ASPNETCORE_ENVIRONMENT=Development dashboard-raspberry-backend;