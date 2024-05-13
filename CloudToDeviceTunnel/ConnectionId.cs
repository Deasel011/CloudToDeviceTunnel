public record ConnectionId
{
    private readonly string _connectionId;

    public ConnectionId(string connectionId)
    {
        _connectionId = connectionId;
    }
    
    public static implicit operator string(ConnectionId connectionId) => connectionId._connectionId;
    
    public static implicit operator ConnectionId(string connectionId) => new(connectionId);
    
    public override string ToString() => _connectionId;
    
    public override int GetHashCode() => _connectionId.GetHashCode();
}