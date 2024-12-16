
using System.Net;
using System.Text;

namespace Http.Server.Network.Cache;

/// <summary>
/// A HTTP response that has previously been computed.
/// </summary>
public class HttpCachedResponse
{
    /// <summary>
    /// The status code of this cached response.
    /// </summary>
    public required int StatusCode { get; set; }

    /// <summary>
    /// The status message of this cached response.
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// The encoding of this cached response. Used to encode the body.
    /// </summary>
    public Encoding? ContentEncoding { get; set; } = null;

    /// <summary>
    /// Default to "text/plain" for real requests.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// The cookies in this cached response.
    /// </summary>
    public CookieCollection? Cookies { get; set; }

    /// <summary>
    /// The headers in this cached response.
    /// </summary>
    public WebHeaderCollection Headers { get; set; } = [];

    // NOTE: KeepAlive is not here, as a static request cannot be keep-alived.

    /// <summary>
    /// The previously calculated body.
    /// </summary>
    public required string Body { get; set; }
    // NOTE: Content-Length is not here as we can calculate it via Body.Length
}

/// <summary>
/// The HTTP cache that works on an indivudual endpoint basis. 
/// </summary>
public class HttpCache
{
    private HttpCachedResponse? _lastCachedResponse;
    private DateTime? _lastModified;

    /// <summary>
    /// Set the cached item to <paramref name="lastCachedResponse"/>.
    /// </summary>
    /// <param name="lastCachedResponse"></param>
    public void SetItem(HttpCachedResponse? lastCachedResponse)
    {
        _lastCachedResponse = lastCachedResponse;
        _lastModified = DateTime.Now;
    }

    /// <summary>
    /// Does this cache instance actually have a cached item?
    /// </summary>
    /// <returns></returns>
    public bool HasCachedItem() => _lastCachedResponse != null;

    /// <summary>
    /// Get the cached item.
    /// </summary>
    /// <returns></returns>
    public HttpCachedResponse? GetCachedItem() => _lastCachedResponse;

    /// <summary>
    /// How long has it been since this cached item was cached.
    /// </summary>
    /// <returns></returns>
    public TimeSpan? TimeSinceModified()
    {
        return DateTime.Now - _lastModified;
    }

    /// <summary>
    /// Invalidate the cache, allowing the endpoint to recompute.
    /// </summary>
    public void Invalidate() => _lastCachedResponse = null;
}
