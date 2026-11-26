namespace LogHub.Graphs;

public sealed class ServiceGraph
{
    private readonly List<Node> _roots = [];

    public IReadOnlyList<Node> Roots => _roots;

    public void AddRoot(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        _roots.Add(root);
    }

    public IEnumerable<Node> DepthFirst(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        return DepthFirstIterator(root, new HashSet<Node>());
    }

    public IEnumerable<Node> BreadthFirst(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var visited = new HashSet<Node> { root };
        var queue = new Queue<Node>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            yield return node;

            foreach (var dependency in node.Dependencies)
            {
                if (visited.Add(dependency))
                {
                    queue.Enqueue(dependency);
                }
            }
        }
    }

    public IEnumerable<Node> DepthFirstManual(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        return new DepthFirstManualEnumerable(root);
    }

    public IEnumerable<Node> BreadthFirstManual(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        return new BreadthFirstManualEnumerable(root);
    }

    public bool TryFindCycle(out IReadOnlyList<Node> path)
    {
        var color = new Dictionary<Node, int>();
        var parent = new Dictionary<Node, Node>();

        foreach (var root in _roots)
        {
            if (color.TryGetValue(root, out var rootColor) && rootColor == 2)
            {
                continue;
            }

            if (Visit(root, color, parent, out path))
            {
                return true;
            }
        }

        path = Array.Empty<Node>();
        return false;
    }

    public static ServiceGraph BuildLogHubGraph()
    {
        var program = new Node("Program");
        var compositionRoot = new Node("CompositionRoot");
        var configLoader = new Node("ConfigLoader");
        var configService = new Node("ConfigService");
        var configServiceContract = new Node("IConfigService");
        var config = new Node("LogHubConfig");
        var processor = new Node("LogProcessor");
        var importer = new Node("ILogImporter");
        var multiEncodingImporter = new Node("MultiEncodingFileImporter");
        var jsonImporter = new Node("JsonLogImporter");
        var lineParser = new Node("LogLineParser");
        var encodingDetector = new Node("EncodingDetector");
        var formatter = new Node("LogFormatter");
        var filter = new Node("Func<LogEntry,bool>");
        var sink = new Node("ILogSink");
        var consoleSink = new Node("ConsoleLogSink");
        var fileSink = new Node("FileLogSink");

        program.AddDependency(compositionRoot);
        program.AddDependency(processor);

        compositionRoot.AddDependency(configLoader);
        compositionRoot.AddDependency(configService);
        compositionRoot.AddDependency(config);
        compositionRoot.AddDependency(processor);
        compositionRoot.AddDependency(importer);
        compositionRoot.AddDependency(lineParser);
        compositionRoot.AddDependency(formatter);
        compositionRoot.AddDependency(filter);
        compositionRoot.AddDependency(sink);

        compositionRoot.AddDependency(multiEncodingImporter);
        compositionRoot.AddDependency(jsonImporter);
        compositionRoot.AddDependency(consoleSink);
        compositionRoot.AddDependency(fileSink);

        configService.AddDependency(configServiceContract);
        config.AddDependency(configServiceContract);

        processor.AddDependency(importer);
        processor.AddDependency(filter);
        processor.AddDependency(formatter);
        processor.AddDependency(sink);

        multiEncodingImporter.AddDependency(lineParser);
        multiEncodingImporter.AddDependency(encodingDetector);

        consoleSink.AddDependency(config);
        fileSink.AddDependency(config);

        var graph = new ServiceGraph();
        graph.AddRoot(program);
        return graph;
    }

    private static IEnumerable<Node> DepthFirstIterator(Node node, HashSet<Node> visited)
    {
        if (!visited.Add(node))
        {
            yield break;
        }

        yield return node;

        foreach (var dependency in node.Dependencies)
        {
            foreach (var descendant in DepthFirstIterator(dependency, visited))
            {
                yield return descendant;
            }
        }
    }

    private static bool Visit(
        Node node,
        Dictionary<Node, int> color,
        Dictionary<Node, Node> parent,
        out IReadOnlyList<Node> path)
    {
        color[node] = 1;

        foreach (var dependency in node.Dependencies)
        {
            if (!color.TryGetValue(dependency, out var dependencyColor) || dependencyColor == 0)
            {
                parent[dependency] = node;
                if (Visit(dependency, color, parent, out path))
                {
                    return true;
                }
            }
            else if (dependencyColor == 1)
            {
                path = BuildCyclePath(node, dependency, parent);
                return true;
            }
        }

        color[node] = 2;
        path = Array.Empty<Node>();
        return false;
    }

    private static IReadOnlyList<Node> BuildCyclePath(
        Node current,
        Node cycleStart,
        Dictionary<Node, Node> parent)
    {
        var reverse = new List<Node>();
        var node = current;

        while (node != cycleStart)
        {
            reverse.Add(node);
            if (!parent.TryGetValue(node, out var next) || next is null)
            {
                return Array.Empty<Node>();
            }

            node = next;
        }

        reverse.Add(cycleStart);
        reverse.Reverse();
        reverse.Add(cycleStart);
        return reverse;
    }
}
