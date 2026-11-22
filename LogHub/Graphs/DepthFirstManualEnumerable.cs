using System.Collections;

namespace LogHub.Graphs;

public sealed class DepthFirstManualEnumerable : IEnumerable<Node>
{
    private readonly Node _root;

    public DepthFirstManualEnumerable(Node root)
    {
        _root = root;
    }

    public IEnumerator<Node> GetEnumerator() => new DepthFirstEnumerator(_root);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
