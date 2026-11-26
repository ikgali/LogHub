using System.Collections;

namespace LogHub.Graphs;

public sealed class BreadthFirstEnumerator : IEnumerator<Node>
{
    private readonly Node _root;
    private readonly Queue<Node> _queue = new();
    private readonly HashSet<Node> _visited = [];
    private bool _started;
    private Node? _current;

    public BreadthFirstEnumerator(Node root)
    {
        _root = root;
    }

    public Node Current => _current ?? throw new InvalidOperationException("MoveNext ещё не вернул true.");

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (!_started)
        {
            _visited.Add(_root);
            _queue.Enqueue(_root);
            _started = true;
        }

        while (_queue.Count > 0)
        {
            var node = _queue.Dequeue();
            foreach (var dependency in node.Dependencies)
            {
                if (_visited.Add(dependency))
                {
                    _queue.Enqueue(dependency);
                }
            }

            _current = node;
            return true;
        }

        _current = null;
        return false;
    }

    public void Reset()
    {
        _queue.Clear();
        _visited.Clear();
        _started = false;
        _current = null;
    }

    public void Dispose()
    {
        _queue.Clear();
        _visited.Clear();
        _current = null;
    }
}
