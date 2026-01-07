using ChronoNet.Application.DTO;
using ChronoNet.Domain;

namespace ChronoNet.Application.Services;
public static class DeviceExtractor
{
    public static IReadOnlyList<Device> ExtractDevices(IEnumerable<RawJsonEdge> edges)
    {
        var map = new Dictionary<string, Device>();

        foreach (var edge in edges)
        {
            foreach (var kvp in edge.Attributes)
            {
                var name = $"{kvp.Key}{kvp.Value}";
                if (!map.ContainsKey(name))
                    map[name] = new Device(name);
            }
        }

        return map.Values.ToList();
    }
}