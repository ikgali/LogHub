using LogHub.Config;

namespace LogHub.Sinks;

public static class SinkCatalog
{
    public static ILogSink Create(string name, LogHubConfig config)
    {
        foreach (var type in typeof(SinkCatalog).Assembly.GetTypes())
        {
            if (type.IsAbstract || !typeof(ILogSink).IsAssignableFrom(type))
            {
                continue;
            }

            var constructor = type.GetConstructor([typeof(LogHubConfig)]);
            if (constructor is null)
            {
                continue;
            }

            if (!string.Equals(GetName(type), name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return (ILogSink)constructor.Invoke([config]);
        }

        throw new InvalidOperationException($"Приёмник '{name}' не найден.");
    }

    public static string GetName(Type type)
    {
        var name = type.Name;
        if (name.EndsWith("LogSink", StringComparison.Ordinal))
        {
            name = name[..^"LogSink".Length];
        }

        return name.ToLowerInvariant();
    }
}
