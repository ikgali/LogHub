using LogHub.Parsing;

namespace LogHub.Import;

public sealed class InMemoryLogImporter : LogImporterBase
{
    private readonly IReadOnlyList<string> _lines;
    private readonly LogLineParser _parser;

    public InMemoryLogImporter(params string[] lines)
        : this(new LogLineParser(), lines)
    {
    }

    public InMemoryLogImporter(IEnumerable<string> lines)
        : this(new LogLineParser(), lines)
    {
    }

    public InMemoryLogImporter(LogLineParser parser, IEnumerable<string> lines)
    {
        _parser = parser;
        _lines = lines.ToList();
    }

    protected override IEnumerable<string> ReadRaw() => _lines;

    protected override bool TryParse(string raw, out LogEntry entry) => _parser.TryParse(raw, out entry);
}
