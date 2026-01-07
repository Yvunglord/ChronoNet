using ChronoNet.Domain.Enums;

namespace ChronoNet.Domain;
public class TemporalGraph
{
    public int Index { get; }
    public TimeInterval Interval { get; }

    public IReadOnlyList<Device> Vertices { get; }
    public IList<Edge> Edges { get; }
    public Dictionary<Guid, TemporalDeviceInfo> DeviceInfos { get; }
    public int EdgeCount => Edges.Count;

    public TemporalGraph(
        int index,
        TimeInterval interval,
        IReadOnlyList<Device> vertices,
        IList<Edge> edges
    )
    {
        Index = index;
        Interval = interval;
        Vertices = vertices;
        Edges = edges;
        DeviceInfos = vertices.ToDictionary(v => v.Id, v => new TemporalDeviceInfo(v.Id));
    }

    public void SetLocalCapability(Guid deviceId, LocalCapabilities capability)
    {
        if (DeviceInfos.ContainsKey(deviceId))
            DeviceInfos[deviceId].LocalCapabilities = capability;
    }

    public LocalCapabilities GetLocalCapabilities(Guid deviceId)
    {
        return DeviceInfos.TryGetValue(deviceId, out var info)
            ? info.LocalCapabilities
            : LocalCapabilities.None;
    }
}