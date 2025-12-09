using System;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class YarpConfigurationHelper
{
    private readonly DynamicConfigProvider _provider;

    private readonly List<RouteConfig> _routes = new();
    private readonly List<ClusterConfig> _clusters = new();

    public YarpConfigurationHelper(DynamicConfigProvider provider)
    {
        _provider = provider;
    }

    public void AddApp(string appName, string host, string backendAddress)
    {
        var route = new RouteConfig()
        {
            RouteId = $"{appName}-route",
            ClusterId = $"{appName}-cluster",
            Match = new RouteMatch
            {
                Hosts = new[] { host },
                Path = "{**catch-all}"
            }
        };

        var cluster = new ClusterConfig()
        {
            ClusterId = $"{appName}-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = backendAddress } }
            }
        };

        _routes.Add(route);
        _clusters.Add(cluster);

        _provider.UpdateConfig(_routes, _clusters);
    }
}

