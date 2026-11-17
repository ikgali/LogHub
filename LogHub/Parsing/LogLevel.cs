namespace LogHub.Parsing;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public static class LogLevelNames
{
    public static bool TryParse(string value, out LogLevel level)
    {
        switch (value.ToUpperInvariant())
        {
            case "DEBUG":
                level = LogLevel.Debug;
                return true;
            case "INFO":
                level = LogLevel.Info;
                return true;
            case "WARN":
                level = LogLevel.Warning;
                return true;
            case "ERROR":
                level = LogLevel.Error;
                return true;
            default:
                level = default;
                return false;
        }
    }

    public static LogLevel Parse(string value) =>
        TryParse(value, out var level)
            ? level
            : throw new ArgumentException($"Неизвестный уровень: {value}", nameof(value));

    public static string ToText(LogLevel level) => level switch
    {
        LogLevel.Debug => "DEBUG",
        LogLevel.Info => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "ERROR",
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}
