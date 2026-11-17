using LogHub.Sinks;

namespace LogHubTests;

internal sealed class RecordingSink : ILogSink
{
    public RecordingSink(string name = "recording")
    {
        Name = name;
    }

    public string Name { get; }

    public List<string> Lines { get; } = [];

    public void Write(string formattedLine) => Lines.Add(formattedLine);
}
