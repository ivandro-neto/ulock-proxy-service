using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class DynamicConfigProvider : IProxyConfigProvider
{
    private volatile InMemoryConfig _config;
    private readonly object _lock = new();

    public DynamicConfigProvider()
    {
        _config = new InMemoryConfig(new List<RouteConfig>(), new List<ClusterConfig>());
    }

    // Este é chamado pelo YARP
    public IProxyConfig GetConfig() => _config;

    public void UpdateConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        lock (_lock)
        {
            _config = new InMemoryConfig(routes, clusters);
        }
    }

    private class InMemoryConfig : IProxyConfig
    {
        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(new CancellationToken());
        }

        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }
    }
}