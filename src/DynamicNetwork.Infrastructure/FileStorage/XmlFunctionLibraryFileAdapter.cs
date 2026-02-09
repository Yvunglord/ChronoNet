using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Domain.Functions;
using System.Xml.Linq;

namespace DynamicNetwork.Infrastructure.FileStorage;

public class XmlFunctionLibraryFileAdapter : IFunctionLibraryFilePort
{
    public FunctionLibrary Load(string path)
    {
        var doc = XDocument.Load(path);
        var root = doc.Root?.Element("task")
                   ?? throw new InvalidOperationException("Invalid XML");

        var processes = root.Element("process")?
            .Elements("type")
            .Select(p => new ProcessType(
                id: p.Attribute("id")!.Value,
                timePerUnit: double.Parse(p.Attribute("time")!.Value),
                inputFlowType: p.Element("input")!.Element("type")!.Attribute("id")!.Value,
                outputFlowType: p.Element("output")!.Element("type")!.Attribute("id")!.Value,
                chunkSize: double.Parse(p.Element("input")!.Element("type")!.Attribute("size")!.Value)
            )) ?? Enumerable.Empty<ProcessType>();

        var transports = root.Element("transport")?
            .Elements("type")
            .Select(t => new TransportType(
                id: t.Attribute("id")!.Value,
                time: double.Parse(t.Attribute("time")!.Value),
                flowType: t.Element("input")!.Element("type")!.Attribute("id")!.Value,
                capacity: double.Parse(t.Element("input")!.Element("type")!.Attribute("size")!.Value)
            )) ?? Enumerable.Empty<TransportType>();

        var storages = root.Element("storage")?
            .Elements("type")
            .Select(s => new StorageType(
                s.Attribute("id")!.Value,
                s.Element("input")!.Elements("type")
                    .Select(t => t.Attribute("id")!.Value)
            )) ?? Enumerable.Empty<StorageType>();

        return new FunctionLibrary(processes, transports, storages);
    }

    public void Save(FunctionLibrary library, string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var doc = new XDocument(
            new XElement("function_library",
                new XElement("processes",
                    library.Processes.Select(p => SerializeProcess(p))),
                new XElement("transports",
                    library.Transports.Select(t => SerializeTransport(t))),
                new XElement("storages",
                    library.Storages.Select(s => SerializeStorage(s)))
            )
        );

        if (path != null)
        {
            var directory = Path.GetDirectoryName(path);

            if (directory != null)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            doc.Save(path);
        }
    }

    private XElement SerializeProcess(ProcessType process)
    {
        return new XElement("process",
            new XAttribute("id", process.Id),
            new XAttribute("timePerUnit", process.TimePerUnit),
            new XElement("input",
                new XAttribute("type", process.InputFlowType),
                new XAttribute("size", process.ChunkSize)),
            new XElement("output",
                new XAttribute("type", process.OutputFlowType),
                new XAttribute("size", process.ChunkSize))
        );
    }

    private XElement SerializeTransport(TransportType transport)
    {
        return new XElement("transport",
            new XAttribute("id", transport.Id),
            new XAttribute("time", transport.Time),
            new XAttribute("capacity", transport.Capacity),
            new XElement("flowType", transport.FlowType)
        );
    }

    private XElement SerializeStorage(StorageType storage)
    {
        return new XElement("storage",
            new XAttribute("id", storage.Id),
            storage.AllowedFlowTypes.Select(t =>
                new XElement("allowedType", t))
        );
    }
}
