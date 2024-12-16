
using Http.Server.Internal.Config;
using Http.Server.Internal.Logging;
using Http.Server.Network;
using Http.Server.Requests;

namespace Http.Server;

/// <summary>
/// The HTTP server container.
/// </summary>
public class Server : IDisposable
{
    /// <summary>
    /// All http listeners.
    /// </summary>
    private HttpListenerCollection _listeners;

    /// <summary>
    /// The server configuration.
    /// </summary>
    private readonly Configuration _configuration;

    /// <summary>
    /// The servers logger.
    /// </summary>
    private ILogger _logger;

    public Server()
    {
        // default initialize the server logger to file logging.
        _logger = new FileLogger();

        _configuration = new Configuration(_logger);
        _listeners = new HttpListenerCollection(_logger, _configuration);

        DebugLog.SetConfiguration(_configuration);
    }

    /// <summary>
    /// Set the servers logger.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get the servers configuration.
    /// </summary>
    /// <returns></returns>
    public Configuration GetConfig() => _configuration;

    /// <summary>
    /// Get the HTTP listeners associated with this server.
    /// </summary>
    /// <returns></returns>
    public HttpListenerCollection GetListeners() => _listeners;

    /// <summary>
    /// Add an endpoint, constructing it using <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="HttpEndpoint"/></typeparam>
    public void AddEndpoint<T>() where T: HttpEndpoint, new()
    {
        _listeners.AddHttpListener(new T());
    }

    /// <summary>
    /// Add an endpoint that is already constructed. This is useful if an endpoint requires 
    /// parameters to construct.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="HttpEndpoint"/></typeparam>
    /// <param name="endpoint"></param>
    public void AddEndpoint<T>(T endpoint) where T: HttpEndpoint
    {
        _listeners.AddHttpListener(endpoint);
    }

    /// <summary>
    /// Start the server. This will configure system settings, like max threads, and also 
    /// start all listeners.
    /// </summary>
    /// <returns></returns>
    public async Task Begin()
    {
        ConfigureMachineSettings();

        _listeners.StartAll();

        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    /// Sets max thread information for the thread pool.
    /// </summary>
    private void ConfigureMachineSettings()
    {
        var maxWorkerThreads = _configuration.Get<long>(ConfigKey.MaxWorkerThreads);
        var maxCompletionPortThreads = _configuration.Get<long>(ConfigKey.MaxCompletionPortThreads);

        ThreadPool.SetMaxThreads((int)maxWorkerThreads, (int)maxCompletionPortThreads);
    }

    /// <summary>
    /// Disposes of the server, saving the configuration and stopping all listeners.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _listeners.Dispose();
        _configuration.Dispose();
    }
}
