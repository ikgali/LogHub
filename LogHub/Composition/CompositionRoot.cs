using LogHub.Config;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Import;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace LogHub.Composition;

public static class CompositionRoot
{
    public static ServiceProvider Build(string configPath)
    {
        var configService = new ConfigService();
        ConfigLoader.LoadInto(configPath, configService);
        return Build(LogHubConfig.From(configService), configService);
    }

    public static ServiceProvider Build(LogHubConfig config, IConfigService configService)
    {
        var source = config.Source.ToLowerInvariant();

        if (source is not ("text" or "json"))
        {
            throw new InvalidOperationException(
                $"Неизвестный источник '{config.Source}'. Ожидается 'text' или 'json'.");
        }

        var services = new ServiceCollection();
        services.AddSingleton<IConfigService>(configService);
        services.AddSingleton(config);
        services.AddSingleton<LogLineParser>();
        services.AddSingleton<LogFormatter>();
        services.AddSingleton<ILogImporter>(provider =>
            CreateImporter(source, config.InputPath, provider.GetRequiredService<LogLineParser>()));
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

    private static ILogImporter CreateImporter(string source, string inputPath, LogLineParser parser) =>
        source switch
        {
            "text" => new MultiEncodingFileImporter(inputPath, parser),
            "json" => new JsonLogImporter(inputPath),
            _ => throw new InvalidOperationException(
                $"Неизвестный источник '{source}'. Ожидается 'text' или 'json'.")
        };
}
