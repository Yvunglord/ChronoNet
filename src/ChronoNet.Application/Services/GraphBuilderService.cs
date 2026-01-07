using ChronoNet.Application.DTO;
using ChronoNet.Domain;

namespace ChronoNet.Application.Services;
public static class GraphBuilderService
{
    public static IList<TemporalGraph> BuildGraphs(
        IReadOnlyList<Device> devices,
        IList<RawJsonEdge> rawEdges,
        IList<TimeInterval> intervals
    )
    {
        var deviceMap = devices.ToDictionary(d => d.Name, d => d);
        var graphs = new List<TemporalGraph>();

        for (int i = 0; i < intervals.Count; i++)
        {
            var interval = intervals[i];
            var edges = new HashSet<Edge>();

            foreach (var raw in rawEdges)
            {
                var rawInterval = new TimeInterval(raw.Begin, raw.End);
                if (!rawInterval.IntersectsWith(interval))
                    continue;

                var deviceNames = raw.Attributes
                    .Select(kvp => $"{kvp.Key}{kvp.Value}")
                    .ToList();

                for (int j = 0; j < deviceNames.Count; j++)
                {
                    for (int k = j + 1; k < deviceNames.Count; k++)
                    {
                        var from = deviceMap[deviceNames[j]].Id;
                        var to = deviceMap[deviceNames[k]].Id;
                        edges.Add(new Edge(from, to));
                    }
                }
            }

            graphs.Add(new TemporalGraph(i, interval, devices, edges.ToList()));
        }

        return graphs;
    }
}