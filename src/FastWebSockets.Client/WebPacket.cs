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
    internal string Handler { get; }
    
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    internal string Message { get; }
}