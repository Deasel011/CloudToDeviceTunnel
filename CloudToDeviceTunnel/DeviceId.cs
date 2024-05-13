public record DeviceId
{
    private readonly string _deviceId;

    public DeviceId(string deviceId)
    {
        _deviceId = deviceId;
    }
    
    public static implicit operator string(DeviceId deviceId) => deviceId._deviceId;
    
    public static implicit operator DeviceId(string deviceId) => new(deviceId);
    
    public override string ToString() => _deviceId;
    
    public override int GetHashCode() => _deviceId.GetHashCode();
}