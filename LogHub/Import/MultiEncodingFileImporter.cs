using System.Text;
using LogHub.Parsing;

namespace LogHub.Import;

public sealed class MultiEncodingFileImporter : LogImporterBase
{
    private readonly string? _path;
    private readonly Stream? _providedStream;
    private readonly long _startPosition;
    private readonly LogLineParser _parser;
    private Stream? _stream;
    private bool _ownsStream;

    public MultiEncodingFileImporter(string path)
        : this(path, new LogLineParser())
    {
    }

    public MultiEncodingFileImporter(string path, LogLineParser parser)
    {
        _path = path;
        _parser = parser;
    }

    public MultiEncodingFileImporter(Stream stream)
        : this(stream, new LogLineParser())
    {
    }

    public MultiEncodingFileImporter(Stream stream, LogLineParser parser)
    {
        _providedStream = stream;
        _startPosition = stream.CanSeek ? stream.Position : 0;
        _parser = parser;
    }

    protected override void Open()
    {
        if (_path is not null)
        {
            _stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _ownsStream = true;
            return;
        }

        _stream = _providedStream!;
        _ownsStream = false;
        if (_stream.CanSeek)
        {
            _stream.Position = _startPosition;
        }
    }

    protected override IEnumerable<string> ReadRaw()
    {
        var bytes = ReadAllBytes(_stream!);
        var encoding = EncodingDetector.Detect(bytes);
        var preambleLength = GetPreambleLength(bytes, encoding);
        var text = encoding.GetString(bytes[preambleLength..]);

        using var reader = new StringReader(text);
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    protected override bool TryParse(string raw, out LogEntry entry) => _parser.TryParse(raw, out entry);

    protected override bool Validate(LogEntry entry) =>
        !string.IsNullOrWhiteSpace(entry.Message) && !entry.Message.Contains('\uFFFD');

    protected override void Close()
    {
        if (_ownsStream)
        {
            _stream?.Dispose();
        }

        _stream = null;
        _ownsStream = false;
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static int GetPreambleLength(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        var preamble = encoding.GetPreamble();
        return preamble.Length > 0 && bytes.StartsWith(preamble) ? preamble.Length : 0;
    }
}
