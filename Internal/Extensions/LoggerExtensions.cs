
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Http.Server.Internal.Logging;

namespace Http.Server.Internal.Extensions;

public static class LoggerExtensions
{
    public static void WriteSync(this ILogger logger, string content)
    {
        logger.Write(content).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void WriteInfoSync(this ILogger logger, string content)
    {
        logger.WriteInfo(content).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void WriteWarningSync(this ILogger logger, string content)
    {
        logger.WriteWarning(content).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void WriteErrorSync(this ILogger logger, string content)
    {
        logger.WriteError(content).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
