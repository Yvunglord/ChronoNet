using ChronoNet.Application.DTO;
using ChronoNet.Domain;

namespace ChronoNet.Infrastructure.Mappings;

public static class StructMapper
{
    public static StructDto Map(TemporalGraph graph, Dictionary<Guid, int> deviceIdMap)
    {
        return new StructDto
        {
            Id = graph.Index,
            StartTime = graph.Interval.Start,
            EndTime = graph.Interval.End,
            Time = graph.Interval.Duration,
            Elements = MapElements(graph, deviceIdMap),
            Links = MapLinks(graph, deviceIdMap)
        };
    }

    private static List<ElemDto> MapElements(
        TemporalGraph graph,
        Dictionary<Guid, int> deviceIdMap)
    {
        var result = new List<ElemDto>();

        foreach (var device in graph.Vertices)
        {
            var dto = new ElemDto
            {
                Id = deviceIdMap[device.Id]
            };

            var info = graph.DeviceInfos[device.Id];

            foreach (var input in info.IncomingFlows)
            {
                dto.Attributes[$"input_{input.Key}"] = input.Value.ToString("F");
            }

            foreach (var output in info.OutgoingFlows)
            {
                dto.Attributes[$"output_{output.Key}"] = output.Value.ToString("F");
            }

            foreach (var procId in device.SupportedProcessTypes)
            {
                dto.Attributes[$"process_{procId}"] = "";
            }

            foreach (var storage in device.StorageCapacities)
            {
                dto.Attributes[$"storage_{storage.Key}"] = storage.Value.ToString("F");
            }

            result.Add(dto);
        }

        return result;
    }

    private static List<LinkDto> MapLinks(
        TemporalGraph graph,
        Dictionary<Guid, int> deviceIdMap)
    {
        var result = new List<LinkDto>();

        foreach (var edge in graph.Edges)
        {
            var dto = new LinkDto
            {
                Id1 = deviceIdMap[edge.From],
                Id2 = deviceIdMap[edge.To]
            };

            foreach (var transportId in edge.SupportedTransprtTypes)
            {
                dto.Attributes[$"transport_{transportId}"] = "";
            }

            result.Add(dto);
        }

        return result;
    }
}