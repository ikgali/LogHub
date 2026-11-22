using System.Collections;

namespace LogHub.Graphs;

public sealed class DepthFirstEnumerator : IEnumerator<Node>
{
    private readonly Node _root;
    private readonly Stack<Node> _stack = new();
    private readonly HashSet<Node> _visited = [];
    private bool _started;
    private Node? _current;

    public DepthFirstEnumerator(Node root)
    {
        _root = root;
    }

    public Node Current => _current ?? throw new InvalidOperationException("MoveNext ещё не вернул true.");

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (!_started)
        {
            _stack.Push(_root);
            _started = true;
        }

        while (_stack.Count > 0)
        {
            var node = _stack.Pop();
            if (!_visited.Add(node))
            {
                continue;
            }

            for (var index = node.Dependencies.Count - 1; index >= 0; index--)
            {
                _stack.Push(node.Dependencies[index]);
            }

            _current = node;
            return true;
        }

        _current = null;
        return false;
    }

    public void Reset()
    {
        _stack.Clear();
        _visited.Clear();
        _started = false;
        _current = null;
    }

    public void Dispose()
    {
        _stack.Clear();
        _visited.Clear();
        _current = null;
    }
}
