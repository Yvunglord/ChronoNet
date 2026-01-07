using ChronoNet.Application.DTO;
using ChronoNet.Domain;

namespace ChronoNet.Application.Services;
public static class IntervalBuilder
{
    public static IList<TimeInterval> Build(IEnumerable<RawJsonEdge> edges)
    {
        var points = new SortedSet<long>();

        foreach (var e in edges)
        {
            points.Add(e.Begin);
            points.Add(e.End);
        }

        var list = points.ToList();
        var intervals = new List<TimeInterval>();

        for (int i = 0; i < list.Count - 1; i++)
        {
            intervals.Add(new TimeInterval(list[i], list[i + 1]));
        }

        return intervals;
    }
}