using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class DynamicProxyConfig : IProxyConfig
{
    public DynamicProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = new CancellationChangeToken(new CancellationToken());
    }

    public IReadOnlyList<RouteConfig> Routes { get; }

    public IReadOnlyList<ClusterConfig> Clusters { get; }

    public IChangeToken ChangeToken { get; }
}
public class DynamicConfigProvider : IProxyConfigProvider
{
    private readonly object _lock = new();
    private List<RouteConfig> _routes = new();
    private List<ClusterConfig> _clusters = new();

    private CancellationTokenSource _cts = new();

    public IProxyConfig GetConfig() =>
        new DynamicProxyConfig(_routes, _clusters);

    public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        lock (_lock)
        {
            _routes = routes.ToList();
            _clusters = clusters.ToList();

            _cts.Cancel();               // avisa YARP que houve update
            _cts = new CancellationTokenSource();
        }
    }
}
