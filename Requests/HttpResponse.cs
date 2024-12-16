
using System.Net;
using System.Text;
using Http.Server.Internal.Logging;
using Newtonsoft.Json;

namespace Http.Server.Requests;

/// <summary>
/// An abstract class that represents a HttpResponse in Http.Server.
/// This will be transformed naturally into the actual response structure (using an output stream)
/// however, we keep the implementation simple here, for developers sake.
/// </summary>
public class HttpResponse
{
    /// <summary>
    /// The cookies sent in the response.
    /// </summary>
    public CookieCollection Cookies { get; set; } = [];

    /// <summary>
    /// The headers sent with the response.
    /// </summary>
    public WebHeaderCollection Headers { get; set; } = [];

    /// <summary>
    /// The content type of the information in the body. This defaults to "text/plain".
    /// </summary>
    public string ContentType { get; set; } = "text/plain";

    /// <summary>
    /// The status code. Defaults to 200 (OK)
    /// </summary>
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    /// <summary>
    /// Optional status code message if you need to be more descriptive.
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// The body, as a string builder.
    /// </summary>
    public StringBuilder Body { get; set; } = new StringBuilder();

    /// <summary>
    /// The encoding of the response. This defaults to <see cref="Encoding.UTF8"/>.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    // Utility methods.

    /// <summary>
    /// Add a cookie to <see cref="Cookies"/>, then return <c>this</c>.
    /// </summary>
    /// <param name="cookie">The cookie itself</param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse AddCookie(Cookie cookie)
    {
        Cookies.Add(cookie);
        return this;
    }

    /// <summary>
    /// Add a header to <see cref="Headers"/>, then return <c>this</c>.
    /// </summary>
    /// <param name="name">The name of the header</param>
    /// <param name="value">The value of the header.</param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse AddHeader(string name, string value)
    {
        Headers.Add(name, value); 
        return this;
    }

    /// <summary>
    /// Set the status code to <paramref name="statusCode"/>.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Set <see cref="ContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse WithContentType(string contentType)
    {
        ContentType = contentType; 
        return this;
    }

    /// <summary>
    /// Overwrites the body, and sets the entirety of its content to <paramref name="body"/>.
    /// To append, using AppendToBody
    /// </summary>
    /// <param name="body">The content, should be the same content-type as <see cref="ContentType"/></param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse WithBody(string body)
    {
        Body = new StringBuilder(body);
        return this;
    }

    /// <summary>
    /// Uses Newtonsoft.Json to serialize <paramref name="obj"/> and sets the response to that.
    /// NOTE: This overwrites anything previously written to the body.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse WithSerializedBody(object? obj)
    {
        Body = new StringBuilder(JsonConvert.SerializeObject(obj));
        return this;
    }

    /// <summary>
    /// Append <paramref name="content"/> to <see cref="Body"/>.
    /// </summary>
    /// <param name="content">The content to append.</param>
    /// <returns>This, the same instance called on.</returns>
    public HttpResponse AppendToBody(string content)
    {
        Body.Append(content);
        return this;
    }

    private static readonly Dictionary<string, string> MappedFileExtensionTranslations = new()
    {
        [".html"] = "text/html",
        [".htm"] = "text/html",
        [".js"] = "text/javascript",
        [".css"] = "text/css"
    };

    /// <summary>
    /// Sets the entire body (overwriting anything else) to the contents of a file.
    /// This will also deduce the content type via the file extension.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public HttpResponse SetBodyToFile(string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"{fileInfo.FullName} is not found.");
        }

        if (MappedFileExtensionTranslations.TryGetValue(fileInfo.Extension, out var contentType))
        {
            ContentType = contentType;
        }
        else
        {
            DebugLog.Log($"Cannot deduce the content type from extension '[yellow bold]{fileInfo.Extension}[/]'");
        }

        Body = new StringBuilder(File.ReadAllText(filePath));
        return this;
    }
}
