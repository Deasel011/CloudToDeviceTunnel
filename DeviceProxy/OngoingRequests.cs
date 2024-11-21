using Microsoft.Extensions.Caching.Memory;

namespace DeviceProxy;

public class OngoingRequests
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<OngoingRequests> _logger;

    public OngoingRequests(IMemoryCache memoryCache, ILogger<OngoingRequests> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }
    
    public void AddRequest(RequestAwaiter requestAwaiter)
    {
        _memoryCache.Set(requestAwaiter.RequestId, requestAwaiter, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2),
            PostEvictionCallbacks = {
                CancelRequestAwaiter
            }
        });
    }

    private PostEvictionCallbackRegistration CancelRequestAwaiter => new() {
        EvictionCallback = (key, value, reason, state) =>
        {
            if (reason > EvictionReason.Replaced)
            {
                _logger.LogWarning("Request {Key} was evicted from the cache with reason {Reason}", key, reason);
                if (value is RequestAwaiter requestAwaiter)
                {
                    _logger.LogWarning("Canceling request {RequestId}", requestAwaiter.RequestId);
                    requestAwaiter.SetCanceled();
                }
            }
        }
    };

    public RequestAwaiter? GetRequest(Guid requestId, bool remove = false)
    {
        try
        {
            _memoryCache.TryGetValue(requestId, out RequestAwaiter? requestAwaiter);
            return requestAwaiter;
        }finally
        {
            if(remove)
                _memoryCache.Remove(requestId);
        }
    }
}