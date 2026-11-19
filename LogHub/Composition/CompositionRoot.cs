using LogHub.Config;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Reading;
using LogHub.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace LogHub.Composition;

public static class CompositionRoot
{
    public static ServiceProvider Build(string configPath)
    {
        var configService = new ConfigService();
        ConfigLoader.LoadInto(configPath, configService);
        var config = LogHubConfig.From(configService);

        var services = new ServiceCollection();
        services.AddSingleton<IConfigService>(configService);
        services.AddSingleton(config);
        services.AddSingleton<ILogReader>(_ => new FileLogReader(config.InputPath));
        services.AddSingleton<LogLineParser>();
        services.AddSingleton<LogFormatter>();
        services.AddSingleton<Func<LogEntry, bool>>(_ => LogFilters.ByMinLevel(config.MinLevel));

        switch (config.Profile.ToLowerInvariant())
        {
            case "console":
                services.AddSingleton<ILogSink, ConsoleLogSink>();
                break;
            case "file":
                services.AddSingleton<ILogSink, FileLogSink>();
                break;
            default:
                throw new InvalidOperationException(
                    $"Неизвестный профиль '{config.Profile}'. Ожидается 'console' или 'file'.");
        }

        services.AddSingleton<LogProcessor>();
        return services.BuildServiceProvider();
    }
}
