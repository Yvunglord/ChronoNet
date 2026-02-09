using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.UseCases.Graphs;

public interface ILoadTemporalGraphsUseCase
{
    IReadOnlyList<TemporalGraph> Execute(string sourcePath);
}
