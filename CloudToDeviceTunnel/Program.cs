using System.Collections.Concurrent;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<DualIndexMapping<ConnectionId, DeviceId>>();
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
var app = builder.Build();

ConcurrentDictionary<string,DeviceWebSocketHandler> deviceWebsocketHandlers = new();

app.UseWebSockets();

app.Map("/device/{deviceId}/wstunnel", async (HttpContext context, string deviceId) =>
{
    if (deviceWebsocketHandlers.ContainsKey(deviceId))
    {
        await deviceWebsocketHandlers[deviceId].CloseConnection();
    }
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = new DeviceWebSocketHandler(webSocket, deviceId);
        await handler.HandleConnection();
        deviceWebsocketHandlers.TryAdd(deviceId, handler);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapHub<DeviceHub>("/deviceHub");

app.Map("/device/{deviceId}/tunnel/{*catchall}", async (HttpContext context, string deviceId, IHubContext<DeviceHub> deviceHubContext, DualIndexMapping<ConnectionId, DeviceId> deviceToConnectionIds, CancellationToken ct = default) =>
{
    deviceToConnectionIds.TryGetValue(new DeviceId(deviceId), out var connectionId);
    await deviceHubContext.Clients.Client(connectionId).InvokeAsync<bool>("Test", ct);
    await deviceHubContext.Clients.Client(connectionId).InvokeAsync<bool>("Test2", "hello", ct);
    ProtoHttpRequest protoHttpRequest = await context.Request.ToLocalProtoHttpRequestAsync();
    var protoHttpResponseBytes = await deviceHubContext.Clients.Client(connectionId)
        .InvokeAsync<byte[]>("ProtoHttpRequest", protoHttpRequest.ToByteArray(), ct);
    var protoHttpResponse = ProtoHttpResponse.Parser.ParseFrom(protoHttpResponseBytes);
    return Results.Content(protoHttpResponse.Body, protoHttpResponse.Headers.FirstOrDefault(x => x.Key == "Content-Type")?.Value ?? "text/html", contentEncoding: null, protoHttpResponse.StatusCode);
});

app.Run();
