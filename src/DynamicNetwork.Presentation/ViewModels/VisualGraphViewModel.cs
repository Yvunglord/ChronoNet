using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Infrastructure.Adapters.VisualGraph;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicNetwork.Presentation.ViewModels;

public sealed class VisualGraphViewModel : ViewModelBase
{
    private readonly MsaglGraphAdapter _adapter;
    private TemporalGraph? _currentGraph;

    public Graph? VisualGraph { get; private set; }

    public VisualGraphViewModel(MsaglGraphAdapter adapter)
    {
        _adapter = adapter;
    }

    public void SetGraph(TemporalGraph graph)
    {
        _currentGraph = graph;
        VisualGraph = _adapter.Build(graph);
        OnPropertyChanged(nameof(VisualGraph));
    }
}
