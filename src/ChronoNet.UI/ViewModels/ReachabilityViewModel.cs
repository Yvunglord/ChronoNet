using ChronoNet.Application.DTO;
using ChronoNet.Application.Services;
using ChronoNet.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoNet.Domain;

namespace ChronoNet.UI.ViewModels
{
    public class ReachabilityViewModel : ViewModelBase
    {
        private readonly WindowViewModel _parent;
        private string _sourceDeviceName = string.Empty;
        private string _targetDeviceNames = string.Empty;
        private bool _considerCapabilities = true;
        private long _start;
        private long _end;
        private long? _dataSize;
        private ReachabilityResult? _result;

        public string SourceDeviceName
        {
            get => _sourceDeviceName;
            set { _sourceDeviceName = value; Raise(nameof(SourceDeviceName)); }
        }

        public string TargetDeviceNames
        {
            get => _targetDeviceNames;
            set { _targetDeviceNames = value; Raise(nameof(TargetDeviceNames)); }
        }

        public bool ConsiderCapabilities
        {
            get => _considerCapabilities;
            set { _considerCapabilities = value; Raise(nameof(ConsiderCapabilities)); }
        }

        public long Start
        {
            get => _start;
            set { _start = value; Raise(nameof(Start)); }
        }

        public long End
        {
            get => _end;
            set { _end = value; Raise(nameof(End)); }
        }

        public long? DataSize
        {
            get => _dataSize;
            set { _dataSize = value; Raise(nameof(DataSize)); }
        }

        public ReachabilityResult? Result
        {
            get => _result;
            set { _result = value; Raise(nameof(Result)); }
        }

        public Dictionary<Guid, string> DeviceMap =>
            _parent.AllDevices.ToDictionary(d => d.Id, d => d.Name);

        public ICommand CalculateReachabilityCommand => new RelayCommand(CalculateReachability);

        public ReachabilityViewModel(WindowViewModel parent)
        {
            _parent = parent;
        }

        private void CalculateReachability()
        {
            if (_parent.CurrentGraph == null || string.IsNullOrEmpty(SourceDeviceName) || string.IsNullOrEmpty(TargetDeviceNames))
            {
                Result = new ReachabilityResult { Message = "Заполните исходное и целевые устройства" };
                return;
            }

            var request = new ReachabilityRequest
            {
                SourceDeviceName = SourceDeviceName,
                TargetDeviceNames = TargetDeviceNames.Split(',').Select(s => s.Trim()).ToList(),
                CustomInterval = new TimeInterval(Start, End),
                ConsiderCapabilities = ConsiderCapabilities,
                DataSize = DataSize
            };

            var deviceMap = _parent.AllDevices
                .GroupBy(d => d.Name)
                .ToDictionary(g => g.Key, g => g.First());
            var graphs = _parent.Graphs.ToList();

            Result = ReachabilityService.CalculateReachability(graphs, request, deviceMap);
        }
    }
}
