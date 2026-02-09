using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.UseCases.Configuration;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.UseCases.Configuration;

public class ManageStructConfigurationUseCase : IManageStructConfigurationUseCase
{
    private readonly IStructConfigurationRepository _repo;

    public ManageStructConfigurationUseCase(IStructConfigurationRepository repo)
    {
        _repo = repo;
    }

    public bool Add(StructConfiguration config) => _repo.Add(config);
    public void Delete(TimeInterval interval) => _repo.Delete(interval);
    public IReadOnlyList<StructConfiguration> GetAll() => _repo.GetAll();
    public StructConfiguration? GetByInterval(TimeInterval interval) => _repo.GetByInterval(interval);
}
