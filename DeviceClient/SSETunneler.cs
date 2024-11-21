namespace DeviceClient;

public class SSETunneler
{
    private readonly HttpClient _httpClient;

    public SSETunneler(HttpClient httpClient) { _httpClient = httpClient; }

    public async Task Start(string yarpPushChannel)
    {
        try
        {
            using var response = await _httpClient.GetAsync("http://yarp-server/push-channel", HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[4096];
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");
                
                // Handle the received HTTP request
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}