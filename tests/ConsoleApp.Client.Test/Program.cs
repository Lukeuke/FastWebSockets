using System.Net.WebSockets;
using FastWebSockets.Client;

using var client = new WebSocketClient("ws://127.0.0.1:8080");
try
{
    await client.ConnectAsync();

    await client.SendMessageAsync("EchoHandler", "Hello, world!", WebSocketMessageType.Text);

    var response = await client.ReceiveMessageAsync(WebSocketMessageType.Text);
    Console.WriteLine($"Received from server: {response}");

    await client.CloseAsync("Client closing.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}