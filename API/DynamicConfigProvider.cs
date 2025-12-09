using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class DynamicConfigProvider : IProxyConfigProvider
{
    private volatile InMemoryConfig _currentConfig;
    private readonly object _lock = new();

    public DynamicConfigProvider()
    {
        _currentConfig = new InMemoryConfig(new List<RouteConfig>(), new List<ClusterConfig>());
    }

    public IProxyConfig GetConfig() => _currentConfig;

    public void Update(List<RouteConfig> routes, List<ClusterConfig> clusters)
    {
        lock (_lock)
        {
            var oldConfig = _currentConfig;

            _currentConfig = new InMemoryConfig(routes, clusters);

            oldConfig.SignalChange();
        }
    }

    private class InMemoryConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new();

        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
        }

        public IReadOnlyList<RouteConfig> Routes { get; }

        public IReadOnlyList<ClusterConfig> Clusters { get; }

        public IChangeToken ChangeToken => new CancellationChangeToken(_cts.Token);

        public void SignalChange()
        {
            _cts.Cancel();
        }
    }
}