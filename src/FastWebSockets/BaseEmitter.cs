using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace FastWebSockets;

public class BaseEmitter
{
    private readonly ConcurrentDictionary<WebSocket, Guid> _clients;

    protected BaseEmitter(ConcurrentDictionary<WebSocket, Guid> clients)
    {
        _clients = clients;
    }

    public async Task BroadcastMessageAsync(byte[] messageBuffer, WebSocketMessageType messageType)
    {
        foreach (var client in _clients.Keys)
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(messageBuffer), messageType, true, CancellationToken.None);
            }
        }
    }

    public async Task UnicastMessageAsync(Guid userId, byte[] messageBuffer, WebSocketMessageType messageType)
    {
        var (user, id) = _clients.First(x => x.Value == userId);
        if (user.State == WebSocketState.Open)
        {
            await user.SendAsync(new ArraySegment<byte>(messageBuffer), messageType, true, CancellationToken.None);
        }
    }

    public async Task UnicastMessageAsync(WebSocket connection, byte[] messageBuffer, WebSocketMessageType messageType)
    {
        var (user, id) = _clients.First(x => x.Key == connection);
        if (user.State == WebSocketState.Open)
        {
            await user.SendAsync(new ArraySegment<byte>(messageBuffer), messageType, true, CancellationToken.None);
        }
    }
    
    public async Task MulticastMessageAsync(IEnumerable<Guid> userIds, byte[] messageBuffer, WebSocketMessageType messageType)
    {
        foreach (var userId in userIds)
        {
            if (!userIds.Contains(userId)) continue;
        
            var (user, id) = _clients.First(x => x.Value == userId);
            if (user.State == WebSocketState.Open)
            {
                await user.SendAsync(new ArraySegment<byte>(messageBuffer), messageType, true, CancellationToken.None);
            }
        }
    }
    
    public async Task MulticastMessageAsync(IEnumerable<WebSocket> connections, byte[] messageBuffer, WebSocketMessageType messageType)
    {
        foreach (var connection in connections)
        {
            if (!connections.Contains(connection)) continue;

            var (user, id) = _clients.First(x => x.Key == connection);
            if (user.State == WebSocketState.Open)
            {
                await user.SendAsync(new ArraySegment<byte>(messageBuffer), messageType, true, CancellationToken.None);
            }
        }
    }
}