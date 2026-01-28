using ChronoNet.Application.DTO;
using ChronoNet.Application.Services;
using ChronoNet.Domain;
using ChronoNet.Domain.Enums;
using ChronoNet.Domain.Flows;
using ChronoNet.Domain.Processes;
using ChronoNet.Domain.Storage;
using ChronoNet.Domain.Transport;
using ChronoNet.Infrastructure.Json;
using ChronoNet.Infrastructure.Visualization;
using ChronoNet.Infrastructure.Xml;
using ChronoNet.UI.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

namespace ChronoNet.UI.ViewModels
{
    public class WindowViewModel : ViewModelBase
    {
        private TemporalGraph? _currentGraph;
        private Edge? _selectedEdge;
        private ObservableCollection<TemporalGraph> _graphs = new();
        private ObservableCollection<Device> _allDevices = new();
        private Dictionary<Guid, Device> _deviceCache = new();
        private GraphViewModel _graphViewModel;
        private IntervalsViewModel? _intervalsViewModel;
        private DeviceViewModel? _deviceViewModel;
        private GraphAnalysisResult? _analysisResult;
        private ReachabilityViewModel? _reachabilityViewModel;

        public TemporalGraph? CurrentGraph
        {
            get => _currentGraph;
            set
            {
                _currentGraph = value;
                Raise(nameof(CurrentGraph));

                if (value != null)
                {
                    GraphViewModel.SetGraph(value);
                    AnalysisResult = GraphAnalyzer.Analyze(value);

                    if (DeviceViewModel != null)
                        _deviceViewModel.OnCurrentGraphChanged();
                }
            }
        }

        public ObservableCollection<TemporalGraph> Graphs => _graphs;
        public ObservableCollection<Device> AllDevices => _allDevices;
        public GraphViewModel GraphViewModel => _graphViewModel;
        public IntervalsViewModel? IntervalsViewModel
        {
            get => _intervalsViewModel;
            set
            {
                _intervalsViewModel = value;
                Raise(nameof(IntervalsViewModel));
            }
        }

        public DeviceViewModel? DeviceViewModel
        {
            get
            {
                if (_deviceViewModel == null)
                    _deviceViewModel = new DeviceViewModel(this);
                return _deviceViewModel;
            }
        }

        public Edge? SelectedEdge
        {
            get => _selectedEdge;
            set
            {
                _selectedEdge = value;
                Raise(nameof(SelectedEdge));
            }
        }

        public GraphAnalysisResult? AnalysisResult
        {
            get => _analysisResult;
            set
            {
                _analysisResult = value;
                Raise(nameof(AnalysisResult));
            }
        }

        public ReachabilityViewModel ReachabilityViewModel
        {
            get
            {
                if (_reachabilityViewModel == null)
                    _reachabilityViewModel = new ReachabilityViewModel(this);
                return _reachabilityViewModel;
            }
        }

        public ICommand LoadCommand => new RelayCommand(Load);
        public ICommand CycleDirectionCommand => new RelayCommand<Edge>(CycleDirection);
        public ICommand MakeRightDirectionCommand => new RelayCommand(MakeRightDirection);
        public ICommand MakeLeftCommand => new RelayCommand(MakeLeftDirection);
        public ICommand MakeUndirectedCommand => new RelayCommand(MakeUndirected);
        public ICommand ExportXmlCommand => new RelayCommand(ExportXml);

        public WindowViewModel()
        {
            MsaglGraphAdapter adapter = new MsaglGraphAdapter();
            _graphViewModel = new GraphViewModel(adapter);
        }

        private void Load()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Выберите JSON файл с данными графа"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var rawEdges = JsonGraphParser.Parse(json);
                    var devices = DeviceExtractor.ExtractDevices(rawEdges);
                    foreach (var device in devices)
                        AllDevices.Add(device);

                    _deviceCache.Clear();
                    foreach (var device in devices)
                    {
                        _deviceCache[device.Id] = device;
                    }

                    var intervals = IntervalBuilder.Build(rawEdges);
                    var graphs = GraphBuilderService.BuildGraphs(devices, rawEdges, intervals);

                    _graphs.Clear();
                    _graphs = new ObservableCollection<TemporalGraph>(graphs);

                    IntervalsViewModel = new IntervalsViewModel(_graphs);

                    IntervalsViewModel.GraphSelected += (graph) =>
                    {
                        CurrentGraph = graph;
                    };

                    if (_graphs.Count > 0)
                        IntervalsViewModel.Selected = _graphs[0];


                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(ex.Message);
                }
            }
        }

        public void OnDeviceCapabilitiesChanged(Device device)
        {
            var selectedDeviceId = DeviceViewModel?.SelectedDevice?.Id;

            if (_deviceCache.ContainsKey(device.Id))
            {
                _deviceCache[device.Id] = device;
            }

            var tempList = new List<Device>(AllDevices);
            AllDevices.Clear();
            foreach (var dev in tempList)
            {
                AllDevices.Add(dev);
            }

            if (selectedDeviceId != Guid.Empty && DeviceViewModel != null)
            {
                DeviceViewModel.SelectedDevice = AllDevices.FirstOrDefault(d => d.Id == selectedDeviceId);
            }

            DeviceViewModel?.OnSelectedDeviceChanged();
        }

        private void ExportXml()
        {
            try
            {
                var staticModel = BuildStaticModel();
                var simulationContext = BuildSimulationContext(staticModel);
                var simulator = new FlowSimulationService();

                foreach (var graph in _graphs)
                {
                    simulator.Simulate(graph, simulationContext);
                }

                var staticSelectors = BuildStaticSelectorsBlocks();

                var builder = new XmlTaskBuilder();
                var document = builder.Build(
                    _graphs.ToList(),
                    staticModel,
                    staticSelectors);

                SaveXml(document);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private StaticModel BuildStaticModel()
        {
            return new StaticModel
            {
                Flows = new[]
                {
                    new FlowType(1, "RawData"),
                    new FlowType(2, "ProcessedData")
                },

                Processes = new[]
                {
                    new ProcessType(1, 0.2,
                        new Dictionary<int, double>() { { 1, 1 } },
                        new Dictionary<int, double>() { { 2, 1 } }),

                    new ProcessType(2, 0.04,
                        new Dictionary<int, double>() { { 1, 1 } },
                        new Dictionary<int, double>() { { 2, 1 } }),

                    new ProcessType(3, 0.06666666666666667,
                        new Dictionary<int, double>() { { 1, 1 } },
                        new Dictionary<int, double>() { { 2, 1 } }),

                    new ProcessType(4, 0.02,
                        new Dictionary<int, double>() { { 1, 1 } },
                        new Dictionary<int, double>() { { 2, 1 } })
                },

                Transports = new[]
                {
                new TransportType(1, 1,
                    new Dictionary<int, double>() { { 2, 20 } }),

                new TransportType(2, 1,
                    new Dictionary<int, double>() { { 1, 20 } })
                },

                Srorages = new[]
                {
                    new StorageType(1, new[] { 1, 2 })
                }
            };
        }

        private SimulationContext BuildSimulationContext(StaticModel model)
        {
            return new SimulationContext(
                model.Processes.ToDictionary(p => p.Id),
                model.Transports.ToDictionary(t => t.Id));
        }

        private XElement[] BuildStaticSelectorsBlocks()
        {
            return new[]
            {
                XElement.Parse("""
                    <selectors>
                      <selector id="1" sign="0.4">
                        <resultflow flow="2" interval="*" object="*" sign="1.0"/>
                      </selector>
                      <selector id="2" sign="0.35">
                        <resultflow flow="1" interval="*" object="*" sign="1.0"/>
                      </selector>
                      <selector id="3" sign="-0.3">
                        <lost flow="*" interval="*" object="*" sign="1.0"/>
                      </selector>
                    </selectors>
                    """),
                XElement.Parse("""
                    <criterion sign="MAX">
                      <selector id="1"/>
                      <selector id="2"/>
                      <selector id="3"/>
                    </criterion>
                    """),
                XElement.Parse("<constraints/>")
            };
        }

        private void SaveXml(XDocument document)
        {
            var dialog = new SaveFileDialog
            { 
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Сохранить XML файл"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                document.Save(dialog.FileName);
            }
        }

        #region Работа с соединениями

        private void CycleDirection(Edge? edge)
        {
            if (edge == null) return;

            edge.SetDirection(edge.Direction switch
            {
                EdgeDirection.Undirected => EdgeDirection.Right,
                EdgeDirection.Right => EdgeDirection.Left,
                EdgeDirection.Left => EdgeDirection.Undirected,
                _ => EdgeDirection.Undirected
            });

            if (CurrentGraph != null)
            {
                var temp = CurrentGraph;
                CurrentGraph = null;
                CurrentGraph = temp;
            }
        }

        private void MakeRightDirection()
        {
            if (SelectedEdge == null) return;
            SelectedEdge.SetDirection(EdgeDirection.Right);
            RefreshBindings();
        }

        private void MakeUndirected()
        {
            if (SelectedEdge == null) return;
            SelectedEdge.SetDirection(EdgeDirection.Undirected);
            RefreshBindings();
        }

        private void MakeLeftDirection()
        {
            if (SelectedEdge == null) return;
            SelectedEdge.SetDirection(EdgeDirection.Left);
            RefreshBindings();
        }

        private void RefreshBindings()
        {
            if (CurrentGraph != null)
            {
                var temp = CurrentGraph;
                CurrentGraph = null;
                CurrentGraph = temp;
            }

            Raise(nameof(SelectedEdge));
        }

        #endregion
    }
}
