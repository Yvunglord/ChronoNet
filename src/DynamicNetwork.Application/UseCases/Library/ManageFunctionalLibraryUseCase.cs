using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.UseCases.Library;
using DynamicNetwork.Domain.Functions;
using System.Diagnostics;

namespace DynamicNetwork.Application.UseCases.Library;

public class ManageFunctionalLibraryUseCase : IManageFunctionLibraryUseCase
{
    private readonly IFunctionLibraryProvider _provider;

    public ManageFunctionalLibraryUseCase(IFunctionLibraryProvider provider)
    {
        _provider = provider;
    }

    public void AddProcesses(IEnumerable<ProcessType> processes)
    {
        var current = _provider.GetCurrent();
        var updated = current.AddProcesses(processes);

        _provider.Update(updated);
    }

    public void AddStorages(IEnumerable<StorageType> storages)
    {
        var current = _provider.GetCurrent();
        var updated = current.AddStorages(storages);

        _provider.Update(updated);
    }

    public void AddTransports(IEnumerable<TransportType> transports)
    {
        var current = _provider.GetCurrent();
        var updated = current.AddTransports(transports);

        _provider.Update(updated);
    }

    public ProcessType? GetProcessById(string processId)
    {
        var current = _provider.GetCurrent();
        return current.GetProcessById(processId);
    }

    public List<ProcessType>? GetProcesses()
    {
        var current = _provider.GetCurrent();
        return current.Processes.ToList();
    }

    public StorageType? GetStorageById(string storageId)
    {
        var current = _provider.GetCurrent();
        return current.GetStorageById(storageId);
    }

    public List<StorageType>? GetStorages()
    {
        var current = _provider.GetCurrent();
        return current.Storages.ToList();
    }

    public TransportType? GetTransportById(string transportId)
    {
        var current = _provider.GetCurrent();
        return current.GetTransportById(transportId);
    }

    public List<TransportType>? GetTransports()
    {
        var current = _provider.GetCurrent();
        return current.Transports.ToList();
    }

    public void RemoveProcesses(IEnumerable<string> processIds)
    {
        var current = _provider.GetCurrent();
        var updated = current.RemoveProcesses(processIds);
        _provider.Update(updated);
    }

    public void RemoveStorages(IEnumerable<string> storageIds)
    {
        var current = _provider.GetCurrent();
        var updated = current.RemoveStorages(storageIds);
        _provider.Update(updated);
    }

    public void RemoveTransports(IEnumerable<string> transportIds)
    {
        var current = _provider.GetCurrent();
        var updated = current.RemoveTransports(transportIds);
        _provider.Update(updated);
    }

    public void UpdateProcesses(IEnumerable<ProcessType> processes)
    {
        var current = _provider.GetCurrent();
        var updated = current.UpdateProcesses(processes);
        _provider.Update(updated);
    }

    public void UpdateStorages(IEnumerable<StorageType> storages)
    {
        var current = _provider.GetCurrent();
        var updated = current.UpdateStorages(storages);
        _provider.Update(updated);
    }

    public void UpdateTransports(IEnumerable<TransportType> transports)
    {
        var current = _provider.GetCurrent();
        var updated = current.UpdateTransports(transports);
        _provider.Update(updated);
    }
}
