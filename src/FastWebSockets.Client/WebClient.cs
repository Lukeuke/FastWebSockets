using System.Net.WebSockets;
using System.Text;

namespace FastWebSockets.Client;

public class WebSocketClient : IDisposable
{
    private readonly string _url;
    private readonly ClientWebSocket _webSocket;
    private bool _isConnected;

    public WebSocketClient(string url)
    {
        _url = url;
        _webSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync()
    {
        if (_isConnected)
            throw new InvalidOperationException("Already connected.");

        await _webSocket.ConnectAsync(new Uri(_url), CancellationToken.None);
        _isConnected = true;
    }

    public async Task SendMessageAsync(string message, WebSocketMessageType messageType)
    {
        if (!_isConnected)
            throw new InvalidOperationException("WebSocket is not connected.");

        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), messageType, true, CancellationToken.None);
    }

    public async Task<string> ReceiveMessageAsync(WebSocketMessageType messageType)
    {
        if (!_isConnected)
            throw new InvalidOperationException("WebSocket is not connected.");

        var buffer = new byte[1024];
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == messageType)
        {
            if (messageType == WebSocketMessageType.Text)
            {
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                return receivedMessage;
            }

            if (messageType == WebSocketMessageType.Binary)
            {
                var receivedMessage = BitConverter.ToString(buffer, 0, result.Count);
                return receivedMessage;
            }
        }
        else if (result.MessageType == WebSocketMessageType.Close)
        {
            await CloseAsync("Closed by server.");
        }
        else
        {
            Console.WriteLine($"Unexpected message type received: {result.MessageType}");
        }

        return null;
    }

    public async Task CloseAsync(string reason)
    {
        if (_isConnected)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
            _isConnected = false;
            Console.WriteLine($"WebSocket closed: {reason}");
        }
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}