using System;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class YarpConfigurationHelper
{
    private readonly DynamicConfigProvider _provider;

    private readonly List<RouteConfig> routes = new();
    private readonly List<ClusterConfig> clusters = new();

    public YarpConfigurationHelper(DynamicConfigProvider provider)
    {
        _provider = provider;
    }

    public void AddApp(string appName, string host, string backend)
    {
        var clusterId = $"{appName}-cluster";

        var route = new RouteConfig
        {
            RouteId = $"{appName}-route",
            ClusterId = clusterId,
            Match = new RouteMatch
            {
                Hosts = new[] { host },
                Path = "{**catch-all}"
            }
        };

        var cluster = new ClusterConfig
        {
            ClusterId = clusterId,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = backend } }
            }
        };

        routes.Add(route);
        clusters.Add(cluster);

        _provider.Update(routes, clusters);
    }
}

