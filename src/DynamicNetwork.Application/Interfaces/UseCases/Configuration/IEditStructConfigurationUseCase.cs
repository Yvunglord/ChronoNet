using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.UseCases.Configuration;

public interface IEditStructConfigurationUseCase
{
    StructConfiguration EditNode(
        TimeInterval interval,
        string nodeId,
        Func<NodeConfiguration, NodeConfiguration> updateFunc);

    StructConfiguration EditLink(
        TimeInterval interval,
        string nodeA,
        string nodeB,
        Func<LinkConfiguration, LinkConfiguration> updateFunc);

    StructConfiguration Edit(TimeInterval interval, StructConfiguration newConfig);
}
