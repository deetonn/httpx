
using System.Net;
using Http.Server.Internal.Logging;
using Spectre.Console;

namespace Http.Server.Requests;

/// <summary>
/// The HTTP method accepted by a route.
/// </summary>
public enum HttpMethod
{
    Get,
    Post,
    Put,
    Delete,
    Patch
}

/// <summary>
/// The base class for any http endpoint.
/// </summary>
public class HttpEndpoint
{
    /// <summary>
    /// The endpoint that this <see cref="HttpEndpoint"/> responds to. This must be relative,
    /// for example, "/api/v1/json". This must be overriden, otherwise it defaults to "/".
    /// </summary>
    public virtual string Endpoint { get; set; } = "/";

    /// <summary>
    /// The method that this endpoint responds to. This is defaulted to
    /// <see cref="HttpMethod.Get"/>.
    /// </summary>
    public virtual HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Is this route dynamic? If it is, the server will not cache it ever.
    /// If it isn't dynamic, the server caches the first computed response, and 
    /// continues to serve that until invalidated.
    /// </summary>
    public virtual bool Dynamic { get; } = false;

    /// <summary>
    /// How much time is needed until the cached data is automatically invalidated?
    /// If this is null, it is never invalidated. This is only available if <see cref="Dynamic"/> is
    /// set to true.
    /// </summary>
    public virtual TimeSpan? InvalidateAfter { get; set; } = null;

    /// <summary>
    /// The actual main logic for the endpoint.
    /// </summary>
    /// <param name="request">The request from a client.</param>
    /// <returns>The <see cref="HttpResponse"/> generated. This method should throw if something fatal occurs.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual async Task<HttpResponse> Execute(HttpListenerRequest request)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        DebugLog.LogRequest(Method, $"{Endpoint}");
        return new HttpResponse().AddHeader("Server", "httpx");
    }

    /// <summary>
    /// This is called when the <see cref="HttpListenerTimeoutManager"/> is initialized. Overriding
    /// this function allows you to modify specifics.
    /// </summary>
    /// <param name="timeoutManager">The timeout manager, referenced by the listener.</param>
    public virtual void OnTimeoutManagerInit(HttpListenerTimeoutManager timeoutManager) 
    {
        return;
    }

    /// <summary>
    /// This is called when <see cref="Execute(HttpListenerRequest)"/> throws. The server naturally handles this,
    /// but this method can be used as middleware for exception handling.
    /// 
    /// The default behaviour of this is to pretty print the exception. If you'd like this
    /// behaviour to continue, just call it in the override.
    /// </summary>
    /// <param name="e">The exception that occured</param>
    public virtual void OnException(Exception e, HttpResponse response)
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
    }
}
