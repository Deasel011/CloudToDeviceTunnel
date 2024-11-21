using System.Net.WebSockets;
using System.Text;

namespace DeviceClient;

public class RequestTunneler: IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ClientWebSocket _clientWebSocket;

    public RequestTunneler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task Start(string yarpServerUri)
    {
        var serverUri = new Uri("ws://yarp-server/ws");
        await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);

        Console.WriteLine("Connected to YARP server");
        
        var buffer = new byte[4096];
        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var result = await _clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
            var requestJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

            // Parse the HTTP request (pseudo-code for clarity)
            var httpRequest = ParseHttpRequest(requestJson);

            // Forward the request to the target server
            var targetResponse = await ForwardToTargetServer(httpRequest);

            // Send the response back to the YARP server
            var responseJson = SerializeHttpResponse(targetResponse);
            await _clientWebSocket.SendAsync(Encoding.UTF8.GetBytes(responseJson), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_clientWebSocket.State < WebSocketState.Closed)
        {
            _clientWebSocket.Abort();
        }
        _clientWebSocket?.Dispose();
    }
    
    async Task<HttpResponseMessage> ForwardToTargetServer(HttpRequestMessage httpRequest)
    {
        // Update the URL to point to the internal server
        httpRequest.RequestUri = new Uri("http://internal-server:port" + httpRequest.RequestUri.PathAndQuery);
        return await _httpClient.SendAsync(httpRequest);
    }

    HttpRequestMessage ParseHttpRequest(string requestJson)
    {
        // Deserialize the HTTP request JSON into HttpRequestMessage
        // Example: Use Newtonsoft.Json or System.Text.Json
        return new HttpRequestMessage(); // Replace with actual parsing logic
    }

    string SerializeHttpResponse(HttpResponseMessage response)
    {
        // Serialize the HttpResponseMessage to JSON
        // Example: Use Newtonsoft.Json or System.Text.Json
        return ""; // Replace with actual serialization logic
    }
}