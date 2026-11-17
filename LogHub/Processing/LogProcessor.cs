using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Reading;
using LogHub.Sinks;

namespace LogHub.Processing;

public sealed class LogProcessor
{
    private readonly ILogReader _reader;
    private readonly LogLineParser _parser;
    private readonly Func<LogEntry, bool> _filter;
    private readonly LogFormatter _formatter;
    private readonly IReadOnlyList<ILogSink> _sinks;

    public LogProcessor(
        ILogReader reader,
        LogLineParser parser,
        Func<LogEntry, bool> filter,
        LogFormatter formatter,
        IReadOnlyList<ILogSink> sinks)
    {
        _reader = reader;
        _parser = parser;
        _filter = filter;
        _formatter = formatter;
        _sinks = sinks;
    }

    public ProcessingResult Run()
    {
        var read = 0;
        var written = 0;
        var skipped = 0;

        foreach (var line in _reader.ReadLines())
        {
            read++;

            if (!_parser.TryParse(line, out var entry))
            {
                skipped++;
                continue;
            }

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
