
namespace Http.Server.Internal.Logging;

public interface ILogger
{
    Task Write(string message);
    Task WriteInfo(string message);
    Task WriteError(string message);
    Task WriteWarning(string message);
}

public class FileLogger : ILogger
{
    private readonly string _path;

    public FileLogger()
    {
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }
        var dateNow = DateTime.Now;
        var logFile = Path.Combine(
            logPath, 
            $"logs-{dateNow.Hour}.{dateNow.Minute}-{dateNow.Year}.{dateNow.Month}.{dateNow.Day}.log"
        );

        if (!File.Exists(logFile))
        {
            File.Create(logFile).Dispose();
        }

        _path = logFile;
    }

    public async Task Write(string message)
    {
        try
        {
            await File.AppendAllTextAsync(_path, message + "\n");
        }
        catch (Exception)
        {
            // Just fail...
        }
    }

    public async Task WriteError(string message)
    {
        await Write("[ERROR]: " + message + "\n");
    }

    public async Task WriteInfo(string message)
    {
        await Write("[ INFO]: " + message + "\n");
    }

    public async Task WriteWarning(string message)
    {
        await Write("[ WARN]: " + message + "\n");
    }
}
