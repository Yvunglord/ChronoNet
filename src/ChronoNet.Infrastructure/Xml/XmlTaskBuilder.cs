using ChronoNet.Domain;
using ChronoNet.Domain.Flows;
using ChronoNet.Domain.Processes;
using ChronoNet.Domain.Storage;
using ChronoNet.Domain.Transport;
using ChronoNet.Infrastructure.Mappings;
using System.Xml.Linq;

namespace ChronoNet.Infrastructure.Xml;

public class XmlTaskBuilder
{
    public XDocument Build(
        IReadOnlyList<TemporalGraph> graphs,
        StaticModel staticModel,
        XElement[] staticSelectorBlock)
    {
        var task = new XElement("task");

        task.Add(BuildFlows(staticModel));
        task.Add(BuildProcesses(staticModel));
        task.Add(BuildTransports(staticModel));
        task.Add(BuildStorages(staticModel));

        foreach (var graph in graphs)
        {
            var idMap = BuildDeviceIdMap(graph);
            var structDto = StructMapper.Map(graph, idMap);
            task.Add(StructXmlWriter.Write(structDto));
        }

        for (int i = 0; i < staticSelectorBlock.Length; i++)
        {
            task.Add(staticSelectorBlock[i]);
        }

        return new XDocument(
            new XElement("XMLDocument", task));
    }

    private XElement BuildFlows(StaticModel model)
    {
        var flows = new XElement("flows");

        foreach (var flow in model.Flows)
        {
            flows.Add(
                new XElement("type",
                    new XAttribute("id", flow.Id)));
        }

        return flows;
    }

    private XElement BuildProcesses(StaticModel model)
    {
        var processes = new XElement("processes");

        foreach (var p in model.Processes)
        {
            var type = new XElement("type",
                new XAttribute("id", p.Id),
                new XAttribute("time", p.TimePerChunk)
                );

            var input = new XElement("input");
            foreach (var i in p.InputFlows)
            {
                input.Add(new XElement("type",
                    new XAttribute("id", i.Key),
                    new XAttribute("size", i.Value)));
            }    

            var output = new XElement("output");
            foreach (var o in p.OutputFlows)
            {
                output.Add(new XElement("type",
                    new XAttribute("id", o.Key),
                    new XAttribute("size", o.Value)));
            }

            type.Add(input);
            type.Add(output);
            processes.Add(type);
        }

        return processes;
    }

    private XElement BuildTransports(StaticModel model)
    {
        var transport = new XElement("transport");

        foreach (var t in model.Transports)
        {
            var type = new XElement("type",
                new XAttribute("id", t.Id),
                new XAttribute("time", t.TimePerChunk));

            var input = new XElement("input");
            foreach (var f in t.SupportedFlows)
            {
                input.Add(new XElement("type",
                    new XAttribute("id", f.Key),
                    new XAttribute("size", f.Value)));
            }

            type.Add(input);
            transport.Add(type);
        }

        return transport;
    }

    private XElement BuildStorages(StaticModel model)
    {
        var storages = new XElement("storages");

        foreach (var s in model.Srorages)
        {
            var type = new XElement("type",
                new XAttribute("id", s.Id));

            var input = new XElement("input");  
            foreach (var flowId in s.SupportedFlows)
            {
                input.Add(new XElement("type",
                    new XAttribute("id", flowId)));
            }

            type.Add(input);
            storages.Add(type);
        }

        return storages;
    }

    private Dictionary<Guid, int> BuildDeviceIdMap(TemporalGraph graph)
    {
        var map = new Dictionary<Guid, int>();
        int id = 1;

        foreach (var v in graph.Vertices)
        {
            map[v.Id] = id++;
        }

        return map;
    }
}