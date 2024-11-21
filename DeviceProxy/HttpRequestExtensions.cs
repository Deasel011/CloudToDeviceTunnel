using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

namespace DeviceClient;

public static class HttpRequestExtensions
{
    public static async Task<HttpRequestMessage> FromProtoHttpRequestAsync(this ProtoHttpRequest protoHttpRequest)
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

    public static async Task<ProtoHttpResponse> ParseResponseBodyToProtoHttpResponseAsync(this HttpRequest request)
    {
        if (request.Method != "POST")
        {
            throw new InvalidOperationException("Only POST requests are supported");
        }
        
        // Read all bytes from the request body
        MemoryStream memoryStream = new MemoryStream();
        await request.Body.CopyToAsync(memoryStream);
        byte[] responseBodyBytes = memoryStream.ToArray();

        var protoHttpResponse = ProtoHttpResponse.Parser.ParseFrom(responseBodyBytes);
        return protoHttpResponse;
    }
    
    public static async Task<ProtoHttpRequest> ToProtoHttpRequestAsync(this HttpRequest request, string serialNumber)
    {
        // Read all bytes from the request body
        MemoryStream memoryStream = new MemoryStream();
        await request.Body.CopyToAsync(memoryStream);
        byte[] requestBodyBytes = memoryStream.ToArray();

        // Build the ProtoHttpRequest
        var protoHttpRequest = new ProtoHttpRequest
        {
            Method = request.Method,
            PathAndQuery = request.Path.ToString().Split($"/{serialNumber}").Last() + request.QueryString,
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