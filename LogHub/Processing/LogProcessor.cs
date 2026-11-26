using LogHub.Formatting;
using LogHub.Import;
using LogHub.Parsing;
using LogHub.Sinks;

namespace LogHub.Processing;

public sealed class LogProcessor
{
    private readonly ILogImporter _importer;
    private readonly Func<LogEntry, bool> _filter;
    private readonly LogFormatter _formatter;
    private readonly IReadOnlyList<ILogSink> _sinks;

    public LogProcessor(
        ILogImporter importer,
        Func<LogEntry, bool> filter,
        LogFormatter formatter,
        IEnumerable<ILogSink> sinks)
    {
        _importer = importer;
        _filter = filter;
        _formatter = formatter;
        _sinks = sinks.ToList();
    }

    public ProcessingResult Run()
    {
        var entries = _importer.Import();
        var read = entries.Count;
        var written = 0;
        var skipped = 0;

        foreach (var entry in entries)
        {
            if (!_filter(entry))
            {
                skipped++;
                continue;
            }

            var formatted = _formatter.Format(entry);
            foreach (var sink in _sinks)
            {
                sink.Write(formatted);
            }

            written++;
        }

        return new ProcessingResult(read, written, skipped);
    }
}
