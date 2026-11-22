namespace LogHub.Graphs;

public sealed class Node
{
    private readonly List<Node> _dependencies = [];

    public Node(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<Node> Dependencies => _dependencies;

    public void AddDependency(Node dependency) => _dependencies.Add(dependency);

    public override string ToString() => Name;
}
