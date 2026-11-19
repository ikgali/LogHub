using System.Collections.Concurrent;

namespace LogHub.Config;

public sealed class LazyConfigService : IConfigService
{
    private static readonly Lazy<LazyConfigService> LazyInstance = new(() => new LazyConfigService());

    private readonly ConcurrentDictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    private LazyConfigService()
    {
    }

    public static LazyConfigService Instance() => LazyInstance.Value;

    public string? GetValue(string key) => _values.GetValueOrDefault(key);

    public void SetValue(string key, string value) => _values[key] = value;
}
