using System.Globalization;
using System.Text.RegularExpressions;

namespace LogHub.Parsing;

public sealed class LogLineParser
{
    private readonly Regex _pattern = new(
        @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\] (DEBUG|INFO|WARN|ERROR) (.+)$",
        RegexOptions.Compiled);

    public bool TryParse(string? line, out LogEntry entry)
    {
        entry = default!;

        if (string.IsNullOrEmpty(line))
        {
            return false;
        }

        var match = _pattern.Match(line);
        if (!match.Success)
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                match.Groups[1].Value,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var timestamp))
        {
            return false;
        }

        if (!LogLevelNames.TryParse(match.Groups[2].Value, out var level))
        {
            return false;
        }

        entry = new LogEntry(timestamp, level, match.Groups[3].Value);
        return true;
    }
}
