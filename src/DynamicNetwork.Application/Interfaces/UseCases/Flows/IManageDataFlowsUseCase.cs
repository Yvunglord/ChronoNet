using DynamicNetwork.Domain.Flows;

namespace DynamicNetwork.Application.Interfaces.UseCases.Flows;

public interface IManageDataFlowsUseCase
{
    bool Add(DataFlow flow);
    DataFlow? GetById(string id);
    IReadOnlyList<DataFlow> GetAll();
    void Delete(string id);

    DataFlow UpdateVolume(string id, double newVolume);
    DataFlow AppendTransformation(string id, FlowTransformation transformation);
}
