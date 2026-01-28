using ChronoNet.Application.DTO;
using System.Xml.Linq;

namespace ChronoNet.Infrastructure.Mappings;

public static class StructXmlWriter
{
    public static XElement Write(StructDto dto)
    {
        var structElem = new XElement("struct",
            new XAttribute("id", dto.Id),
            new XAttribute("time", dto.Time),
            new XAttribute("start_time", dto.StartTime),
            new XAttribute("end_time", dto.EndTime));

        foreach (var elem in dto.Elements)
        {
            var e = new XElement("elem", new XAttribute("id", elem.Id));
            foreach (var attr in elem.Attributes)
            {
                e.Add(new XAttribute(attr.Key, attr.Value));
            }

            structElem.Add(e);
        }

        foreach (var link in dto.Links)
        {
            var l = new XElement("link",
                new XAttribute("id1", link.Id1),
                new XAttribute("id2", link.Id2));

            foreach (var attr in link.Attributes)
            {
                l.Add(new XAttribute(attr.Key, attr.Value));
            }

            structElem.Add(l);
        }

        return structElem;
    }
}