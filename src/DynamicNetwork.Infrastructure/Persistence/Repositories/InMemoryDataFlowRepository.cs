using DynamicNetwork.Application.Interfaces.Repositories;

namespace DynamicNetwork.Infrastructure.Persistence.Repositories;

public class InMemoryDataFlowRepository : IDataFlowRepository
{
    private readonly Dictionary<string, DataFlow> _flows = new();

    public bool Add(DataFlow flow)
    {
        if (_flows.ContainsKey(flow.Id))
            return false;

        _flows[flow.Id] = flow;
        return true;
    }

    public DataFlow? GetById(string id) =>
    _flows.TryGetValue(id, out var flow) ? flow : null;

    public void Update(DataFlow flow)
    {
        _flows[flow.Id] = flow;
    }

    public void Delete(string id)
    {
        _flows.Remove(id);
    }

    public IReadOnlyList<DataFlow> GetAll() =>
        _flows.Values.ToList().AsReadOnly();

    public bool Exists(string id) =>
        _flows.ContainsKey(id);
}
