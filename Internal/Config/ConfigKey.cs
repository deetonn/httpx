
using System.Text.RegularExpressions;

namespace Http.Server.Internal.Config;

/// <summary>
/// The attribute used to assign proper, descriptive names to configuration
/// keys, and assign a default value.
/// </summary>
/// <param name="Key">The name that should be used in the actual configuration file.</param>
/// <param name="Default">The default value, can be null to signify it's not set.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public partial class ConfigKeyAttribute(
    string Key, 
    object? Default
) : Attribute
{
    public string Key { get; set; } = Key;
    public object? Default { get; set; } = Default;

    /// <summary>
    /// Executes a simple regex to ensure that Key is a valid json key.
    /// </summary>
    /// <returns></returns>
    public bool IsValidKeyForJson()
    {
        return !string.IsNullOrEmpty(Key)
            && !ValidJsonKeySchema().IsMatch(Key);
    }

    /// <summary>
    /// Attempt to cast the default value into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? Into<T>() where T: class =>  Default as T;

    public object? GetDefault() => Default;
    public string GetKey() => Key;

    [GeneratedRegex("([A-Za-z_][A-Za-z0-9_-]*)")]
    public static partial Regex ValidJsonKeySchema();
}
