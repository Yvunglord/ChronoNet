using DynamicNetwork.Application.Dtos;

namespace DynamicNetwork.Application.Interfaces.Ports;

public interface ITemporalDataSourcePort
{
    IReadOnlyCollection<LinkParsingDto> LoadRawLinks(string path);
}
