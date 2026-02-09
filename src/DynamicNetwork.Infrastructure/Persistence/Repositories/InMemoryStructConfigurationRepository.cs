using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Infrastructure.Persistence.Repositories;

public class InMemoryStructConfigurationRepository : IStructConfigurationRepository
{
    private readonly Dictionary<TimeInterval, StructConfiguration> _configs = new();

    public bool Add(StructConfiguration config)
    {
        if (_configs.ContainsKey(config.Interval))
            return false;

        _configs[config.Interval] = config;
        return true;
    }

    public void Delete(TimeInterval interval)
    {
        _configs.Remove(interval);
    }

    public bool Exists(TimeInterval interval) =>
        _configs.ContainsKey(interval);

    public IReadOnlyList<StructConfiguration> GetAll() =>
        _configs.Values.ToList().AsReadOnly();

    public StructConfiguration? GetByInterval(TimeInterval interval)
    {
        return _configs.TryGetValue(interval, out var config) ? config : null;
    }

    public IReadOnlyList<StructConfiguration> GetByTimeRange(TimeInterval range)
    {
        return _configs
            .Where(kv => kv.Key.Overlaps(range))
            .Select(kv => kv.Value)
            .ToList()
            .AsReadOnly();
    }

    public void Update(StructConfiguration config)
    {
        _configs[config.Interval] = config;
    }
}
