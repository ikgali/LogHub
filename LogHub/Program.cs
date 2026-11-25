using LogHub.Config;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Reading;
using LogHub.Sinks;

var configPath = args.Length > 0 ? args[0] : "config.txt";
var config = ConfigLoader.Load(configPath);
var sinks = config.SinkNames
    .Select(name => SinkCatalog.Create(name, config))
    .ToList();

var processor = new LogProcessor(
    new FileLogReader(config.InputPath),
    new LogLineParser(),
    LogFilters.ByMinLevel(config.MinLevel),
    new LogFormatter(),
    sinks);

var result = processor.Run();
Console.WriteLine($"Обработано: {result.Read}, записано: {result.Written}, пропущено: {result.Skipped}");
