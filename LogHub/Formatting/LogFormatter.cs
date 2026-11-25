using System.Globalization;
using LogHub.Parsing;

namespace LogHub.Formatting;

public sealed class LogFormatter
{
    public string Format(LogEntry entry) =>
        $"{entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} [{LogLevelNames.ToText(entry.Level)}] {entry.Message}";
}
