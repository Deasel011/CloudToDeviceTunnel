using Google.Protobuf;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeviceClient
{
    public class SSETunneler
    {
        private readonly HttpClient _httpClient;

        public SSETunneler(HttpClient httpClient) { _httpClient = httpClient; }

        public async Task Start(string yarpPushChannel, string yarpResponseChannel)
        {
            try
            {
                using var SSEResponse = await _httpClient.GetAsync(yarpPushChannel, HttpCompletionOption.ResponseHeadersRead);
                using var stream = await SSEResponse.Content.ReadAsStreamAsync();
                
                while (true)
                {
                    try
                    {
                        stream.Position = 0;
                        var protoHttpRequest = ProtoHttpRequest.Parser.ParseDelimitedFrom(stream);
                        var response = await _httpClient.SendAsync(await protoHttpRequest.FromProtoHttpRequestAsync());
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var protoHttpResponse = new ProtoHttpResponse
                        {
                            StatusCode = (int)response.StatusCode,
                            Body = responseBody,
                            Headers = { response.Headers.Select(header => new Header { Key = header.Key, Value = header.Value.First() }) }
                        };

                        var content = new ByteArrayContent(protoHttpResponse.ToByteArray());
                        content.Headers.Add("X-Request-Id", protoHttpRequest.Headers.Where(x => x.Key == "X-Request-Id").Select(x => x.Value).FirstOrDefault());
                        await _httpClient.PostAsync(yarpResponseChannel, content);
                    }
                    catch (InvalidProtocolBufferException ex)
                    {
                        Console.WriteLine($"Protobuf parsing error: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

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
    }
}