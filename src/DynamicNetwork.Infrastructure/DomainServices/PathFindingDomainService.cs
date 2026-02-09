using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Enums;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;
using DynamicNetwork.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicNetwork.Infrastructure.DomainServices;

public class PathFindingDomainService : IPathFindingDomainService
{
    public IReadOnlyList<ReachabilityPath> FindAllPaths(
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<StructConfiguration> configurations,
        string sourceNode,
        string targetNode,
        TimeInterval timeWindow)
    {
        var relevantGraphs = graphs
            .Where(g => g.Interval.Overlaps(timeWindow))
            .OrderBy(g => g.Interval.Start)
            .ToList();

        if (!relevantGraphs.Any())
            return new List<ReachabilityPath>();

        var startGraphIndices = relevantGraphs
            .Select((g, idx) => new { Graph = g, Index = idx })
            .Where(x => x.Graph.AllNetworkNodes.Contains(sourceNode))
            .Select(x => x.Index)
            .ToList();

        if (!startGraphIndices.Any())
            return new List<ReachabilityPath>();

        var paths = new List<ReachabilityPath>();

        var stack = new Stack<PathState>();

        foreach (var startGraphIdx in startGraphIndices)
        {
            var startGraph = relevantGraphs[startGraphIdx];

            stack.Push(new PathState
            {
                CurrentNode = sourceNode,
                CurrentGraphIndex = startGraphIdx,
                StartTime = startGraph.Interval.Start,
                VisitedNodes = new HashSet<string> { sourceNode },
                Nodes = new List<string> { sourceNode },
                GraphIndices = new List<int> { startGraphIdx },
                Edges = new List<EdgeTraversal>()
            });
        }

        while (stack.Count > 0)
        {
            var state = stack.Pop();

            if (state.CurrentNode == targetNode)
            {
                paths.Add(new ReachabilityPath(
                    nodes: state.Nodes.AsReadOnly(),
                    graphIndices: state.GraphIndices.AsReadOnly(),
                    edges: state.Edges.AsReadOnly(),
                    interval: new TimeInterval(state.StartTime,
                        relevantGraphs[state.CurrentGraphIndex].Interval.End)
                ));
                continue;
            }

            var currentGraph = relevantGraphs[state.CurrentGraphIndex];
            var currentConfig = configurations
                .FirstOrDefault(c => c.Interval.Overlaps(currentGraph.Interval));

            var nextStates = GenerateNextStates(
                state, currentGraph, currentConfig, targetNode, relevantGraphs);

            foreach (var nextState in nextStates)
            {
                stack.Push(nextState);
            }
        }

        return paths
            .OrderBy(p => p.Edges.Count)
            .ToList()
            .AsReadOnly();
    }

    private List<PathState> GenerateNextStates(
        PathState currentState,
        TemporalGraph currentGraph,
        StructConfiguration? currentConfig,
        string targetNode,
        IReadOnlyList<TemporalGraph> relevantGraphs)
    {
        var nextStates = new List<PathState>();

        var availableEdges = GetAvailableEdges(currentGraph, currentConfig, currentState.CurrentNode);

        foreach (var edge in availableEdges)
        {
            var nextNode = DetermineNextNode(edge, currentState.CurrentNode);
            if (nextNode == null)
                continue;

            if (currentState.VisitedNodes.Contains(nextNode) && nextNode != targetNode)
                continue;

            var nextState = new PathState
            {
                CurrentNode = nextNode,
                CurrentGraphIndex = currentState.CurrentGraphIndex,
                StartTime = currentState.StartTime,
                VisitedNodes = new HashSet<string>(currentState.VisitedNodes) { nextNode },
                Nodes = new List<string>(currentState.Nodes) { nextNode },
                GraphIndices = new List<int>(currentState.GraphIndices) { currentState.CurrentGraphIndex },
                Edges = new List<EdgeTraversal>(currentState.Edges)
                {
                    new EdgeTraversal
                    {
                        FromNode = currentState.CurrentNode,
                        ToNode = nextNode,
                        GraphIndex = currentState.CurrentGraphIndex,
                        Link = edge
                    }
                }
            };

            nextStates.Add(nextState);
        }

        if (currentState.CurrentGraphIndex + 1 < relevantGraphs.Count)
        {
            var nextGraphIndex = currentState.CurrentGraphIndex + 1;
            var nextGraph = relevantGraphs[nextGraphIndex];

            if (nextGraph.AllNetworkNodes.Contains(currentState.CurrentNode))
            {
                if (currentGraph.Interval.End <= nextGraph.Interval.Start)
                {
                    var nextState = new PathState
                    {
                        CurrentNode = currentState.CurrentNode,
                        CurrentGraphIndex = nextGraphIndex,
                        StartTime = currentState.StartTime,
                        VisitedNodes = new HashSet<string>(currentState.VisitedNodes),
                        Nodes = new List<string>(currentState.Nodes) { currentState.CurrentNode },
                        GraphIndices = new List<int>(currentState.GraphIndices) { nextGraphIndex },
                        Edges = new List<EdgeTraversal>(currentState.Edges)
                    };

                    nextStates.Add(nextState);
                }
            }
        }

        return nextStates;
    }

    private List<Link> GetAvailableEdges(
        TemporalGraph graph,
        StructConfiguration? config,
        string currentNode)
    {
        var edges = new List<Link>();

        foreach (var link in graph.Links)
        {
            if (link.Direction == LinkDirection.None)
                continue;

            bool isTraversable = false;
            if (link.NodeA == currentNode &&
                (link.Direction == LinkDirection.Right || link.Direction == LinkDirection.Undirected))
            {
                isTraversable = true;
            }
            else if (link.NodeB == currentNode &&
                     (link.Direction == LinkDirection.Left || link.Direction == LinkDirection.Undirected))
            {
                isTraversable = true;
            }

            if (!isTraversable)
                continue;

            if (config != null)
            {
                var linkConfig = config.Links.FirstOrDefault(lc =>
                    (lc.NodeA == link.NodeA && lc.NodeB == link.NodeB) ||
                    (lc.NodeA == link.NodeB && lc.NodeB == link.NodeA));

                if (linkConfig != null)
                {
                    bool hasActiveTransports =
                        (linkConfig.ActiveTransports?.Any() ?? false) ||
                        (linkConfig.EnabledTransports?.Any() ?? false);

                    if (!hasActiveTransports)
                        continue;
                }
            }

            edges.Add(link);
        }

        return edges;
    }

    private string? DetermineNextNode(Link edge, string currentNode)
    {
        if (edge.NodeA == currentNode &&
            (edge.Direction == LinkDirection.Right || edge.Direction == LinkDirection.Undirected))
            return edge.NodeB;

        if (edge.NodeB == currentNode &&
            (edge.Direction == LinkDirection.Left || edge.Direction == LinkDirection.Undirected))
            return edge.NodeA;

        return null;
    }

    private sealed class PathState
    {
        public string CurrentNode { get; set; } = string.Empty;
        public int CurrentGraphIndex { get; set; }
        public long StartTime { get; set; }
        public HashSet<string> VisitedNodes { get; set; } = new();
        public List<string> Nodes { get; set; } = new();
        public List<int> GraphIndices { get; set; } = new();
        public List<EdgeTraversal> Edges { get; set; } = new();
    }
}