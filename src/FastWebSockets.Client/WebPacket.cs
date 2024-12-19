using System.Text.Json;

namespace FastWebSockets.Client;

/// <summary>
/// Represents packet being sent to server
/// </summary>
internal class WebPacket
{
    internal WebPacket(string handler, string message)
    {
        Handler = handler;
        Message = message;
    }
    
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Handler { get; }
    
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Message { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}