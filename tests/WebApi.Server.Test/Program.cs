using System.Net;
using FastWebSockets;
using WebApi.Server.Test;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FastWebSockets with Handlers
builder.Services.AddFastWebSockets()
    .RegisterHandler<EchoHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

// Ip and port from appsettings
var ipAddressConfig = builder.Configuration.GetValue<string>("WebSocketServer:IpAddress");
var portConfig = builder.Configuration.GetValue<uint>("WebSocketServer:Port");

if (!IPAddress.TryParse(ipAddressConfig, out var ipAddress) || portConfig == 0)
{
    throw new Exception("Invalid WebSocket Server IP address or port configuration.");
}

// start the FastWebSockets
_ = app.StartFastWebSocketsAsync(ipAddress, portConfig);
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}