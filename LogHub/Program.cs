using LogHub.Composition;
using LogHub.Processing;
using Microsoft.Extensions.DependencyInjection;

var configPath = args.Length > 0 ? args[0] : "config.txt";
using var provider = CompositionRoot.Build(configPath);
var processor = provider.GetRequiredService<LogProcessor>();
var result = processor.Run();
Console.WriteLine($"Обработано: {result.Read}, записано: {result.Written}, пропущено: {result.Skipped}");
