using DynamicNetwork.Application.Interfaces.Services;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Functions;
using System.Xml.Linq;

namespace DynamicNetwork.Infrastructure.Adapters.FileStorage;

public class XmlConfigurationExportAdapter : IConfigurationExportService
{
    public XDocument Export(
        IEnumerable<StructConfiguration> configurations,
        FunctionLibrary library,
        IEnumerable<DataFlow> flows)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("XMLDocument",
                new XAttribute("version", "1.0"),
                new XElement("comment"),
                new XElement("task",
                    BuildFlows(flows),
                    BuildProcesses(library),
                    BuildTransports(library),
                    BuildStorages(library),
                    BuildStructs(configurations),
                    BuildStaticPart(),
                    BuildCriterionPart(),
                    BuildConstraintsPart()
                )
            )
        );
    }

    private XElement BuildFlows(IEnumerable<DataFlow> flows)
    {
        var flowTypes = flows
            .SelectMany(f => f.Transformations
                .SelectMany(t => new[] { t.InputType, t.OutputType }))
            .Distinct();

        return new XElement("flows",
            flowTypes.Select(t =>
                new XElement("type",
                    new XAttribute("id", t))));
    }

    private XElement BuildProcesses(FunctionLibrary library)
    {
        return new XElement("process",
            library.Processes.Select(p =>
                new XElement("type",
                    new XAttribute("id", p.Id),
                    new XAttribute("time", p.TimePerUnit),
                    new XElement("input",
                        new XElement("type",
                            new XAttribute("id", p.InputFlowType),
                            new XAttribute("size", p.ChunkSize))),
                        new XElement("output",
                            new XElement("type",
                                new XAttribute("id", p.OutputFlowType),
                                new XAttribute("size", p.ChunkSize)))
                )));
    }

    private XElement BuildTransports(FunctionLibrary library)
    {
        return new XElement("transport",
            library.Transports.Select(t =>
                new XElement("type",
                    new XAttribute("id", t.Id),
                    new XAttribute("time", t.Time),
                    new XElement("input",
                        new XElement("type",
                            new XAttribute("id", t.FlowType),
                            new XAttribute("size", t.Capacity))),
                        new XElement("output",
                            new XElement("type",
                                new XAttribute("id", t.FlowType),
                                new XAttribute("size", t.Capacity)))
                )));
    }

    private XElement BuildStorages(FunctionLibrary library)
    {
        return new XElement("storage",
            library.Storages.Select(s =>
                new XElement("type",
                    new XAttribute("id", s.Id),
                    new XElement("input",
                        s.AllowedFlowTypes.Select(t =>
                            new XElement("type",
                            new XAttribute("id", t)))
                    )
                )));
    }

    private XElement BuildStructs(IEnumerable<StructConfiguration> structs)
    {
        int structId = 1;

        return new XElement("structs",
            structs.Select(s =>
            {
                var structElement = new XElement("struct",
                    new XAttribute("id", structId++),
                    new XAttribute("time", s.Interval.Duration),
                    new XAttribute("start_time", s.Interval.Start),
                    new XAttribute("end_time", s.Interval.End));

                structElement.Add(BuildStructElements(s));
                structElement.Add(BuildStructLinks(s));

                return structElement;
            }));
    }

    private IEnumerable<XElement> BuildStructElements(StructConfiguration structConfig)
    {
        return structConfig.Nodes.Select(n =>
        {
            var elem = new XElement("elem",
                new XAttribute("id", n.NodeId));

            foreach (var input in n.Inputs)
                elem.Add(new XAttribute($"input_{input}", string.Empty));

            foreach (var output in n.Outputs)
                elem.Add(new XAttribute($"output_{output}", string.Empty));

            foreach (var process in n.EnabledProcesses)
                elem.Add(new XAttribute($"process_{process}", string.Empty));

            foreach (var storage in n.StorageCapacities)
                elem.Add(new XAttribute($"storage_{storage.Key}", storage.Value));

            return elem;
        });
    }

    private IEnumerable<XElement> BuildStructLinks(StructConfiguration structConfig)
    {
        return structConfig.Links.Select(l =>
        {
            var link = new XElement("link",
                new XAttribute("id1", l.NodeA),
                new XAttribute("id2", l.NodeB));

            foreach (var transport in l.EnabledTransports)
            {
                link.Add(new XAttribute($"transport_{transport}", string.Empty));
            }

            return link;
        });
    }

    private XElement BuildStaticPart()
    {
        return XElement.Parse(@"
                <selectors>
                    <selector id='1' sign='0.4'>
                        <resultflow flow='2' interval='*' object='*' sign='1.0'/>
                    </selector>
                    <selector id='2' sign='0.35'>
                        <resultflow flow='1' interval='*' object='*' sign='1.0'/>
                    </selector>
                    <selector id='3' sign='-0.3'>
                        <lost flow='*' interval='*' object='*' sign='1.0'/>
                    </selector>
                </selectors>");
    }

    private XElement BuildCriterionPart()
    {
        return XElement.Parse(@"
                <criterion sign='MAX'>
                    <selector id='1'/>
                    <selector id='2'/>
                    <selector id='3'/>
                </criterion>");
    }

    private XElement BuildConstraintsPart()
    {
        return XElement.Parse(@"<constraints/>");
    }
}
