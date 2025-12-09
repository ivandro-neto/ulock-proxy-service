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
        // cria rota
        var route = new RouteConfig
        {
            RouteId = $"{appName}Route",
            ClusterId = $"{appName}Cluster",
            Match = new RouteMatch
            {
                Hosts = new[] { host },
                Path = "{**catch-all}"
            }
        };

        // cria cluster
        var cluster = new ClusterConfig
        {
            ClusterId = $"{appName}Cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = backendAddress } }
            }
        };

        // adiciona às listas existentes
        var current = _provider.GetConfig();

        var newRoutes = current.Routes.Concat(new[] { route }).ToList();
        var newClusters = current.Clusters.Concat(new[] { cluster }).ToList();

        _provider.Update(newRoutes, newClusters);

    }
}

