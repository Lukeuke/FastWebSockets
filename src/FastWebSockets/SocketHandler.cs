using System.Net.WebSockets;

namespace FastWebSockets;

public abstract class SocketHandler
{
    protected WebServer Server { get; private set; }

    public void AttachServer(WebServer server)
    {
        Server = server;
    }
    
    public virtual Task OnConnectedAsync(WebSocket socket, Guid clientId) => Task.CompletedTask;
    public virtual Task OnDisconnectedAsync(WebSocket socket, Guid clientId) => Task.CompletedTask;
    public abstract Task OnMessageReceivedAsync(WebSocket socket, Guid clientId, string message);
}