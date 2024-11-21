namespace DeviceProxy;

public class RequestAwaiter
{
    private readonly TaskCompletionSource<ProtoHttpResponse> _tcs = new();
    
    public Task<ProtoHttpResponse> Task => _tcs.Task;

    public RequestAwaiter(Guid requestId, HttpRequest request)
    {
        RequestId = requestId;
        Request = request; 
    }
    
    public HttpRequest Request { get; }
    public Guid RequestId { get; set; }
    
    public void SetResult(ProtoHttpResponse result)
    {
        _tcs.SetResult(result);
    }

    public void SetException(Exception exception)
    {
        _tcs.SetException(exception);
    }

    public void SetCanceled()
    {
        _tcs.SetCanceled();
    }
}