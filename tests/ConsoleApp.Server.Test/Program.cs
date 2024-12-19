using System.Net;
using ConsoleApp.Test;

var server = new WebServer(100);
server.RegisterHandler(new EchoHandler());

await server.StartAsync(IPAddress.Loopback, 8080);