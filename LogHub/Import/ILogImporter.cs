using LogHub.Parsing;

namespace LogHub.Import;

public interface ILogImporter
{
    IReadOnlyList<LogEntry> Import();
}
