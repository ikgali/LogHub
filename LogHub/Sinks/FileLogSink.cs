using LogHub.Config;

namespace LogHub.Sinks;

public sealed class FileLogSink : ILogSink
{
    private readonly string _outputPath;

    public FileLogSink(LogHubConfig config)
    {
        _outputPath = config.OutputPath;
    }

    public string Name => "file";

    public void Write(string formattedLine)
    {
        using var writer = new StreamWriter(_outputPath, append: true);
        writer.WriteLine(formattedLine);
    }
}
