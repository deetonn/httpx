
using Http.Server.Internal.Config;
using Http.Server.Requests;
using Spectre.Console;

namespace Http.Server.Internal.Logging;

/// <summary>
/// This is the console logger, seperate from the ILogger interface. We use this everywhere, just for 
/// logging to the console with color and information.
/// </summary>
public static class DebugLog
{
    private static Configuration? _configuration;

    public static void Log(string message)
    {
        if (_configuration?.Get<bool>(ConfigKey.DisableConsoleLogging) == true)
            return;
        var time = DateTime.UtcNow;
        AnsiConsole.MarkupLine($"[[[yellow bold]{time}[/]]] [white bold]{message}[/]");
    }

    public static void LogRequest(Requests.HttpMethod method, string extraInfo)
    {
        if (_configuration?.Get<bool>(ConfigKey.DisableConsoleLogging) == true)
            return;
        var time = DateTime.UtcNow;
        AnsiConsole.MarkupLine($"[[[yellow bold]{time}[/]]] [[[green bold]{method.ToString().ToUpper()}[/]]] [white bold]{extraInfo}[/]");
    }

    public static void SetConfiguration(Configuration? configuration)
    {
        _configuration = configuration;
    }
}
