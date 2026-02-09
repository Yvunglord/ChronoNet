using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.UseCases;
using DynamicNetwork.Application.Interfaces.UseCases.Reachability;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Presentation.Commands;
using System;
using System.Linq;
using System.Windows.Input;

namespace DynamicNetwork.Presentation.ViewModels;

public class TopologyReachabilityViewModel : ViewModelBase
{
    private readonly MainViewModel _parent;
    private readonly ICheckReachabilityUseCase _reachabilityUseCase;

    private string _src = string.Empty;
    private string _targets = string.Empty;
    private long _start;
    private long _end;
    private long? _dataSize;
    private ReachabilityResult? _result;

    public string Src
    {
        get => _src;
        set => SetField(ref _src, value);
    }

    public string Targets
    {
        get => _targets;
        set => SetField(ref _targets, value);
    }

    public long Start
    {
        get => _start;
        set => SetField(ref _start, value);
    }

    public long End
    {
        get => _end;
        set => SetField(ref _end, value);
    }

    public long? DataSize
    {
        get => _dataSize;
        set => SetField(ref _dataSize, value);
    }

    public ReachabilityResult? Result
    {
        get => _result;
        set
        {
            if (SetField(ref _result, value))
            {
                OnPropertyChanged(nameof(AllPaths));
                OnPropertyChanged(nameof(ShortestPathLength));
            }
        }
    }

    public IReadOnlyList<ReachabilityPathDto>? AllPaths => Result?.AllPaths;
    public int? ShortestPathLength => Result?.ShortestPathLength;

    public ICommand CalculateReachabilityCommand => new RelayCommand(CalculateReachability);

    public TopologyReachabilityViewModel(
        MainViewModel parent,
        ICheckReachabilityUseCase reachabilityUseCase)
    {
        _parent = parent;
        _reachabilityUseCase = reachabilityUseCase;
    }

    private void CalculateReachability()
    {
        try
        {
            if (_parent.CurrentGraph == null)
            {
                Result = new ReachabilityResult
                {
                    Message = "Необходимо выбрать временной граф",
                    IsReachable = false
                };
                return;
            }

            if (string.IsNullOrWhiteSpace(Src))
            {
                Result = new ReachabilityResult
                {
                    Message = "Укажите исходный узел",
                    IsReachable = false
                };
                return;
            }

            if (string.IsNullOrWhiteSpace(Targets))
            {
                Result = new ReachabilityResult
                {
                    Message = "Укажите целевые узлы (через запятую)",
                    IsReachable = false
                };
                return;
            }

            var request = new ReachabilityRequest
            {
                SourceNode = Src.Trim(),
                TargetNodes = Targets
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList(),
                CustomInterval = new TimeInterval(Start, End),
                DataSize = DataSize
            };

            var graphs = _parent.TemporalGraphs.ToList();

            if (!graphs.Any())
            {
                Result = new ReachabilityResult
                {
                    Message = "Нет загруженных временных графов",
                    IsReachable = false
                };
                return;
            }

            Result = _reachabilityUseCase.Execute(graphs, request);

            if (!Result.IsReachable)
            {
                _parent.DialogService.ShowWarning("Пути не найдены");
            }
        }
        catch (Exception ex)
        {
            Result = new ReachabilityResult
            {
                Message = $"Ошибка расчёта: {ex.Message}",
                IsReachable = false
            };
            _parent.DialogService.ShowError($"Ошибка расчёта достижимости: {ex.Message}");
        }
    }
}