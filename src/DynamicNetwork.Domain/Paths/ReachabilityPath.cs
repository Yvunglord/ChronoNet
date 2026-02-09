using DynamicNetwork.Domain.Graph;
using System.Diagnostics.CodeAnalysis;

namespace DynamicNetwork.Domain.Paths;

public sealed class ReachabilityPath
{
    public required IReadOnlyList<string> Nodes { get; init; }
    public required IReadOnlyList<int> GraphIndices { get; init; }
    public required IReadOnlyList<EdgeTraversal> Edges { get; init; }
    public required TimeInterval Interval { get; init; }

    public int Length => Nodes.Count - 1;

    [SetsRequiredMembers]
    public ReachabilityPath(
        IReadOnlyList<string> nodes,
        IReadOnlyList<int> graphIndices,
        IReadOnlyList<EdgeTraversal> edges,
        TimeInterval interval)
    {
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        GraphIndices = graphIndices ?? throw new ArgumentNullException(nameof(graphIndices));
        Edges = edges ?? throw new ArgumentNullException(nameof(edges));
        Interval = interval;

        if (nodes.Count < 2)
            throw new ArgumentException("Path must contain at least 2 nodes", nameof(nodes));
    }

    public ReachabilityPath()
    {
        if (Nodes == null || Nodes.Count < 2)
            throw new InvalidOperationException("Path must contain at least 2 nodes");
    }
}

public sealed class EdgeTraversal
{
    public required string FromNode { get; init; }
    public required string ToNode { get; init; }
    public required int GraphIndex { get; init; }
    public required Link Link { get; init; }
}
