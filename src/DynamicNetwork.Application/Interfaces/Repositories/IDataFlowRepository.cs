namespace DynamicNetwork.Application.Interfaces.Repositories;

public interface IDataFlowRepository
{
    bool Add(DataFlow flow);
    DataFlow? GetById(string id);
    void Update(DataFlow flow);
    void Delete(string id);
    IReadOnlyList<DataFlow> GetAll();
    bool Exists(string id);
}
