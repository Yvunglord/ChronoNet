using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.UseCases;
using DynamicNetwork.Application.Interfaces.UseCases.Library;
using DynamicNetwork.Domain.Functions;
using DynamicNetwork.Presentation.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DynamicNetwork.Presentation.ViewModels;

public class FunctionalLibraryViewModel : ViewModelBase
{
    private readonly MainViewModel _parent;
    private readonly IFunctionLibraryProvider _libraryProvider;
    private readonly IManageFunctionLibraryUseCase _manageLibraryUseCase;

    private string? _processId;
    private double _timeProcess;
    private string? _inputFlowType;
    private string? _outputFlowType;
    private double _processChunkSize;
    private ProcessType? _selectedProcess;

    private string? _transportId;
    private double _transportTime;
    private string? _transportFlowType;
    private double _transportCapacity;
    private TransportType? _selectedTransport;

    private string? _storageId;
    private string? _storageAllowedFlowType;
    private StorageType? _selectedStorage;

    public ObservableCollection<ProcessType> Processes { get; }
        = new ObservableCollection<ProcessType>();

    public ObservableCollection<TransportType> Transports { get; }
        = new ObservableCollection<TransportType>();

    public ObservableCollection<StorageType> Storages { get; }
        = new ObservableCollection<StorageType>();

    public string? ProcessId
    {
        get => _processId;
        set => SetField(ref _processId, value);
    }

    public double TimeProcess
    {
        get => _timeProcess;
        set => SetField(ref _timeProcess, value);
    }

    public string? InputFlowType
    {
        get => _inputFlowType;
        set => SetField(ref _inputFlowType, value);
    }

    public string? OutputFlowType
    {
        get => _outputFlowType;
        set => SetField(ref _outputFlowType, value);
    }

    public double ProcessChunkSize
    {
        get => _processChunkSize;
        set => SetField(ref _processChunkSize, value);
    }

    public ProcessType? SelectedProcess
    {
        get => _selectedProcess;
        set
        {
            SetField(ref _selectedProcess, value);
            if (value != null)
            {
                ProcessId = value.Id;
                TimeProcess = value.TimePerUnit;
                InputFlowType = value.InputFlowType;
                OutputFlowType = value.OutputFlowType;
                ProcessChunkSize = value.ChunkSize;
            }
        }
    }

    public string? TransportId
    {
        get => _transportId;
        set => SetField(ref _transportId, value);
    }

    public double TransportTime
    {
        get => _transportTime;
        set => SetField(ref _transportTime, value);
    }

    public string? TransportFlowType
    {
        get => _transportFlowType;
        set => SetField(ref _transportFlowType, value);
    }

    public double TransportCapacity
    {
        get => _transportCapacity;
        set => SetField(ref _transportCapacity, value);
    }

    public TransportType? SelectedTransport
    {
        get => _selectedTransport;
        set
        {
            SetField(ref _selectedTransport, value);
            if (value != null)
            {
                TransportId = value.Id;
                TransportTime = value.Time;
                TransportFlowType = value.FlowType;
                TransportCapacity = value.Capacity;
            }
        }
    }

    public string? StorageId
    {
        get => _storageId;
        set => SetField(ref _storageId, value);
    }

    public string? StorageAllowedFlowType
    {
        get => _storageAllowedFlowType;
        set => SetField(ref _storageAllowedFlowType, value);
    }

    public StorageType? SelectedStorage
    {
        get => _selectedStorage;
        set
        {
            SetField(ref _selectedStorage, value);
            if (value != null)
            {
                StorageId = value.Id;
                StorageAllowedFlowType = string.Join(", ", value.AllowedFlowTypes);
            }
        }
    }

    public ICommand AddProcessCommand => new RelayCommand(AddProcess);
    public ICommand RemoveProcessCommand => new RelayCommand(RemoveProcess, CanRemoveProcess);
    public ICommand UpdateProcessCommand => new RelayCommand(UpdateProcess, CanUpdateProcess);

    public ICommand AddTransportCommand => new RelayCommand(AddTransport);
    public ICommand RemoveTransportCommand => new RelayCommand(RemoveTransport, CanRemoveTransport);
    public ICommand UpdateTransportCommand => new RelayCommand(UpdateTransport, CanUpdateTransport);

    public ICommand AddStorageCommand => new RelayCommand(AddStorage);
    public ICommand RemoveStorageCommand => new RelayCommand(RemoveStorage, CanRemoveStorage);
    public ICommand UpdateStorageCommand => new RelayCommand(UpdateStorage, CanUpdateStorage);

    public FunctionalLibraryViewModel(
        MainViewModel parent,
        IFunctionLibraryProvider libraryProvider,
        IManageFunctionLibraryUseCase manageLibraryUseCase)
    {
        _parent = parent;
        _libraryProvider = libraryProvider;
        _manageLibraryUseCase = manageLibraryUseCase;
        LoadLibraryFromProvider();
    }

    private void LoadLibraryFromProvider()
    {
        var library = _libraryProvider.GetCurrent();

        Processes.Clear();
        foreach (var process in library.Processes)
        {
            Processes.Add(process);
        }

        Transports.Clear();
        foreach (var transport in library.Transports)
        {
            Transports.Add(transport);
        }

        Storages.Clear();
        foreach (var storage in library.Storages)
        {
            Storages.Add(storage);
        }

        OnPropertyChanged(nameof(Processes));
        OnPropertyChanged(nameof(Transports));
        OnPropertyChanged(nameof(Storages));
    }

    #region Processes Management

    private void AddProcess()
    {
        if (!IsProcessFilled())
            return;

        if (Processes.Any(p => p.Id == ProcessId))
        {
            _parent.DialogService.ShowError($"Процесс с ID '{ProcessId}' уже существует");
            return;
        }

        try
        {
            var process = new ProcessType(
                ProcessId!,
                TimeProcess,
                InputFlowType!,
                OutputFlowType!,
                ProcessChunkSize
            );

            _manageLibraryUseCase.AddProcesses(new[] { process });

            LoadLibraryFromProvider();
            ClearProcessForm();
            _parent.DialogService.ShowInfo($"Процесс '{process.Id}' добавлен");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка добавления процесса: {ex.Message}");
        }
    }

    private void RemoveProcess()
    {
        if (_selectedProcess == null)
            return;

        try
        {
            _manageLibraryUseCase.RemoveProcesses(new[] { _selectedProcess.Id });
            LoadLibraryFromProvider();
            SelectedProcess = null;
            ClearProcessForm();
            _parent.DialogService.ShowInfo($"Процесс '{_selectedProcess.Id}' удалён");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка удаления процесса: {ex.Message}");
        }
    }

    private void UpdateProcess()
    {
        if (_selectedProcess == null || !IsProcessFilled())
            return;

        try
        {
            var updatedProcess = new ProcessType(
                ProcessId!,
                TimeProcess,
                InputFlowType!,
                OutputFlowType!,
                ProcessChunkSize
            );

            _manageLibraryUseCase.UpdateProcesses(new[] { updatedProcess });
            LoadLibraryFromProvider();
            var updated = Processes.FirstOrDefault(p => p.Id == updatedProcess.Id);
            if (updated != null)
                SelectedProcess = updated;

            _parent.DialogService.ShowInfo($"Процесс '{updatedProcess.Id}' обновлён");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка обновления процесса: {ex.Message}");
        }
    }

    private bool CanRemoveProcess() => _selectedProcess != null;
    private bool CanUpdateProcess() => _selectedProcess != null && IsProcessFilled();
    private bool IsProcessFilled() => !string.IsNullOrWhiteSpace(_processId) &&
                                     !string.IsNullOrWhiteSpace(_inputFlowType) &&
                                     !string.IsNullOrWhiteSpace(_outputFlowType);

    private void ClearProcessForm()
    {
        ProcessId = null;
        TimeProcess = 0;
        InputFlowType = null;
        OutputFlowType = null;
        ProcessChunkSize = 0;
    }

    #endregion

    #region Transports Management

    private void AddTransport()
    {
        if (!IsTransportFilled())
            return;

        if (Transports.Any(t => t.Id == TransportId))
        {
            _parent.DialogService.ShowError($"Транспорт с ID '{TransportId}' уже существует");
            return;
        }

        try
        {
            var transport = new TransportType(
                TransportId!,
                TransportTime,
                TransportFlowType!,
                TransportCapacity
            );

            _manageLibraryUseCase.AddTransports(new[] { transport });
            LoadLibraryFromProvider();
            ClearTransportForm();
            _parent.DialogService.ShowInfo($"Транспорт '{transport.Id}' добавлен");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка добавления транспорта: {ex.Message}");
        }
    }

    private void RemoveTransport()
    {
        if (_selectedTransport == null)
            return;

        try
        {
            _manageLibraryUseCase.RemoveTransports(new[] { _selectedTransport.Id });
            LoadLibraryFromProvider();
            SelectedTransport = null;
            ClearTransportForm();
            _parent.DialogService.ShowInfo($"Транспорт '{_selectedTransport.Id}' удалён");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка удаления транспорта: {ex.Message}");
        }
    }

    private void UpdateTransport()
    {
        if (_selectedTransport == null || !IsTransportFilled())
            return;

        try
        {
            var updatedTransport = new TransportType(
                TransportId!,
                TransportTime,
                TransportFlowType!,
                TransportCapacity
            );

            _manageLibraryUseCase.UpdateTransports(new[] { updatedTransport });
            LoadLibraryFromProvider();
            var updated = Transports.FirstOrDefault(t => t.Id == updatedTransport.Id);
            if (updated != null)
                SelectedTransport = updated;

            _parent.DialogService.ShowInfo($"Транспорт '{updatedTransport.Id}' обновлён");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка обновления транспорта: {ex.Message}");
        }
    }

    private bool CanRemoveTransport() => _selectedTransport != null;
    private bool CanUpdateTransport() => _selectedTransport != null && IsTransportFilled();
    private bool IsTransportFilled() => !string.IsNullOrWhiteSpace(_transportId) &&
                                       !string.IsNullOrWhiteSpace(_transportFlowType);

    private void ClearTransportForm()
    {
        TransportId = null;
        TransportTime = 0;
        TransportFlowType = null;
        TransportCapacity = 0;
    }

    #endregion

    #region Storages Management

    private void AddStorage()
    {
        if (!IsStorageFilled())
            return;

        if (Storages.Any(s => s.Id == StorageId))
        {
            _parent.DialogService.ShowError($"Хранилище с ID '{StorageId}' уже существует");
            return;
        }

        try
        {
            var allowedTypes = ParseStorageTypes(StorageAllowedFlowType);
            var storage = new StorageType(StorageId!, allowedTypes);

            _manageLibraryUseCase.AddStorages(new[] { storage });
            LoadLibraryFromProvider();
            ClearStorageForm();
            _parent.DialogService.ShowInfo($"Хранилище '{storage.Id}' добавлено");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка добавления хранилища: {ex.Message}");
        }
    }

    private void RemoveStorage()
    {
        if (_selectedStorage == null)
            return;

        try
        {
            _manageLibraryUseCase.RemoveStorages(new[] { _selectedStorage.Id });
            LoadLibraryFromProvider();
            SelectedStorage = null;
            ClearStorageForm();
            _parent.DialogService.ShowInfo($"Хранилище '{_selectedStorage.Id}' удалено");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка удаления хранилища: {ex.Message}");
        }
    }

    private void UpdateStorage()
    {
        if (_selectedStorage == null || !IsStorageFilled())
            return;

        try
        {
            var allowedTypes = ParseStorageTypes(StorageAllowedFlowType);
            var updatedStorage = new StorageType(StorageId!, allowedTypes);

            _manageLibraryUseCase.UpdateStorages(new[] { updatedStorage });
            LoadLibraryFromProvider();
            var updated = Storages.FirstOrDefault(s => s.Id == updatedStorage.Id);
            if (updated != null)
                SelectedStorage = updated;

            _parent.DialogService.ShowInfo($"Хранилище '{updatedStorage.Id}' обновлено");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка обновления хранилища: {ex.Message}");
        }
    }

    private bool CanRemoveStorage() => _selectedStorage != null;
    private bool CanUpdateStorage() => _selectedStorage != null && IsStorageFilled();
    private bool IsStorageFilled() => !string.IsNullOrWhiteSpace(_storageId) &&
                                     !string.IsNullOrWhiteSpace(_storageAllowedFlowType);

    private void ClearStorageForm()
    {
        StorageId = null;
        StorageAllowedFlowType = null;
    }

    private IEnumerable<string> ParseStorageTypes(string? typesString)
    {
        if (string.IsNullOrWhiteSpace(typesString))
            return Enumerable.Empty<string>();

        return typesString
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct();
    }

    #endregion
}