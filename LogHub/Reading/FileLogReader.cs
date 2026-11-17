namespace LogHub.Reading;

public sealed class FileLogReader : ILogReader
{
    private readonly string _path;

    public FileLogReader(string path)
    {
        _path = path;
    }

    public IEnumerable<string> ReadLines() => File.ReadLines(_path);
}
