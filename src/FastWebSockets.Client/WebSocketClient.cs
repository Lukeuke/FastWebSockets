using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastWebSockets.Client;

public class WebSocketClient : IDisposable
{
    private readonly string _url;
    private readonly ClientWebSocket _webSocket;
    private bool _isConnected;

    /// <summary>
    /// Client instance of websocket
    /// </summary>
    /// <param name="url">Full url of the ws server</param>
    public WebSocketClient(string url)
    {
        _url = url;
        _webSocket = new ClientWebSocket();
    }

    /// <summary>
    /// Connect to the server.
    /// </summary>
    /// <exception cref="InvalidOperationException">When client is already connected.</exception>
    public async Task ConnectAsync()
    {
        if (_isConnected)
            throw new InvalidOperationException("Already connected.");

        await _webSocket.ConnectAsync(new Uri(_url), CancellationToken.None);
        _isConnected = true;
    }

    /// <summary>
    /// Sends message to server.
    /// </summary>
    /// <param name="handler">The server's handler name where the message will be processed.</param>
    /// <param name="message">The message that will be sent</param>
    /// <param name="messageType">WebSocketMessageType</param>
    /// <exception cref="InvalidOperationException">When client is not connected to the server.</exception>
    public async Task SendMessageAsync(string handler, string message, WebSocketMessageType messageType)
    {
        if (!_isConnected)
            throw new InvalidOperationException("WebSocket is not connected.");

        var packet = new WebPacket(handler, message);
        var serialized = JsonSerializer.Serialize(packet);
        
        var messageBytes = Encoding.UTF8.GetBytes(serialized);
        await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), messageType, true, CancellationToken.None);
    }

    /// <summary>
    /// Receive message from server.
    /// </summary>
    /// <param name="messageType">WebSocketMessageType</param>
    /// <returns>Message as string from server</returns>
    /// <exception cref="InvalidOperationException">When message type from result does not match the messageType</exception>
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

    /// <summary>
    /// Closes the connection
    /// </summary>
    /// <param name="reason">Reason description</param>
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