using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using FastWebSockets;
using FastWebSockets.Utils;

public class WebServer : BaseEmitter
{
    private static readonly ConcurrentDictionary<WebSocket, Guid> Clients = new();
    private readonly List<SocketHandler> _handlers = new();

    public uint MaxConnections { get; }

    public WebServer(uint maxConnections) : base(Clients)
    {
        MaxConnections = maxConnections;
    }

    public void RegisterHandler(SocketHandler handler)
    {
        handler.AttachServer(this);
        _handlers.Add(handler);
    }

    public async Task StartAsync(IPAddress ipAddress, uint port)
    {
        var addr = $"{ipAddress}:{port}";
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://{addr}/");
        httpListener.Start();
        Console.WriteLine($"WebSocket server started at ws://{addr} with max connections {MaxConnections}");

        while (true)
        {
            var context = await httpListener.GetContextAsync();

            if (context.Request.IsWebSocketRequest)
            {
                if (Clients.Count >= MaxConnections)
                {
                    Console.WriteLine("Maximum connections limit reached. Rejecting connection.");
                    context.Response.StatusCode = 400;
                    await using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        await writer.WriteAsync("Server is full. Try again later.");
                    }
                    context.Response.Close();
                    continue;
                }
                
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                var clientId = Guid.NewGuid();
                Console.WriteLine("New WebSocket connection established.");

                Clients.TryAdd(webSocket, clientId);

                foreach (var handler in _handlers)
                {
                    await handler.OnConnectedAsync(webSocket, clientId);
                }

                _ = HandleWebSocketAsync(webSocket, clientId);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task HandleWebSocketAsync(WebSocket webSocket, Guid clientId)
    {
        var buffer = new byte[1024];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket connection closing.");
                    Clients.TryRemove(webSocket, out _);

                    foreach (var handler in _handlers)
                    {
                        await handler.OnDisconnectedAsync(webSocket, clientId);
                    }

                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var packet = JsonSerializer.Deserialize<WebPacket>(receivedMessage);

                    if (packet is null)
                    {
                        throw new SerializationException("Failed to deserialize web packet.");
                    }
                    
                    var handler = _handlers.FirstOrDefault(x => x.GetType().Name == packet.Handler);

                    if (handler is null)
                    {
                        throw new Exception($"Cannot find handler of type {packet.Handler}");
                    }

                    await handler.OnMessageReceivedAsync(webSocket, clientId, packet.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            if (webSocket.State != WebSocketState.Closed)
            {
                Clients.TryRemove(webSocket, out _);

                foreach (var handler in _handlers)
                {
                    await handler.OnDisconnectedAsync(webSocket, clientId);
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
            }
        }
    }
}
