using DynamicNetwork.Presentation.ViewModels;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DynamicNetwork.Presentation.Views
{
    /// <summary>
    /// Логика взаимодействия для VisualGraphView.xaml
    /// </summary>
    public partial class VisualGraphView : UserControl
    {
        private GViewer? _viewer;

        public VisualGraphView()
        {
            InitializeComponent();
            _viewer = new GViewer();
            GraphViewerHost.Child = _viewer;

            this.Loaded += (s, e) =>
            {
                if (DataContext is VisualGraphViewModel vm)
                {
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(VisualGraphViewModel.VisualGraph))
                        {
                            UpdateGraph(vm.VisualGraph);
                        }
                    };

                    if (vm.VisualGraph != null)
                    {
                        UpdateGraph(vm.VisualGraph);
                    }
                }
            };
        }

        private void UpdateGraph(Graph graph)
        {
            if (graph != null && _viewer != null)
            {
                _viewer.Graph = graph;
            }
        }
    }
}
