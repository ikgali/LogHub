using System.Collections;

namespace LogHub.Graphs;

public sealed class BreadthFirstManualEnumerable : IEnumerable<Node>
{
    private readonly Node _root;

    public BreadthFirstManualEnumerable(Node root)
    {
        _root = root;
    }

    public IEnumerator<Node> GetEnumerator() => new BreadthFirstEnumerator(_root);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
