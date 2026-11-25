namespace LogHub.Sinks;

public interface ILogSink
{
    string Name { get; }

    void Write(string formattedLine);
}
