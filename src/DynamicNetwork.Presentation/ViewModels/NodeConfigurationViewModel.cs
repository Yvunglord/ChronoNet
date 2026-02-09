using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Presentation.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DynamicNetwork.Presentation.ViewModels;

public class NodeConfigurationViewModel : ViewModelBase
{
    private readonly Action _onChanged;
    private readonly List<string> _originalActiveProcesses = new();
    private string _nodeId;

    public string NodeId
    {
        get => _nodeId;
        set
        {
            if (SetField(ref _nodeId, value))
                _onChanged?.Invoke();
        }
    }

    public ObservableCollection<string> EnabledProcesses { get; }
        = new ObservableCollection<string>();

    public ObservableCollection<string> Inputs { get; }
        = new ObservableCollection<string>();

    public ObservableCollection<string> Outputs { get; }
        = new ObservableCollection<string>();

    public ObservableCollection<StorageCapacityViewModel> StorageCapacities { get; }
        = new ObservableCollection<StorageCapacityViewModel>();

    private string? _selectedProcessToAdd;
    private string? _newInputText;
    private string? _newOutputText;
    private string? _selectedStorageToAdd;

    public string? SelectedProcessToAdd
    {
        get => _selectedProcessToAdd;
        set => SetField(ref _selectedProcessToAdd, value);
    }

    public string? NewInputText
    {
        get => _newInputText;
        set => SetField(ref _newInputText, value);
    }

    public string? NewOutputText
    {
        get => _newOutputText;
        set => SetField(ref _newOutputText, value);
    }

    public string? SelectedStorageToAdd
    {
        get => _selectedStorageToAdd;
        set => SetField(ref _selectedStorageToAdd, value);
    }

    public NodeConfigurationViewModel(NodeConfiguration nodeConfig, Action onChanged)
    {
        _onChanged = onChanged;
        _nodeId = nodeConfig.NodeId;

        foreach (var process in nodeConfig.EnabledProcesses)
            EnabledProcesses.Add(process);

        _originalActiveProcesses.AddRange(nodeConfig.ActiveProcesses);

        foreach (var input in nodeConfig.Inputs)
            Inputs.Add(input);

        foreach (var output in nodeConfig.Outputs)
            Outputs.Add(output);

        foreach (var storage in nodeConfig.StorageCapacities)
            StorageCapacities.Add(new StorageCapacityViewModel(storage.Key, storage.Value, onChanged));

        EnabledProcesses.CollectionChanged += (s, e) => _onChanged?.Invoke();
        Inputs.CollectionChanged += (s, e) => _onChanged?.Invoke();
        Outputs.CollectionChanged += (s, e) => _onChanged?.Invoke();
        StorageCapacities.CollectionChanged += (s, e) => _onChanged?.Invoke();
    }

    public NodeConfiguration ToDomainModel()
    {
        var processes = EnabledProcesses.ToList();
        var inputs = Inputs.ToList();
        var outputs = Outputs.ToList();
        var storageDict = StorageCapacities
            .ToDictionary(s => s.StorageType, s => s.Capacity);

        var activeProcesses = _originalActiveProcesses
            .Where(process => processes.Contains(process))
            .ToList();

        return new NodeConfiguration(
            NodeId,
            processes,
            inputs,
            outputs,
            storageDict,
            activeProcesses
        );
    }

    public void UpdateActiveProcesses(IEnumerable<string> activeProcesses)
    {
        _originalActiveProcesses.Clear();
        _originalActiveProcesses.AddRange(activeProcesses);
        _onChanged?.Invoke();
    }

    public ICommand AddProcessCommand => new RelayCommand(AddProcess, CanAddProcess);
    public ICommand RemoveProcessCommand => new RelayCommand<string>(RemoveProcess);

    public ICommand AddInputCommand => new RelayCommand(AddInput, CanAddInput);
    public ICommand RemoveInputCommand => new RelayCommand<string>(RemoveInput);

    public ICommand AddOutputCommand => new RelayCommand(AddOutput, CanAddOutput);
    public ICommand RemoveOutputCommand => new RelayCommand<string>(RemoveOutput);

    public ICommand AddStorageCommand => new RelayCommand(AddStorage, CanAddStorage);
    public ICommand RemoveStorageCommand => new RelayCommand<StorageCapacityViewModel>(RemoveStorage);

    private void AddProcess()
    {
        if (!string.IsNullOrWhiteSpace(SelectedProcessToAdd) &&
            !EnabledProcesses.Contains(SelectedProcessToAdd))
        {
            EnabledProcesses.Add(SelectedProcessToAdd);
            SelectedProcessToAdd = null;
            _onChanged?.Invoke();
        }
    }

    private bool CanAddProcess() => !string.IsNullOrWhiteSpace(SelectedProcessToAdd);

    private void RemoveProcess(string? processId)
    {
        if (!string.IsNullOrWhiteSpace(processId) && EnabledProcesses.Contains(processId))
        {
            EnabledProcesses.Remove(processId);
            _onChanged?.Invoke();
        }
    }

    private void AddInput()
    {
        if (!string.IsNullOrWhiteSpace(NewInputText) && !Inputs.Contains(NewInputText))
        {
            Inputs.Add(NewInputText);
            NewInputText = null;
            _onChanged?.Invoke();
        }
    }

    private bool CanAddInput() => !string.IsNullOrWhiteSpace(NewInputText);

    private void RemoveInput(string? input)
    {
        if (!string.IsNullOrWhiteSpace(input) && Inputs.Contains(input))
        {
            Inputs.Remove(input);
            _onChanged?.Invoke();
        }
    }

    private void AddOutput()
    {
        if (!string.IsNullOrWhiteSpace(NewOutputText) && !Outputs.Contains(NewOutputText))
        {
            Outputs.Add(NewOutputText);
            NewOutputText = null;
            _onChanged?.Invoke();
        }
    }

    private bool CanAddOutput() => !string.IsNullOrWhiteSpace(NewOutputText);

    private void RemoveOutput(string? output)
    {
        if (!string.IsNullOrWhiteSpace(output) && Outputs.Contains(output))
        {
            Outputs.Remove(output);
            _onChanged?.Invoke();
        }
    }

    private void AddStorage()
    {
        if (!string.IsNullOrWhiteSpace(SelectedStorageToAdd) &&
            !StorageCapacities.Any(s => s.StorageType == SelectedStorageToAdd))
        {
            StorageCapacities.Add(new StorageCapacityViewModel(SelectedStorageToAdd, 0, _onChanged));
            SelectedStorageToAdd = null;
            _onChanged?.Invoke();
        }
    }

    private bool CanAddStorage() => !string.IsNullOrWhiteSpace(SelectedStorageToAdd);

    private void RemoveStorage(StorageCapacityViewModel? storage)
    {
        if (storage != null && StorageCapacities.Contains(storage))
        {
            StorageCapacities.Remove(storage);
            _onChanged?.Invoke();
        }
    }
}