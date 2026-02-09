using System.Xml.Linq;

namespace DynamicNetwork.Application.Interfaces.Ports;

public interface IFileStoragePort
{
    void SaveXml(XDocument document, string path);
    XDocument LoadXml(string path);
    bool FileExists(string path);
}