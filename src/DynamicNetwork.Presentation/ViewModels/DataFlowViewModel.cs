using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Domain.Flows;
using DynamicNetwork.Presentation.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DynamicNetwork.Presentation.ViewModels;

public class DataFlowViewModel : ViewModelBase
{
    private readonly MainViewModel _parent;
    private readonly IDataFlowRepository _dataFlowRepository;

    private string? _dataFlowId;
    private double _dataFlowVolume;
    private string? _dataFlowTransformations;
    private DataFlow? _selectedDataFlow;

    public ObservableCollection<DataFlow> DataFlows { get; }
        = new ObservableCollection<DataFlow>();

    public string? DataFlowId
    {
        get => _dataFlowId;
        set => SetField(ref _dataFlowId, value);
    }

    public double DataFlowVolume
    {
        get => _dataFlowVolume;
        set => SetField(ref _dataFlowVolume, value);
    }

    public string? DataFlowTransformations
    {
        get => _dataFlowTransformations;
        set => SetField(ref _dataFlowTransformations, value);
    }

    public DataFlow? SelectedDataFlow
    {
        get => _selectedDataFlow;
        set
        {
            SetField(ref _selectedDataFlow, value);
            if (value != null)
            {
                DataFlowId = value.Id;
                DataFlowVolume = value.Volume;
                DataFlowTransformations = TransformationsToString(value.Transformations);
            }
        }
    }

    public ICommand AddDataFlowCommand => new RelayCommand(AddDataFlow);
    public ICommand RemoveDataFlowCommand => new RelayCommand(RemoveDataFlow, CanRemoveDataFlow);
    public ICommand UpdateDataFlowCommand => new RelayCommand(UpdateDataFlow, CanUpdateDataFlow);

    public DataFlowViewModel(
        MainViewModel parent,
        IDataFlowRepository dataFlowRepository)
    {
        _parent = parent;
        _dataFlowRepository = dataFlowRepository;
        LoadDataFlowsFromRepository();
    }

    private void LoadDataFlowsFromRepository()
    {
        DataFlows.Clear();
        var dataFlows = _dataFlowRepository.GetAll();
        foreach (var dataFlow in dataFlows)
        {
            DataFlows.Add(dataFlow);
        }
        OnPropertyChanged(nameof(DataFlows));
    }

    private void AddDataFlow()
    {
        if (!IsDataFlowFilled())
            return;

        if (DataFlows.Any(df => df.Id == DataFlowId))
        {
            _parent.DialogService.ShowError($"Поток с ID '{DataFlowId}' уже существует");
            return;
        }

        try
        {
            var transformations = ParseTransformations(DataFlowTransformations);
            var dataFlow = new DataFlow(
                DataFlowId!,
                DataFlowVolume,
                transformations
            );

            // ← Добавление через репозиторий (корректно для сущности)
            if (_dataFlowRepository.Add(dataFlow))
            {
                DataFlows.Add(dataFlow);
                ClearForm();
                _parent.DialogService.ShowInfo($"Поток '{dataFlow.Id}' добавлен");
            }
            else
            {
                _parent.DialogService.ShowError("Не удалось добавить поток");
            }
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка добавления потока: {ex.Message}");
        }
    }

    private void RemoveDataFlow()
    {
        if (_selectedDataFlow == null)
            return;

        try
        {
            _dataFlowRepository.Delete(_selectedDataFlow.Id);
            DataFlows.Remove(_selectedDataFlow);
            SelectedDataFlow = null;
            ClearForm();
            _parent.DialogService.ShowInfo($"Поток '{_selectedDataFlow.Id}' удалён");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка удаления потока: {ex.Message}");
        }
    }

    private void UpdateDataFlow()
    {
        if (_selectedDataFlow == null || !IsDataFlowFilled())
            return;

        try
        {
            var transformations = ParseTransformations(DataFlowTransformations);
            var updatedDataFlow = new DataFlow(
                DataFlowId!,
                DataFlowVolume,
                transformations
            );

            _dataFlowRepository.Update(updatedDataFlow);

            var index = DataFlows.IndexOf(_selectedDataFlow);
            if (index != -1)
            {
                DataFlows[index] = updatedDataFlow;
                SelectedDataFlow = updatedDataFlow;
            }

            _parent.DialogService.ShowInfo($"Поток '{updatedDataFlow.Id}' обновлён");
        }
        catch (Exception ex)
        {
            _parent.DialogService.ShowError($"Ошибка обновления потока: {ex.Message}");
        }
    }

    private bool CanRemoveDataFlow() => _selectedDataFlow != null;
    private bool CanUpdateDataFlow() => _selectedDataFlow != null && IsDataFlowFilled();

    private bool IsDataFlowFilled()
    {
        return !string.IsNullOrWhiteSpace(_dataFlowId) &&
               _dataFlowVolume > 0;
    }

    private void ClearForm()
    {
        DataFlowId = null;
        DataFlowVolume = 0;
        DataFlowTransformations = null;
    }

    private string TransformationsToString(IReadOnlyList<FlowTransformation> transformations)
    {
        return string.Join("; ", transformations.Select(t => $"{t.InputType}->{t.OutputType}"));
    }

    private IEnumerable<FlowTransformation> ParseTransformations(string? transformationsString)
    {
        if (string.IsNullOrWhiteSpace(transformationsString))
            return Enumerable.Empty<FlowTransformation>();

        var result = new List<FlowTransformation>();
        var transformationStrings = transformationsString.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var transformationString in transformationStrings)
        {
            var parts = transformationString.Split("->", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                result.Add(new FlowTransformation(parts[0].Trim(), parts[1].Trim()));
            }
        }

        return result;
    }
}