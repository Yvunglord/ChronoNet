using ChronoNet.Domain;
using ChronoNet.Infrastructure.Visualization;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.UI.ViewModels
{
    public class GraphViewModel : ViewModelBase
    {
        private readonly MsaglGraphAdapter _adapter;
        private TemporalGraph? _currentGraph;

        public Graph? VisualGraph { get; private set; }

        public GraphViewModel(MsaglGraphAdapter adapter)
        {
            _adapter = adapter;
        }

        public void SetGraph(TemporalGraph graph)
        {
            _currentGraph = graph;
            VisualGraph = _adapter.Build(graph);
            Raise(nameof(VisualGraph));
        }
    }
}
