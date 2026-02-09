using DynamicNetwork.Domain.Functions;

namespace DynamicNetwork.Application.Interfaces.UseCases.Library;

public interface IManageFunctionLibraryUseCase
{
    void AddProcesses(IEnumerable<ProcessType> processes);
    void RemoveProcesses(IEnumerable<string> processIds);
    void UpdateProcesses(IEnumerable<ProcessType> processes);
    ProcessType? GetProcessById(string processId);
    List<ProcessType>? GetProcesses();

    void AddTransports(IEnumerable<TransportType> transports);
    void RemoveTransports(IEnumerable<string> transportIds);
    void UpdateTransports(IEnumerable<TransportType> transports);
    TransportType? GetTransportById(string transportId);
    List<TransportType>? GetTransports();

    void AddStorages(IEnumerable<StorageType> storages);
    void RemoveStorages(IEnumerable<string> storageIds);
    void UpdateStorages(IEnumerable<StorageType> storages);
    StorageType? GetStorageById(string storageId);
    List<StorageType>? GetStorages();
}
