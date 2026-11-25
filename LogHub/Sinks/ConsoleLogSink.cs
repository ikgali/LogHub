using LogHub.Config;

namespace LogHub.Sinks;

public sealed class ConsoleLogSink : ILogSink
{
    public ConsoleLogSink(LogHubConfig config)
    {
        _ = config;
    }

    public string Name => "console";

    public void Write(string formattedLine) => Console.WriteLine(formattedLine);
}
