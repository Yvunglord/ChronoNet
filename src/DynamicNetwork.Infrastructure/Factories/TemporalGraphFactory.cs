using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.Factories;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Infrastructure.Factories;

public class TemporalGraphFactory : ITemporalGraphFactory
{
    public IReadOnlyList<TemporalGraph> BuildGraphs(
    IEnumerable<LinkParsingDto> rawLinks)
    {
        var nodes = ExtractNodes(rawLinks);
        var intervals = BuildIntervals(rawLinks);
        var graphs = BuildGraphs(nodes, rawLinks, intervals);

        return graphs;
    }

    private IReadOnlyCollection<string> ExtractNodes(
     IEnumerable<LinkParsingDto> links)
    {
        var nodes = new HashSet<string>();
        foreach (var link in links)
        {
            nodes.Add(link.NodeA);
            nodes.Add(link.NodeB);
        }
        return nodes.ToList().AsReadOnly();
    }

    private IReadOnlyList<TimeInterval> BuildIntervals(
    IEnumerable<LinkParsingDto> links)
    {
        var points = new SortedSet<long>();
        foreach (var link in links)
        {
            points.Add(link.Begin);
            points.Add(link.End);
        }

        var intervals = new List<TimeInterval>();
        var list = points.ToList();

        for (int i = 0; i < list.Count - 1; i++)
        {
            intervals.Add(new TimeInterval(list[i], list[i + 1]));
        }

        return intervals.AsReadOnly();
    }

    private IReadOnlyList<TemporalGraph> BuildGraphs(
     IReadOnlyCollection<string> nodes,
     IEnumerable<LinkParsingDto> rawLinks,
     IReadOnlyList<TimeInterval> intervals)
    {
        var graphs = new List<TemporalGraph>();

        for (int i = 0; i < intervals.Count; i++)
        {
            var interval = intervals[i];
            var activeLinks = rawLinks
                .Where(l => l.Begin <= interval.Start && l.End >= interval.End)
                .Select(l => new Link(l.NodeA, l.NodeB))
                .ToList();

            var graph = new TemporalGraph(i, interval, activeLinks, nodes);
            graphs.Add(graph);
        }

        return graphs.AsReadOnly();
    }
}
