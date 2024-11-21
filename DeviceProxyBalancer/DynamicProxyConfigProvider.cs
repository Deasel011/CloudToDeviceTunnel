using Yarp.ReverseProxy.Configuration;

namespace DeviceProxy;

public class DynamicProxyConfigProvider : IProxyConfigProvider
{
    private volatile InMemoryConfigProvider _configProvider;

    public DynamicProxyConfigProvider()
    {
        // Initialize with empty routes and clusters
        _configProvider = new InMemoryConfigProvider(new List<RouteConfig>(), new List<ClusterConfig>());
    }

    public IProxyConfig GetConfig() => _configProvider.GetConfig();

    public void UpdateConfig(IEnumerable<string> serialNumbers)
    {
        var routes = new List<RouteConfig>();
        var clusters = new List<ClusterConfig>();

        foreach (var serialNumber in serialNumbers)
        {
            // Define /proxy/<serialNumber>/{**catch-all} route
            routes.Add(new RouteConfig
            {
                RouteId = $"proxy-{serialNumber}",
                ClusterId = $"cluster-{serialNumber}",
                Match = new RouteMatch
                {
                    Path = $"/proxy/{serialNumber}/{{**catch-all}}"
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "PathRemovePrefix", $"/proxy/{serialNumber}" }
                    }
                }
            });

            // Define /ws/<serialNumber>/{**catch-all} route
            routes.Add(new RouteConfig
            {
                RouteId = $"ws-{serialNumber}",
                ClusterId = $"cluster-{serialNumber}",
                Match = new RouteMatch
                {
                    Path = $"/ws/{serialNumber}/{{**catch-all}}"
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "WebSocket", "true" },
                        { "PathRemovePrefix", $"/ws/{serialNumber}" }
                    }
                }
            });

            // Define cluster for the serial number
            clusters.Add(new ClusterConfig
            {
                ClusterId = $"cluster-{serialNumber}",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    {
                        "destination1",
                        new DestinationConfig
                        {
                            Address = $"http://target-server-for-{serialNumber}/" // Dynamically determine the address
                        }
                    }
                }
            });
        }

        _configProvider.Update(routes, clusters);
    }
}
