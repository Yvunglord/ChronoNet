using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.UseCases.Configuration;

public interface IManageStructConfigurationUseCase
{
    bool Add(StructConfiguration config);
    void Delete(TimeInterval interval);
    IReadOnlyList<StructConfiguration> GetAll();
    StructConfiguration? GetByInterval(TimeInterval interval);
}
