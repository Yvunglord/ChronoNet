using DynamicNetwork.Application.Interfaces.Ports;
using System.Xml.Linq;

namespace DynamicNetwork.Infrastructure.Adapters.FileStorage;

public class XmlFileStorageAdapter : IFileStoragePort
{
    public void SaveXml(XDocument document, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        document.Save(path);
    }

    public XDocument LoadXml(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}", path);

        return XDocument.Load(path);
    }

    public bool FileExists(string path) => File.Exists(path);
}
