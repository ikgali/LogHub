using LogHub.Parsing;

namespace LogHub.Config;

public sealed record LogHubConfig(
    string InputPath,
    LogLevel MinLevel,
    string Profile,
    string OutputPath,
    string Source)
{
    public static LogHubConfig From(IConfigService config) =>
        new(
            config.GetValue("input") ?? "data/sample-utf8.log",
            LogLevelNames.Parse(config.GetValue("minLevel") ?? "INFO"),
            config.GetValue("profile") ?? "console",
            config.GetValue("output") ?? "data/out.log",
            config.GetValue("source") ?? "text");
}
