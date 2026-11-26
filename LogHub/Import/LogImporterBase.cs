using LogHub.Parsing;

namespace LogHub.Import;

public abstract class LogImporterBase : ILogImporter
{
    public int SkippedCount { get; private set; }

    public IReadOnlyList<LogEntry> Import()
    {
        SkippedCount = 0;
        var entries = new List<LogEntry>();

        Open();
        try
        {
            foreach (var raw in ReadRaw())
            {
                if (!TryParse(raw, out var entry))
                {
                    SkippedCount++;
                    continue;
                }

                if (!Validate(entry))
                {
                    SkippedCount++;
                    continue;
                }

                entries.Add(entry);
            }
        }
        finally
        {
            Close();
            Release();
        }

        return entries;
    }

    IReadOnlyList<LogEntry> ILogImporter.Import() => Import();

    protected abstract IEnumerable<string> ReadRaw();

    protected abstract bool TryParse(string raw, out LogEntry entry);

    protected virtual bool Validate(LogEntry entry) => true;

    protected virtual void Open()
    {
    }

    protected virtual void Close()
    {
    }

    protected virtual void Release()
    {
    }
}
