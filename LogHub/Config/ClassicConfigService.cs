namespace LogHub.Config;

public sealed class ClassicConfigService : IConfigService
{
    private static readonly object SyncRoot = new();
    private static volatile ClassicConfigService? _instance;

    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    private ClassicConfigService()
    {
    }

    public static ClassicConfigService Instance()
    {
        if (_instance is null)
        {
            lock (SyncRoot)
            {
                _instance ??= new ClassicConfigService();
            }
        }

        return _instance;
    }

    public string? GetValue(string key)
    {
        lock (SyncRoot)
        {
            return _values.GetValueOrDefault(key);
        }
    }

    public void SetValue(string key, string value)
    {
        lock (SyncRoot)
        {
            _values[key] = value;
        }
    }
}
