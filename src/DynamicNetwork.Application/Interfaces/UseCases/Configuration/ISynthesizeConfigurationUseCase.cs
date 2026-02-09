using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Synthesis;

namespace DynamicNetwork.Application.Interfaces.UseCases.Configuration;

public interface ISynthesizeConfigurationUseCase
{
    IReadOnlyList<StructConfiguration> Execute(
        StructConfigurationRequestDto request,
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<DataFlow> flows);
}
