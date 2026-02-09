using DynamicNetwork.Application.Interfaces.Factories;
using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Application.Interfaces.Session;
using DynamicNetwork.Domain.Enums;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Session;

public class GraphSessionManager : IGraphSessionManager
{
    private readonly ITemporalDataSourcePort _dataSource;
    private readonly ITemporalGraphFactory _graphFactory;
    private readonly Dictionary<string, GraphSession> _sessions = new();
    private int _sessionCounter = 0;

    public GraphSessionManager(
        ITemporalDataSourcePort dataSource,
        ITemporalGraphFactory graphFactory)
    {
        _dataSource = dataSource;
        _graphFactory = graphFactory;
    }

    public string CreateSession(string sourcePath)
    {
        var rawLinks = _dataSource.LoadRawLinks(sourcePath);
        var graphs = _graphFactory.BuildGraphs(rawLinks);

        var sessionId = $"session_{++_sessionCounter}";
        _sessions[sessionId] = new GraphSession(graphs);
        return sessionId;
    }

    public IReadOnlyList<TemporalGraph> GetGraphs(string sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session)
            ? session.Graphs
            : throw new KeyNotFoundException($"Session {sessionId} not found");
    }

    public TemporalGraph UpdateLinkDirection(
        string sessionId,
        int graphIndex,
        string nodeA,
        string nodeB,
        LinkDirection newDirection)
    {
        var session = _sessions[sessionId];
        var graph = session.Graphs[graphIndex];

        var link = graph.Links.FirstOrDefault(l =>
            (l.NodeA == nodeA && l.NodeB == nodeB) ||
            (l.NodeA == nodeB && l.NodeB == nodeA))
            ?? throw new InvalidOperationException($"Link between {nodeA} and {nodeB} not found");

        var updatedLinks = graph.Links
            .Select(l => l == link ? l.WithDirection(newDirection) : l)
            .ToList();

        var updatedGraph = new TemporalGraph(
            graph.Index,
            graph.Interval,
            updatedLinks,
            graph.AllNetworkNodes);

        var updatedGraphs = session.Graphs.ToList();
        updatedGraphs[graphIndex] = updatedGraph;
        _sessions[sessionId] = new GraphSession(updatedGraphs);

        return updatedGraph;
    }

    public TemporalGraph UpdateLinkDirectionCycled(
        string sessionId,
        int graphIndex,
        string nodeA,
        string nodeB)
    {
        var session = _sessions[sessionId];
        var graph = session.Graphs[graphIndex];

        var link = graph.Links.FirstOrDefault(l =>
            (l.NodeA == nodeA && l.NodeB == nodeB) ||
            (l.NodeA == nodeB && l.NodeB == nodeA))
            ?? throw new InvalidOperationException($"Link between {nodeA} and {nodeB} not found");

        var updatedLinks = graph.Links
            .Select(l => l == link ? l.WithCycledDirection() : l)
            .ToList();

        var updatedGraph = new TemporalGraph(
            graph.Index,
            graph.Interval,
            updatedLinks,
            graph.AllNetworkNodes);

        var updatedGraphs = session.Graphs.ToList();
        updatedGraphs[graphIndex] = updatedGraph;
        _sessions[sessionId] = new GraphSession(updatedGraphs);

        return updatedGraph;
    }

    public void CloseSession(string sessionId)
    {
        _sessions.Remove(sessionId);
    }

    private class GraphSession
    {
        public IReadOnlyList<TemporalGraph> Graphs { get; }

        public GraphSession(IReadOnlyList<TemporalGraph> graphs)
        {
            Graphs = graphs;
        }
    }
}
