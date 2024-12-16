
using Newtonsoft.Json;
using Http.Server.Internal.Extensions;
using Http.Server.Internal.Logging;

namespace Http.Server.Internal.Config;

public enum ConfigKey
{
    [ConfigKey("port", 8080)]
    Port,
    [ConfigKey("disable_unneeded_io_calls_like_console_logs", false)]
    DisableConsoleLogging,
    [ConfigKey("use_https", false)]
    UseHttps,
    [ConfigKey("base_url", "localhost")]
    BaseUrl,

    // NOTE: This is computed below to the max amount in the config.
    //       do not, I REPEAT, DO NOT change the name of either of these.
    [ConfigKey("max_worker_threads", null)]
    MaxWorkerThreads,
    [ConfigKey("max_completion_port_threads", null)]
    MaxCompletionPortThreads,
}

public class Configuration : IDisposable
{
    private Dictionary<ConfigKey, object?> _config;
    private readonly DirectoryInfo _path;
    private readonly ILogger _logger;

    private const string ConfigFileName = "core_config.json";

    public Configuration(ILogger logger)
    {
        _logger = logger;
        _path = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "configuration"));

        if (!_path.Exists)
        {
            _path.Create();
            // configuration does not exist. Default initialize.

            _config = CreateDefaultConfig();
            Save();

            return;
        }
        else
        {
            // We need to load the pre-existing storage.
            _config = LoadConfig();
        }
    }

    public T? Get<T>(ConfigKey key)
    {
        return (T?)_config[key];
    }

    private Dictionary<ConfigKey, object?> CreateDefaultConfig()
    {
        var result = new Dictionary<ConfigKey, object?>();

        foreach (var key in Enum.GetValues(typeof(ConfigKey)))
        {
            // NOTE: This exception is fatal, and is a bug if it occurs.
            var attribute = ((ConfigKey)key).GetAttributeOfType<ConfigKeyAttribute>() 
                ?? throw new InvalidDataException($"The configuration key {key} does not have a ConfigKeyAttribute attached.");

            if (attribute.GetKey() == "max_worker_threads")
            {
                ThreadPool.GetAvailableThreads(out int workers, out int _);
                attribute.Default = workers;
            }

            if (attribute.GetKey() == "max_completion_port_threads")
            {
                ThreadPool.GetAvailableThreads(out int _, out int cpt);
                attribute.Default = cpt;
            }

            result.Add((ConfigKey)key, attribute.GetDefault());
        }

        return result;
    }

    private Dictionary<ConfigKey, object?> LoadConfig()
    {
        Dictionary<string, object?>? json;

        try
        {
            var saveFile = File.ReadAllText(Path.Combine(_path.FullName, ConfigFileName));
            json = JsonConvert.DeserializeObject<Dictionary<string, object?>>(saveFile);
        }
        catch (Exception e)
        {
            _logger.WriteErrorSync($"Failed to load configuration! {e.Message}");
            throw;
        }

        if (json is null)
        {
            _logger.WriteErrorSync("The configuration file exists, but has invalid json. Using the defaults instead.");
            return CreateDefaultConfig();
        }

        var enumKeys = new SortedSet<object>();
        foreach (var enumValue in typeof(ConfigKey).GetEnumValues())
        {
            enumKeys.Add(enumValue);
        }

        Dictionary<ConfigKey, object?> values = [];

        // Now, we need to map the attribute names to the enum they exist on.
        foreach (var (key, value) in json)
        {
            var keyThatMatches
                = enumKeys.Where(x => ((ConfigKey)x).GetAttributeOfType<ConfigKeyAttribute>()?.GetKey() == key)
                .FirstOrDefault();

            if (keyThatMatches is null)
            {
                _logger.WriteWarningSync($"Unrecognized configuration value present: {key}");
                continue;
            }
            // remove entrys that have been processed.
            enumKeys.RemoveWhere(x => x == keyThatMatches);

            values.Add((ConfigKey)keyThatMatches, value);
        }

        return values;
    }

    private void Save()
    {
        var finalJson = new Dictionary<string, object?>();

        foreach (var (key, value) in _config)
        {
            var attribute =
                key.GetAttributeOfType<ConfigKeyAttribute>() ??
                throw new InvalidDataException($"The configuration key {key} does not have a ConfigKeyAttribute attached.");

            finalJson.Add(attribute.GetKey(), value);
        }

        try
        {
            var saveFile = Path.Combine(_path.FullName, ConfigFileName);
            var json = JsonConvert.SerializeObject(finalJson, Formatting.Indented);

            File.WriteAllText(saveFile, json);
        }
        catch (Exception e)
        {
            _logger.WriteWarningSync($"Failed to save configuration. {e.Message}");
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Save();
    }
}
