using System.Net;
using System.Text;

namespace Http.Server.Internal.Extensions;

public static class ResponseExtensions
{
    public static void AppendString(this HttpListenerResponse response, string value)
    {
        response.OutputStream.Write((response.ContentEncoding ?? Encoding.UTF8).GetBytes(value));
        response.ContentLength64 += value.Length;
    }
}
