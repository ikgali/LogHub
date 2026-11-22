using System.Globalization;
using System.Text;
using System.Text.Json;
using LogHub.Parsing;

namespace LogHub.Import;

public sealed class JsonLogImporter : LogImporterBase
{
    private readonly string? _path;
    private readonly Stream? _providedStream;
    private readonly long _startPosition;
    private StreamReader? _reader;
    private Stream? _stream;
    private bool _ownsStream;

    public JsonLogImporter(string path)
    {
        _path = path;
    }

    public JsonLogImporter(Stream stream)
    {
        _providedStream = stream;
        _startPosition = stream.CanSeek ? stream.Position : 0;
    }

    protected override void Open()
    {
        if (_path is not null)
        {
            _stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _ownsStream = true;
        }
        else
        {
            _stream = _providedStream!;
            _ownsStream = false;
            if (_stream.CanSeek)
            {
                _stream.Position = _startPosition;
            }
        }

        _reader = new StreamReader(
            _stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);
    }

    protected override IEnumerable<string> ReadRaw()
    {
        while (_reader!.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    protected override bool TryParse(string raw, out LogEntry entry)
    {
        entry = default!;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!root.TryGetProperty("timestamp", out var timestampElement) ||
                timestampElement.ValueKind != JsonValueKind.String ||
                !root.TryGetProperty("level", out var levelElement) ||
                levelElement.ValueKind != JsonValueKind.String ||
                !root.TryGetProperty("message", out var messageElement) ||
                messageElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var timestampText = timestampElement.GetString();
            var levelText = levelElement.GetString();
            var message = messageElement.GetString();

            if (timestampText is null || levelText is null || message is null)
            {
                return false;
            }

            if (!DateTime.TryParseExact(
                    timestampText,
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var timestamp))
            {
                return false;
            }

            if (!LogLevelNames.TryParse(levelText, out var level))
            {
                return false;
            }

            entry = new LogEntry(timestamp, level, message);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    protected override void Close()
    {
        _reader?.Dispose();
        _reader = null;

        if (_ownsStream)
        {
            _stream?.Dispose();
        }

        _stream = null;
        _ownsStream = false;
    }
}
