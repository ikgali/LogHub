using System.Globalization;
using System.Text.RegularExpressions;

namespace LogHub;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public interface ILogModule
{
    void Init();

    void Process();

    void Flush();

    void Audit();
}

public abstract class ProcessingModuleBase
{
    public virtual bool Validate() => true;
}

public class LogProcessingModule : ProcessingModuleBase, ILogModule
{
    private readonly string _configPath;
    private readonly Dictionary<string, string> _config = new();
    private readonly Regex _linePattern = new(
        @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\] (DEBUG|INFO|WARN|ERROR) (.+)$",
        RegexOptions.Compiled);
    private readonly DbConnectionStub _db = new();

    private string _inputPath = string.Empty;
    private string _outputPath = string.Empty;
    private string _sinkType = "console";
    private LogLevel _minLevel = LogLevel.Info;
    private StreamWriter? _fileWriter;
    private int _readCount;
    private int _writtenCount;
    private int _skippedCount;

    public LogProcessingModule(string configPath)
    {
        _configPath = configPath;
    }

    public void Run()
    {
        Init();
        Process();
        Flush();
        Console.WriteLine($"Обработано: {_readCount}, записано: {_writtenCount}, пропущено: {_skippedCount}");
    }

    public void Init()
    {
        foreach (var line in File.ReadAllLines(_configPath))
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                _config[parts[0].Trim()] = parts[1].Trim();
            }
        }

        _db.Open();
        _inputPath = _config.GetValueOrDefault("input", "data/sample-utf8.log");
        _outputPath = _config.GetValueOrDefault("output", "data/out.log");
        _sinkType = _config.GetValueOrDefault("sink", "console");
        _minLevel = ParseLevel(_config.GetValueOrDefault("minLevel", "INFO"));
    }

    public void Process()
    {
        var lines = File.ReadAllLines(_inputPath);
        _readCount = lines.Length;
        _writtenCount = 0;
        _skippedCount = 0;

        foreach (var line in lines)
        {
            if (!TryParseLine(line, out var timestamp, out var level, out var message))
            {
                _skippedCount++;
                continue;
            }

            if (level < _minLevel)
            {
                _skippedCount++;
                continue;
            }

            var formatted = $"{timestamp:yyyy-MM-dd HH:mm:ss} [{LevelName(level)}] {message}";
            switch (_sinkType)
            {
                case "console":
                    Console.WriteLine(formatted);
                    break;
                case "file":
                    _fileWriter ??= new StreamWriter(_outputPath, append: true);
                    _fileWriter.WriteLine(formatted);
                    break;
                default:
                    throw new NotSupportedException($"Неизвестный приёмник: {_sinkType}");
            }

            _writtenCount++;
        }
    }

    public void Flush()
    {
        _fileWriter?.Flush();
        _fileWriter?.Dispose();
        _db.Close();
    }

    public void Audit()
    {
        throw new NotImplementedException();
    }

    public override bool Validate()
    {
        throw new NotSupportedException();
    }

    private bool TryParseLine(string line, out DateTime timestamp, out LogLevel level, out string message)
    {
        timestamp = default;
        level = default;
        message = string.Empty;

        var match = _linePattern.Match(line);
        if (!match.Success)
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                match.Groups[1].Value,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out timestamp))
        {
            return false;
        }

        level = ParseLevel(match.Groups[2].Value);
        message = match.Groups[3].Value;
        return true;
    }

    private static LogLevel ParseLevel(string value) => value.ToUpperInvariant() switch
    {
        "DEBUG" => LogLevel.Debug,
        "INFO" => LogLevel.Info,
        "WARN" => LogLevel.Warning,
        "ERROR" => LogLevel.Error,
        _ => throw new ArgumentException($"Неизвестный уровень: {value}", nameof(value))
    };

    private static string LevelName(LogLevel level) => level switch
    {
        LogLevel.Debug => "DEBUG",
        LogLevel.Info => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "ERROR",
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };

    private sealed class DbConnectionStub
    {
        public void Open()
        {
        }

        public void Close()
        {
        }
    }
}
