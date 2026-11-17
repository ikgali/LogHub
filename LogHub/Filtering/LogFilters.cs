using LogHub.Parsing;

namespace LogHub.Filtering;

public static class LogFilters
{
    public static Func<LogEntry, bool> ByMinLevel(LogLevel min) =>
        entry => entry.Level >= min;
}
