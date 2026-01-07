using System.Text.Json;
using ChronoNet.Application.DTO;

namespace ChronoNet.Infrastructure.Json;
public static class JsonGraphParser
{
    public static IList<RawJsonEdge> Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var result = new List<RawJsonEdge>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var edge = new RawJsonEdge();

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.NameEquals("begin"))
                    edge.Begin = prop.Value.GetInt64();
                else if (prop.NameEquals("end"))
                    edge.End = prop.Value.GetInt64();
                else 
                    edge.Attributes[prop.Name] = prop.Value.GetString()!;
            }

            result.Add(edge);
        }

        return result;
    }
}