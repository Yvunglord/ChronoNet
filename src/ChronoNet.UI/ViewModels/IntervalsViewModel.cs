using ChronoNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.UI.ViewModels
{
    public class IntervalsViewModel : ViewModelBase
    {
        public IReadOnlyList<TemporalGraph> Graphs { get; }

        private TemporalGraph? _selected;
        public TemporalGraph? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Raise(nameof(Selected));
                GraphSelected?.Invoke(value!);
            }
        }

        public event Action<TemporalGraph>? GraphSelected;

        public IntervalsViewModel(IReadOnlyList<TemporalGraph> graphs)
        {
            Graphs = graphs;
        }
    }
}
