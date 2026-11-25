using LogHub.Parsing;

namespace LogHub.Config;

public sealed record LogHubConfig(
    string InputPath,
    LogLevel MinLevel,
    IReadOnlyList<string> SinkNames,
    string OutputPath);
