using DeviceClient;
using DeviceProxy;

var builder = WebApplication.CreateSlimBuilder(args);

// Add services
builder.Services.AddSingleton<OngoingRequests>();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

app.Map("/{serialNumber}/{**catch-all}", RequestDelegates.ExternalRequestDelegate);

app.Map("/push-channel/{serialNumber}", RequestDelegates.PushChannelRequestDelegate);

app.MapPost("/response/{serialNumber}", RequestDelegates.ResponseRequestDelegate);

app.Run();


