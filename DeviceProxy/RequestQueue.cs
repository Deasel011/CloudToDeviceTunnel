using System.Collections.Concurrent;

namespace DeviceProxy;

public class RequestQueue
{
    private readonly Dictionary<string, ConcurrentQueue<Guid>> _queue = new();
    
    public void Enqueue(string serialNumber, Guid requestId)
    {
        if(!_queue.TryGetValue(serialNumber, out ConcurrentQueue<Guid> queue))
        {
            queue = new ConcurrentQueue<Guid>();
            _queue.Add(serialNumber, queue);
        }
        queue.Enqueue(requestId);
    }
    
    public Guid? Dequeue(string serialNumber)
    {
        if(_queue.TryGetValue(serialNumber, out ConcurrentQueue<Guid> queue))
        {
            if(queue.TryDequeue(out Guid requestId))
            {
                return requestId;
            }
        }
        return null;
    }
}