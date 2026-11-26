using LogHub.Composition;
using LogHub.Graphs;
using LogHub.Processing;
using Microsoft.Extensions.DependencyInjection;

if (args.Contains("--graph"))
{
    var graph = ServiceGraph.BuildLogHubGraph();
    var root = graph.Roots[0];

    Console.WriteLine("Обход в глубину (DFS):");
    foreach (var node in graph.DepthFirst(root))
    {
        Console.WriteLine($"  {node.Name}");
    }

    Console.WriteLine("Обход в ширину (BFS):");
    foreach (var node in graph.BreadthFirst(root))
    {
        Console.WriteLine($"  {node.Name}");
    }

    if (graph.TryFindCycle(out var cycle))
    {
        Console.WriteLine($"Найден цикл: {string.Join(" -> ", cycle.Select(node => node.Name))}");
    }
    else
    {
        Console.WriteLine("Циклических зависимостей не найдено.");
    }

    return;
}

var configPath = args.Length > 0 ? args[0] : "config.txt";
using var provider = CompositionRoot.Build(configPath);
var processor = provider.GetRequiredService<LogProcessor>();
var result = processor.Run();
Console.WriteLine($"Обработано: {result.Read}, записано: {result.Written}, пропущено: {result.Skipped}");
