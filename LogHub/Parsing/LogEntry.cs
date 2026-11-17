namespace LogHub.Parsing;

public sealed record LogEntry(DateTime Timestamp, LogLevel Level, string Message);
