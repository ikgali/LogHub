namespace LogHub.Reading;

public sealed class InMemoryLogReader : ILogReader
{
    private readonly IReadOnlyList<string> _lines;

    public InMemoryLogReader(params string[] lines)
    {
        _lines = lines;
    }

    public IEnumerable<string> ReadLines() => _lines;
}
