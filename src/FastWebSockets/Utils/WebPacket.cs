namespace FastWebSockets.Utils;

public class WebPacket
{
    public WebPacket(string handler, string message)
    {
        Handler = handler;
        Message = message;
    }
    
    public string Handler { get; }
    public string Message { get; }
}