using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.UseCases.Reachability;

public interface ICheckReachabilityUseCase
{
    ReachabilityResult Execute(
        IReadOnlyList<TemporalGraph> graphs,
        ReachabilityRequest request);
}