
using Http.Server.Internal.Config;
using Http.Server.Internal.Extensions;
using Http.Server.Internal.Logging;
using Http.Server.Requests;

namespace Http.Server.Network;

/// <summary>
/// A collection of HTTP listeners.
/// </summary>
public class HttpListenerCollection : IDisposable
{
    private readonly List<HttpListenerContainer> _containers;
    private readonly ILogger _logger;
    private readonly Configuration _configuration;

    /// <summary>
    /// Construct the collection of listeners with access to a logger and configuration.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    public HttpListenerCollection(ILogger logger, Configuration config)
    {
        _configuration = config;
        _containers = [];
        _logger = logger;
    }

    /// <summary>
    /// Initialize all listeners.
    /// </summary>
    public void StartAll()
    {
        foreach (var container in _containers)
        {
            container.Initialize(_configuration);
        }

        DebugLog.Log($"Initialized {_containers.Count} listeners.");
    }

    /// <summary>
    /// Add an HTTP endpoint, this will be not started by default.
    /// </summary>
    /// <param name="endpoint"></param>
    public void AddHttpListener(HttpEndpoint endpoint)
    {
        _containers.Add(new HttpListenerContainer(endpoint, _configuration));
    }

    /// <summary>
    /// Invalid the cache for <paramref name="route"/>.
    /// </summary>
    /// <param name="route"></param>
    public void InvalidateCacheFor(string route)
    {
        var validContainers = _containers.Where(x => x.Listener.Prefixes.Contains(route));
        
        if (!validContainers.Any())
        {
            _logger.WriteWarningSync($"Failed to invalidate cache for {route}. It does not exist.");
            return;
        }

        validContainers.First().GetCache().Invalidate();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var item in _containers)
        {
            item.Dispose();
        }
    }
}
