using Microsoft.AspNetCore.SignalR;

public class DeviceHub : Hub
{
    private readonly DualIndexMapping<ConnectionId, DeviceId> _connectionToDeviceIds;

    public DeviceHub(DualIndexMapping<ConnectionId, DeviceId> connectionToDeviceIds)
    {
        _connectionToDeviceIds = connectionToDeviceIds;
    }
    
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Device connected: {Context.ConnectionId}");
        _connectionToDeviceIds.Add(new DeviceId(Context.GetHttpContext().Request.Headers["DeviceId"]), new ConnectionId(Context.ConnectionId));
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Device disconnected: {Context.ConnectionId}");
        _connectionToDeviceIds.Remove(new DeviceId(Context.GetHttpContext().Request.Headers["DeviceId"]));
        return base.OnDisconnectedAsync(exception);
    }
}