using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace API;

public class DynamicConfigProvider : IProxyConfigProvider
{
    private InMemoryConfig _config;

    public DynamicConfigProvider()
    {
        _config = new InMemoryConfig(
            new List<RouteConfig>(),
            new List<ClusterConfig>()
        );
    }

    public IProxyConfig GetConfig() => _config;

    public void Update(IEnumerable<RouteConfig> routes, IEnumerable<ClusterConfig> clusters)
    {
        var newChangeToken = new CancellationChangeToken(new CancellationTokenSource().Token);

        _config = new InMemoryConfig(routes.ToList(), clusters.ToList(), newChangeToken);
    }
}

public class InMemoryConfig : IProxyConfig
{
    public InMemoryConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        IChangeToken? changeToken = null)
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = changeToken ?? new CancellationChangeToken(new CancellationToken());
    }

    public IReadOnlyList<RouteConfig> Routes { get; }
    public IReadOnlyList<ClusterConfig> Clusters { get; }
    public IChangeToken ChangeToken { get; }
}
