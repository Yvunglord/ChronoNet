using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.UseCases.Configuration;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.UseCases.Configuration;

public class EditStructConfigurationUseCase : IEditStructConfigurationUseCase
{
    private readonly IStructConfigurationRepository _repo;

    public EditStructConfigurationUseCase(IStructConfigurationRepository repo)
    {
        _repo = repo;
    }

    public StructConfiguration EditNode(
    TimeInterval interval,
    string nodeId,
    Func<NodeConfiguration, NodeConfiguration> updateFunc)
    {
        var config = _repo.GetByInterval(interval)
            ?? throw new InvalidOperationException($"Configuration for interval {interval} not found");

        var updated = config.WithUpdatedNode(nodeId, updateFunc);

        _repo.Update(updated);
        return updated;
    }

    public StructConfiguration EditLink(
        TimeInterval interval,
        string nodeA,
        string nodeB,
        Func<LinkConfiguration, LinkConfiguration> updateFunc)
    {
        var config = _repo.GetByInterval(interval)
            ?? throw new InvalidOperationException($"Configuration for interval {interval} not found");

        var updated = config.WithUpdatedLink(nodeA, nodeB, updateFunc);
        _repo.Update(updated);
        return updated;
    }

    public StructConfiguration Edit(TimeInterval interval, StructConfiguration newConfig)
    {
        _repo.Update(newConfig);
        return _repo.GetByInterval(interval)!;
    }
}
