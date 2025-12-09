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

    public IProxyConfig GetConfig() => _config;

    public void Update(IEnumerable<RouteConfig> routes, IEnumerable<ClusterConfig> clusters)
    {
        lock (_lock)
        {
            _config = new InMemoryConfig(routes.ToList(), clusters.ToList());
            _config.SignalChange();
        }
    }

    private class InMemoryConfig : IProxyConfig
    {
        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        private readonly CancellationTokenSource _cts = new();

        public InMemoryConfig(
            IReadOnlyList<RouteConfig> routes,
            IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
        }

        public void SignalChange() => _cts.Cancel();

        public IChangeToken ChangeToken =>
            new CancellationChangeToken(_cts.Token);
    }
}
