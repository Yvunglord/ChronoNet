using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Presentation.ViewModels;

public class IntervalsViewModel : ViewModelBase
{
    public IReadOnlyList<TemporalGraph> Graphs { get; }

    private TemporalGraph? _selected;
    public TemporalGraph? Selected
    {
        get => _selected;
        set
        {
            SetField(ref _selected, value);
            GraphSelected?.Invoke(value!);
        }
    }

    public event Action<TemporalGraph>? GraphSelected;

    public IntervalsViewModel(IReadOnlyList<TemporalGraph> graphs)
    {
        Graphs = graphs;
    }
}

