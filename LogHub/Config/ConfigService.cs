using System.Collections.Concurrent;

namespace LogHub.Config;

public sealed class ConfigService : IConfigService
{
    private readonly ConcurrentDictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public string? GetValue(string key) => _values.GetValueOrDefault(key);

    public void SetValue(string key, string value) => _values[key] = value;
}
