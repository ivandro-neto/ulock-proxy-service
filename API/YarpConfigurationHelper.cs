using System;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class YarpConfigurationHelper
{
     private readonly DynamicConfigProvider _provider;

    public YarpConfigurationHelper(DynamicConfigProvider provider)
    {
        _provider = provider;
    }

    public void AddApp(string appName, string host, string backend)
    {
        var routes = _provider.GetConfig().Routes.ToList();
        var clusters = _provider.GetConfig().Clusters.ToList();

        routes.Add(new RouteConfig
        {
            RouteId = $"{appName}_route",
            ClusterId = $"{appName}_cluster",
            Match = new RouteMatch
            {
                Hosts = new[] { host },
                Path = "{**catch-all}"
            }
        });

        clusters.Add(new ClusterConfig
        {
            ClusterId = $"{appName}_cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = backend } }
            }
        });

        _provider.Update(routes, clusters);
    }
}

