using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.Repositories;

public interface IStructConfigurationRepository
{
    bool Add(StructConfiguration config);
    void Update(StructConfiguration config);
    void Delete(TimeInterval interval);
    StructConfiguration? GetByInterval(TimeInterval interval);
    IReadOnlyList<StructConfiguration> GetByTimeRange(TimeInterval range);
    IReadOnlyList<StructConfiguration> GetAll();
    bool Exists(TimeInterval interval);
}
