
using System.Net;
using Http.Server.Requests;
using Http.Server.Network.Cache;
using System.Text;
using Http.Server.Internal.Config;
using Http.Server.Internal.Logging;

namespace Http.Server.Network;

/// <summary>
/// A container that encapsulates as <see cref="HttpEndpoint"/>.
/// </summary>
public class HttpListenerContainer : IDisposable
{
    /// <summary>
    /// The actual http listener.
    /// </summary>
    private readonly HttpListener _listener;

    /// <summary>
    /// The cache for this container.
    /// </summary>
    private readonly Lazy<HttpCache> _cache;

    /// <summary>
    /// The encapsulated endpoint.
    /// </summary>
    private HttpEndpoint _endpoint;

    /// <summary>
    /// The actual http listener.
    /// </summary>
    public HttpListener Listener =>  _listener;

    /// <summary>
    /// Construct a <see cref="HttpListenerContainer"/> with a specific endpoint and access to the configuration.
    /// </summary>
    /// <param name="endpoint">The endpoint this container is operating.</param>
    /// <param name="config">The server configuration.</param>
    /// <exception cref="ArgumentException"></exception>
    public HttpListenerContainer(HttpEndpoint endpoint, Configuration config)
    {
        _endpoint = endpoint;
        _listener = new HttpListener();
        // We need to format the prefix correctly, with http/s and the base URL.

        var httpSegment = config.Get<bool>(ConfigKey.UseHttps) ? "https://" : "http://";
        var baseUrl = config.Get<string>(ConfigKey.BaseUrl) ?? throw new ArgumentException("The base URL has not been set in the configuration.");

        if (baseUrl == "localhost" || baseUrl == "127.0.0.1")
        {
            var port = config.Get<long>(ConfigKey.Port);
            baseUrl += $":{port}";
        }

        var endpointString = endpoint.Endpoint.StartsWith('/') ? endpoint.Endpoint : $"/{endpoint.Endpoint}";
        if (!endpointString.EndsWith('/'))
            endpointString += '/';
        var fullPrefix = $"{httpSegment}{baseUrl}{endpointString}";

        _listener.Prefixes.Add(fullPrefix);

        endpoint.OnTimeoutManagerInit(_listener.TimeoutManager);

        _cache = new Lazy<HttpCache>(() => new HttpCache());
    }

    /// <summary>
    /// Get the cache for this endpoint.
    /// </summary>
    /// <returns></returns>
    public HttpCache GetCache() =>  _cache.Value;

    /// <summary>
    /// Starts the listener and begins listening for HTTP requests.
    /// </summary>
    /// <param name="config">The server configuration.</param>
    public void Initialize(Configuration config)
    {
        _listener.Start();

        Task.Run(async () =>
        {
            while (_listener.IsListening)
            {
                var context = await _listener.GetContextAsync();

                var certificate = await context.Request.GetClientCertificateAsync();

                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    var cache = GetCache();

                    if (cache.HasCachedItem())
                    {
                        // This basically checks if the InvalidateAfter timespan has occured.
                        var revalidationPeriod = _endpoint.InvalidateAfter;

                        if (revalidationPeriod != null)
                        {
                            if (revalidationPeriod > cache.TimeSinceModified())
                            {
                                cache.Invalidate();
                            }
                        }
                        else
                        {
                            await DoCachedResponse(context, GetCache());
                            return;
                        }
                    }

                    HttpResponse response = new();
                    bool exceptionOccured = false;

                    try
                    {
                        response = await _endpoint.Execute(context.Request);
                    }
                    catch (Exception ex)
                    {
                        exceptionOccured = true;
                        HandleServerException(ex, response);
                    }

                    // Translate the easy to use HttpResponse, to the actual one.
                    context.Response.Cookies = response.Cookies;
                    context.Response.Headers = response.Headers;
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.ContentType = response.ContentType;
                    context.Response.ContentLength64 = response.Body.Length;
                    context.Response.ContentEncoding = response.Encoding;
                    await context.Response.OutputStream.WriteAsync(
                        response.Encoding.GetBytes(response.Body.ToString())
                    );

                    if (!GetCache().HasCachedItem() && !_endpoint.Dynamic
                        // if an exception occurs, we don't cache that response.
                        &&!exceptionOccured)
                    {
                        GetCache().SetItem(new HttpCachedResponse
                        {
                            StatusMessage = context.Response.StatusDescription,
                            StatusCode = context.Response.StatusCode,
                            ContentEncoding = context.Response.ContentEncoding,
                            ContentType = context.Response.ContentType,
                            Cookies = context.Response.Cookies,
                            Headers = context.Response.Headers,
                            Body = response.Body.ToString()
                        });
                    }

                    PostFinalizeResponse(context.Response);

                    // The response is sent.
                    context.Response.Close();
                });
            }
        });
    }

    /// <summary>
    /// Called when an exception occurs in user defined endpoint logic.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="response">The response</param>
    private void HandleServerException(Exception ex, HttpResponse response)
    {
        response.StatusCode = HttpStatusCode.InternalServerError;

        _endpoint.OnException(ex, response);

        return;
    }

    /// <summary>
    /// Called after a response is computed.
    /// </summary>
    /// <param name="response"></param>
    private static void PostFinalizeResponse(HttpListenerResponse response)
    {

    }

    /// <summary>
    /// Complete a cached response, skipping user-create computations.
    /// </summary>
    /// <param name="context">The HTTP listener context</param>
    /// <param name="cache">The cache itself</param>
    /// <returns></returns>
    private async Task DoCachedResponse(HttpListenerContext context, HttpCache cache)
    {
        DebugLog.Log($"[[[green bold]CACHE HIT[/]]] {context.Request.Url} (Cached for {cache.TimeSinceModified()})");

        // NOTE: This should never be called with a NULL cached item. So we use the ! here.
        var response = cache.GetCachedItem()!;

        context.Response.ContentLength64 = response.Body.Length;
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        context.Response.ContentEncoding = response.ContentEncoding ??  Encoding.UTF8;
        context.Response.Cookies = response.Cookies ??  [];
        context.Response.Headers = response.Headers;

        await context.Response.OutputStream.WriteAsync(
            context.Response.ContentEncoding.GetBytes(response.Body));

        context.Response.Close();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _listener.Close();
    }
}
