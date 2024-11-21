using DeviceProxy;
using Google.Protobuf;

namespace DeviceClient;

public static class RequestDelegates
{
    public static RequestDelegate ExternalRequestDelegate = async context =>
    {
        var serialNumber = context.Request.RouteValues["serialNumber"]?.ToString();
        if(serialNumber == null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var requestAwaiter = new RequestAwaiter(Guid.NewGuid(), context.Request);
        context.RequestServices.GetRequiredService<OngoingRequests>().AddRequest(requestAwaiter);
        context.RequestServices.GetRequiredService<RequestQueue>().Enqueue(serialNumber, requestAwaiter.RequestId);
    
        try
        {
            var response = await requestAwaiter.Task;
            context.Response.StatusCode = response.StatusCode;
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }
            await context.Response.WriteAsync(response.Body);
        }
        catch (TaskCanceledException)
        {
            context.Response.StatusCode = 408;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
        }
    };
    
    public static RequestDelegate ResponseRequestDelegate = async context =>
    {
        var serialNumber = context.Request.RouteValues["serialNumber"]?.ToString();
        if(serialNumber == null)
        {
            context.Response.StatusCode = 400;
            return;
        }
        
        Guid? requestId = context.Request.Headers.ContainsKey("X-Request-Id") ? Guid.Parse(context.Request.Headers["X-Request-Id"].First() ?? "") : null;

        if(requestId == null)
        {
            context.Response.StatusCode = 408;
            return;
        }
        
        var requestAwaiter = context.RequestServices.GetRequiredService<OngoingRequests>().GetRequest(requestId.Value, remove: true);
        if(requestAwaiter == null)
        {
            context.Response.StatusCode = 404;
            return;
        }
    
        // deserialize the body of the request into a ProtoHttpResponse
        ProtoHttpResponse protoResponse = await context.Request.ParseResponseBodyToProtoHttpResponseAsync();
        requestAwaiter.SetResult(protoResponse);
    };

    public static RequestDelegate PushChannelRequestDelegate = async context =>
    {
        context.Response.ContentType = "text/event-stream";

        var serialNumber = context.Request.RouteValues["serialNumber"]?.ToString();
        if (serialNumber == null)
        {
            context.Response.StatusCode = 400;
            return;
        }
        Console.WriteLine($"Serial Number: {serialNumber}");

        var queue = context.RequestServices.GetRequiredService<RequestQueue>();
        var ongoingRequests = context.RequestServices.GetRequiredService<OngoingRequests>();

        while (true)
        {
            var requestId = queue.Dequeue(serialNumber);
            if (requestId == null)
            {
                await Task.Delay(100); // Avoid tight loop
                continue;
            }

            var requestAwaiter = ongoingRequests.GetRequest(requestId.Value, remove: true);
            if (requestAwaiter?.Request == null)
            {
                continue;
            }

            // Simulate waiting for incoming HTTP requests to forward
            var requestData = await requestAwaiter.Request.ToProtoHttpRequestAsync(serialNumber);
            requestData.Headers.Add(new Header
            {
                Key = "X-Request-Id",
                Value = requestId.ToString()
            });

            await context.Response.WriteAsync("data: ");

            // Serialize the Protobuf message to a memory stream
            using (var memoryStream = new MemoryStream())
            {
                requestData.WriteDelimitedTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position

                // Write the serialized data asynchronously to the response
                await memoryStream.CopyToAsync(context.Response.Body);
            }

            await context.Response.WriteAsync("\n\n");
            await context.Response.Body.FlushAsync();
        }
    };
}