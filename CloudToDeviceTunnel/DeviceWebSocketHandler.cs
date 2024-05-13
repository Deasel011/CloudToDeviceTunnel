using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DeviceWebSocketHandler
{
    private readonly WebSocket _webSocket;
    private readonly string _deviceId;

    public DeviceWebSocketHandler(WebSocket webSocket, string deviceId)
    {
        _webSocket = webSocket;
        _deviceId = deviceId;
    }

    public async Task HandleConnection()
    {
        while (_webSocket.State == WebSocketState.Open)
        {
            var message = await ReceiveMessage();
            // Handle received message stored in 'message'
        }
    }

    private async Task<string> ReceiveMessage()
    {
        var buffer = new byte[1024 * 4];
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return message;
    }

    public async Task CloseConnection()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    public HttpResponseMessage ForwardHttpRequest(HttpRequest request)
    {
        var jsonHttpRequest = JsonSerializer.Serialize(request.ToHttpRequestMessage());
        _webSocket.SendAsync(Encoding.UTF8.GetBytes(jsonHttpRequest), WebSocketMessageType.Text, true, CancellationToken.None);
        // _webSocket.ReceiveAsync()
        return new HttpResponseMessage(HttpStatusCode.OK);
    }
}