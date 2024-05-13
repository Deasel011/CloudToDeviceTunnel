using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

public static class HttpRequestExtensions
{
    public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest request)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(request.Method),
            RequestUri = new Uri(request.GetDisplayUrl()),
            Content = new StreamContent(request.Body)
        };
        foreach (var header in request.Headers)
        {
            httpRequestMessage.Headers.Add(header.Key, header.Value.ToArray());
        }
        return httpRequestMessage;
    }
    
    public static HttpRequestMessage ToLocalHttpRequestMessage(this HttpRequest request)
    {
        var uri = new UriBuilder
        {
            Scheme = "https",
            Host = "localhost",
            Port = 443,
            Path = request.Path.ToString().Split("/tunnel").Last(),
            Query = request.QueryString.ToString()
        };
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(request.Method),
            RequestUri = new Uri(uri.ToString()),
            Content = new StreamContent(request.Body)
        };
        foreach (var header in request.Headers)
        {
            httpRequestMessage.Headers.Add(header.Key, header.Value.ToArray());
        }
        return httpRequestMessage;
    }
    
    public static async Task<HttpRequestMessage> FromPtotoHttpRequestAsync(this ProtoHttpRequest protoHttpRequest)
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
    
    public static async Task<ProtoHttpRequest> ToLocalProtoHttpRequestAsync(this HttpRequest request)
    {
        // Read all bytes from the request body
        MemoryStream memoryStream = new MemoryStream();
        await request.Body.CopyToAsync(memoryStream);
        byte[] requestBodyBytes = memoryStream.ToArray();

        // Build the ProtoHttpRequest
        var protoHttpRequest = new ProtoHttpRequest
        {
            Method = request.Method,
            PathAndQuery = request.Path.ToString().Split("/tunnel").Last() + request.QueryString,
            Body = Encoding.UTF8.GetString(requestBodyBytes)
        };

        // Add headers to ProtoHttpRequest
        foreach (var header in request.Headers)
        {
            foreach (var headerValue in header.Value)
            {
                protoHttpRequest.Headers.Add(new Header
                {
                    Key = header.Key,
                    Value = headerValue
                });
            }
        }

        return protoHttpRequest;
    }

}