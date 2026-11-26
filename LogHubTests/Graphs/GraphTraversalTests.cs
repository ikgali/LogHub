using LogHub.Graphs;

namespace LogHubTests.Graphs;

public class GraphTraversalTests
{
    [Test]
    public void DepthFirst_ReferenceGraph_ReturnsPreOrder()
    {
        var (graph, root) = BuildReferenceGraph();

        var order = graph.DepthFirst(root).Select(node => node.Name);

        Assert.That(order, Is.EqualTo(new[] { "A", "B", "D", "E", "C" }));
    }

    [Test]
    public void BreadthFirst_ReferenceGraph_ReturnsLevelOrder()
    {
        var (graph, root) = BuildReferenceGraph();

        var order = graph.BreadthFirst(root).Select(node => node.Name);

        Assert.That(order, Is.EqualTo(new[] { "A", "B", "C", "D", "E" }));
    }

    [Test]
    public void NestedForeach_OverSameGraph_ProducesCorrectPairs()
    {
        var (graph, root) = BuildReferenceGraph();

        var pairs = new List<string>();
        foreach (var service in graph.DepthFirst(root))
        {
            foreach (var dependency in graph.BreadthFirst(service))
            {
                pairs.Add($"{service.Name}->{dependency.Name}");
            }
        }

        Assert.That(pairs, Is.EqualTo(new[]
        {
            "A->A", "A->B", "A->C", "A->D", "A->E",
            "B->B", "B->D", "B->E",
            "D->D", "D->E",
            "E->E",
            "C->C", "C->D", "C->E"
        }));
    }

    [Test]
    public void CyclicGraph_TraversalTerminatesAndVisitsEachNodeOnce()
    {
        var (graph, root) = BuildCycleGraph();

        var depthFirst = graph.DepthFirst(root).Select(node => node.Name).ToArray();
        var breadthFirst = graph.BreadthFirst(root).Select(node => node.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(depthFirst, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(breadthFirst, Is.EqualTo(new[] { "A", "B", "C" }));
        });
    }

    [Test]
    public void TryFindCycle_CyclicGraph_ReturnsClosedPath()
    {
        var (graph, _) = BuildCycleGraph();

        var found = graph.TryFindCycle(out var path);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.True);
            Assert.That(path.Select(node => node.Name), Is.EqualTo(new[] { "A", "B", "C", "A" }));
            Assert.That(path[0], Is.SameAs(path[^1]));
        });
    }

    [Test]
    public void TryFindCycle_AcyclicGraph_ReturnsFalse()
    {
        var (graph, _) = BuildReferenceGraph();

        var found = graph.TryFindCycle(out var path);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.False);
            Assert.That(path, Is.Empty);
        });
    }

    [Test]
    public void TryFindCycle_EmptyGraph_ReturnsFalse()
    {
        var graph = new ServiceGraph();

        var found = graph.TryFindCycle(out var path);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.False);
            Assert.That(path, Is.Empty);
        });
    }

    [Test]
    public void Traversal_SingleVertex_YieldsOnlyRoot()
    {
        var root = new Node("Solo");
        var graph = new ServiceGraph();
        graph.AddRoot(root);

        Assert.Multiple(() =>
        {
            Assert.That(graph.DepthFirst(root).Select(node => node.Name), Is.EqualTo(new[] { "Solo" }));
            Assert.That(graph.BreadthFirst(root).Select(node => node.Name), Is.EqualTo(new[] { "Solo" }));
            Assert.That(graph.DepthFirstManual(root).Select(node => node.Name), Is.EqualTo(new[] { "Solo" }));
            Assert.That(graph.BreadthFirstManual(root).Select(node => node.Name), Is.EqualTo(new[] { "Solo" }));
        });
    }

    [Test]
    public void DepthFirstManual_MatchesYieldVersion()
    {
        var (graph, root) = BuildReferenceGraph();

        var manual = graph.DepthFirstManual(root).Select(node => node.Name).ToArray();
        var yieldVersion = graph.DepthFirst(root).Select(node => node.Name).ToArray();

        Assert.That(manual, Is.EqualTo(yieldVersion));
    }

    [Test]
    public void BreadthFirstManual_MatchesYieldVersion()
    {
        var (graph, root) = BuildReferenceGraph();

        var manual = graph.BreadthFirstManual(root).Select(node => node.Name).ToArray();
        var yieldVersion = graph.BreadthFirst(root).Select(node => node.Name).ToArray();

        Assert.That(manual, Is.EqualTo(yieldVersion));
    }

    [Test]
    public void ManualEnumerators_Reset_AllowsSecondTraversal()
    {
        var (graph, root) = BuildReferenceGraph();

        AssertResetMatchesYield(new DepthFirstEnumerator(root), graph.DepthFirst(root));
        AssertResetMatchesYield(new BreadthFirstEnumerator(root), graph.BreadthFirst(root));
    }

    private static void AssertResetMatchesYield(
        IEnumerator<Node> enumerator,
        IEnumerable<Node> expected)
    {
        var first = Drain(enumerator);

        enumerator.Reset();
        var second = Drain(enumerator);

        var expectedNames = expected.Select(node => node.Name).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(expectedNames));
            Assert.That(second, Is.EqualTo(expectedNames));
        });
    }

    private static List<string> Drain(IEnumerator<Node> enumerator)
    {
        var names = new List<string>();
        while (enumerator.MoveNext())
        {
            names.Add(enumerator.Current.Name);
        }

        return names;
    }

    private static (ServiceGraph Graph, Node Root) BuildReferenceGraph()
    {
        var a = new Node("A");
        var b = new Node("B");
        var c = new Node("C");
        var d = new Node("D");
        var e = new Node("E");

        a.AddDependency(b);
        a.AddDependency(c);
        b.AddDependency(d);
        c.AddDependency(d);
        d.AddDependency(e);

        var graph = new ServiceGraph();
        graph.AddRoot(a);
        return (graph, a);
    }

    private static (ServiceGraph Graph, Node Root) BuildCycleGraph()
    {
        var a = new Node("A");
        var b = new Node("B");
        var c = new Node("C");

        a.AddDependency(b);
        b.AddDependency(c);
        c.AddDependency(a);

        var graph = new ServiceGraph();
        graph.AddRoot(a);
        return (graph, a);
    }
}
