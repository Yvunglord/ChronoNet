using ChronoNet.Infrastructure.Visualization;
using ChronoNet.UI.ViewModels;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChronoNet.UI.Views
{
    /// <summary>
    /// Логика взаимодействия для GraphView.xaml
    /// </summary>
    public partial class GraphView : System.Windows.Controls.UserControl
    {
        private GViewer? _viewer;

        public GraphView()
        {
            InitializeComponent();
            _viewer = new GViewer();
            GraphViewerHost.Child = _viewer;

            this.Loaded += (s, e) =>
            {
                if (DataContext is GraphViewModel vm)
                {
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(GraphViewModel.VisualGraph))
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