using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Functions;
using System.Xml.Linq;

namespace DynamicNetwork.Application.Interfaces.Services;

public interface IConfigurationExportService
{
    XDocument Export(
        IEnumerable<StructConfiguration> configurations,
        FunctionLibrary library,
        IEnumerable<DataFlow> flows);
}