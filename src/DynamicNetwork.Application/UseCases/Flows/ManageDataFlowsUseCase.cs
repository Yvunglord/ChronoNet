using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.UseCases.Flows;
using DynamicNetwork.Domain.Flows;

namespace DynamicNetwork.Application.UseCases.Flows;

public class ManageDataFlowsUseCase : IManageDataFlowsUseCase
{
    private readonly IDataFlowRepository _repo;

    public ManageDataFlowsUseCase(IDataFlowRepository repo)
    {
        _repo = repo;
    }

    public bool Add(DataFlow flow) => _repo.Add(flow);
    public DataFlow? GetById(string id) => _repo.GetById(id);
    public IReadOnlyList<DataFlow> GetAll() => _repo.GetAll();
    public void Delete(string id) => _repo.Delete(id);

    public DataFlow UpdateVolume(string id, double newVolume)
    {
        var flow = _repo.GetById(id)
            ?? throw new InvalidOperationException($"Flow with ID '{id}' not found");

        var updated = flow.WithVolume(newVolume);

        _repo.Update(updated);
        return updated;
    }

    public DataFlow AppendTransformation(string id, FlowTransformation transformation)
    {
        var flow = _repo.GetById(id)
            ?? throw new InvalidOperationException($"Flow with ID '{id}' not found");

        var updated = flow.AppendTransformation(transformation);
        _repo.Update(updated);
        return updated;
    }
}
