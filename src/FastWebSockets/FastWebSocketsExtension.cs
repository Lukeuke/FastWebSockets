using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FastWebSockets;

public static class FastWebSocketsExtension
{
    public static IServiceCollection AddFastWebSockets(this IServiceCollection collection, uint maxConnections = uint.MaxValue)
    {
        var server = new WebServer(maxConnections);
        collection.AddSingleton(server);
        return collection;
    }

    public static IServiceCollection RegisterHandler<T>(this IServiceCollection collection) where T : SocketHandler
    {
        var serviceProvider = collection.BuildServiceProvider();
        var webServer = serviceProvider.GetService<WebServer>();
        
        if (webServer is null)
        {
            throw new Exception("FastWebSockets WebServer is not registered.");
        }
        
        var handler = Activator.CreateInstance<T>();
        webServer.RegisterHandler(handler);

        return collection;
    }

    public static async Task<IApplicationBuilder?> StartFastWebSocketsAsync(this IApplicationBuilder? app, IPAddress ipAddress, uint port)
    {
        if (app is null)
        {
            throw new Exception("WebApplication was null.");
        }
        
        var webServer = app.ApplicationServices.GetService<WebServer>();
        
        if (webServer is null)
        {
            throw new Exception("FastWebSockets WebServer is not registered.");
        }
        
        await webServer.StartAsync(ipAddress, port);
        
        return app;
    } 
}