using System.Net.WebSockets;
using System.Text;
using FastWebSockets;

namespace ConsoleApp.Test;

public class EchoHandler : SocketHandler
{
    public override async Task OnMessageReceivedAsync(WebSocket socket, Guid clientId, string message)
    {
        var bytes = Encoding.UTF8.GetBytes($"Echo from {clientId}: {message}");
        await Server.UnicastMessageAsync(socket, bytes, WebSocketMessageType.Text);
    }
}