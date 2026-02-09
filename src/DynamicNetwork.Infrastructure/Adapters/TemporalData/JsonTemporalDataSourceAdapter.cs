using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.Ports;
using System.Text.Json;

namespace DynamicNetwork.Infrastructure.Adapters.TemporalData;

public class JsonTemporalDataSourceAdapter : ITemporalDataSourcePort
{
    public IReadOnlyCollection<LinkParsingDto> LoadRawLinks(string sourcePath)
    {
        var json = File.ReadAllText(sourcePath);
        using var doc = JsonDocument.Parse(json);

        var links = new List<LinkParsingDto>();

        foreach (var elem in doc.RootElement.EnumerateArray())
        {
            var link = new LinkParsingDto();
            var nodeNames = new List<string>();

            foreach (var prop in elem.EnumerateObject())
            {
                if (prop.NameEquals("begin"))
                    link.Begin = prop.Value.GetInt64();
                else if (prop.NameEquals("end"))
                    link.End = prop.Value.GetInt64();
                else
                    nodeNames.Add($"{prop.Name}{prop.Value}");
            }

            if (nodeNames.Count >= 2)
            {
                link.NodeA = nodeNames[0];
                link.NodeB = nodeNames[1];
                links.Add(link);
            }
        }

        return links.AsReadOnly();
    }
}
