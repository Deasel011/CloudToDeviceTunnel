// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Booting up SignalR Client to CloudToDevice Tunnel...");

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5192/deviceHub", options =>
    {
        options.Headers.Add("DeviceId", "1");
    })
    .WithAutomaticReconnect()
    .Build();

var localHttpClient = new HttpClient(){BaseAddress = new Uri("http://localhost:5164")};

connection.On<byte[],byte[]>("ProtoHttpRequest", async (message) =>
{
    var protoHttpRequest = ProtoHttpRequest.Parser.ParseFrom(message);
    var response = await localHttpClient.SendAsync(await protoHttpRequest.FromProtoHttpRequestAsync());
    var responseBody = await response.Content.ReadAsStringAsync();
    var protoHttpResponse = new ProtoHttpResponse
    {
        StatusCode = (int)response.StatusCode,
        Body = responseBody,
        Headers = {response.Headers.Select(header => new Header {Key = header.Key, Value = header.Value.First()})}
    };
    return protoHttpResponse.ToByteArray();
});

// connection.On<string, string>("ProxyRequest", async (message) => 
// {
    // var serializableHttpRequestMessage = JsonSerializer.Deserialize<SerializableHttpRequestMessage>(message);
    // var response = await localHttpClient.SendAsync(serializableHttpRequestMessage.ToHttpRequestMessage());
    // return JsonSerializer.Serialize(new SerializableHttpResponseMessage(response));
// });

connection.On<bool>("Test", async () =>
{
    Console.WriteLine("Test invoked.");
    return true;
});

connection.On<string, bool>("Test2", async (message) =>
{
    Console.WriteLine($"Test invoked with message: {message}");
    return true;
});


Task<bool> StartConnection()
{
    return connection.StartAsync().ContinueWith(task =>
    {
        if (task.IsFaulted)
        {
            Console.WriteLine("There was an error opening the connection: {0}", task.Exception.GetBaseException());
            return false;
        }
        else
        {
            Console.WriteLine("Connected to CloudToDevice Tunnel.");
            return true;
        }
    });
}


do
{
    Thread.Sleep(1000);
} while(!StartConnection().Result);


Console.WriteLine("SignalR Client started. Press any key to exit.");
Console.ReadKey();


public static class HttpRequestExtensions
{
    public static async Task<HttpRequestMessage> FromProtoHttpRequestAsync(this ProtoHttpRequest protoHttpRequest)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(protoHttpRequest.Method),
            RequestUri = new Uri(protoHttpRequest.PathAndQuery, UriKind.Relative),
            Content = new StringContent(protoHttpRequest.Body)
        };
        foreach (var header in protoHttpRequest.Headers)
        {
            httpRequestMessage.Headers.Add(header.Key, header.Value);
        }
        return httpRequestMessage;
    }
}