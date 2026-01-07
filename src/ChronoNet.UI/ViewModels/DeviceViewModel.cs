using ChronoNet.Domain;
using ChronoNet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.UI.ViewModels
{
    public class DeviceViewModel : ViewModelBase
    {
        private readonly WindowViewModel _parent;
        private Device? _selectedDevice;
        private bool _isUpdating;

        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice == value) return;

                _selectedDevice = value;
                Raise(nameof(SelectedDevice));
                Raise(nameof(IsDeviceSelected));
                RaiseAllCapabilitiesProperties();
            }
        }

        public bool IsDeviceSelected => _selectedDevice != null;

        public bool GlobalCompute
        {
            get => SelectedDevice != null && SelectedDevice.HasCapability(GlobalCapabilities.Compute);
            set
            {
                if (SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    if (value)
                        SelectedDevice.AddCapability(GlobalCapabilities.Compute);
                    else
                        SelectedDevice.RemoveCapability(GlobalCapabilities.Compute);

                    Raise(nameof(GlobalCompute));

                    _parent.OnDeviceCapabilitiesChanged(SelectedDevice);

                    RaiseAllCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool GlobalStorage
        {
            get => SelectedDevice != null && SelectedDevice.HasCapability(GlobalCapabilities.Storage);
            set
            {
                if (SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    if (value)
                        SelectedDevice.AddCapability(GlobalCapabilities.Storage);
                    else
                        SelectedDevice.RemoveCapability(GlobalCapabilities.Storage);

                    Raise(nameof(GlobalStorage));
                    _parent.OnDeviceCapabilitiesChanged(SelectedDevice);
                    RaiseAllCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool GlobalTransfer
        {
            get => SelectedDevice != null && SelectedDevice.HasCapability(GlobalCapabilities.Transfer);
            set
            {
                if (SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    if (value)
                        SelectedDevice.AddCapability(GlobalCapabilities.Transfer);
                    else
                        SelectedDevice.RemoveCapability(GlobalCapabilities.Transfer);

                    Raise(nameof(GlobalTransfer));
                    _parent.OnDeviceCapabilitiesChanged(SelectedDevice);
                    RaiseAllCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool LocalCompute
        {
            get => CurrentGraph != null && SelectedDevice != null &&
                   (CurrentGraph.GetLocalCapabilities(SelectedDevice.Id) & LocalCapabilities.Compute) == LocalCapabilities.Compute;
            set
            {
                if (CurrentGraph == null || SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    var current = CurrentGraph.GetLocalCapabilities(SelectedDevice.Id);
                    if (value)
                        current |= LocalCapabilities.Compute;
                    else
                        current &= ~LocalCapabilities.Compute;

                    CurrentGraph.SetLocalCapability(SelectedDevice.Id, current);
                    Raise(nameof(LocalCompute));

                    RaiseLocalCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool LocalStorage
        {
            get => CurrentGraph != null && SelectedDevice != null &&
                   (CurrentGraph.GetLocalCapabilities(SelectedDevice.Id) & LocalCapabilities.Storage) == LocalCapabilities.Storage;
            set
            {
                if (CurrentGraph == null || SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    var current = CurrentGraph.GetLocalCapabilities(SelectedDevice.Id);
                    if (value)
                        current |= LocalCapabilities.Storage;
                    else
                        current &= ~LocalCapabilities.Storage;

                    CurrentGraph.SetLocalCapability(SelectedDevice.Id, current);
                    Raise(nameof(LocalStorage));
                    RaiseLocalCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool LocalTransfer
        {
            get => CurrentGraph != null && SelectedDevice != null &&
                   (CurrentGraph.GetLocalCapabilities(SelectedDevice.Id) & LocalCapabilities.Transfer) == LocalCapabilities.Transfer;
            set
            {
                if (CurrentGraph == null || SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    var current = CurrentGraph.GetLocalCapabilities(SelectedDevice.Id);
                    if (value)
                        current |= LocalCapabilities.Transfer;
                    else
                        current &= ~LocalCapabilities.Transfer;

                    CurrentGraph.SetLocalCapability(SelectedDevice.Id, current);
                    Raise(nameof(LocalTransfer));
                    RaiseLocalCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool LocalCanSend
        {
            get => CurrentGraph != null && SelectedDevice != null &&
                   (CurrentGraph.GetLocalCapabilities(SelectedDevice.Id) & LocalCapabilities.CanSend) == LocalCapabilities.CanSend;
            set
            {
                if (CurrentGraph == null || SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    var current = CurrentGraph.GetLocalCapabilities(SelectedDevice.Id);
                    if (value)
                        current |= LocalCapabilities.CanSend;
                    else
                        current &= ~LocalCapabilities.CanSend;

                    CurrentGraph.SetLocalCapability(SelectedDevice.Id, current);
                    Raise(nameof(LocalCanSend));
                    RaiseLocalCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public bool LocalCanReceive
        {
            get => CurrentGraph != null && SelectedDevice != null &&
                   (CurrentGraph.GetLocalCapabilities(SelectedDevice.Id) & LocalCapabilities.CanReceive) == LocalCapabilities.CanReceive;
            set
            {
                if (CurrentGraph == null || SelectedDevice == null || _isUpdating) return;

                _isUpdating = true;
                try
                {
                    var current = CurrentGraph.GetLocalCapabilities(SelectedDevice.Id);
                    if (value)
                        current |= LocalCapabilities.CanReceive;
                    else
                        current &= ~LocalCapabilities.CanReceive;

                    CurrentGraph.SetLocalCapability(SelectedDevice.Id, current);
                    Raise(nameof(LocalCanReceive));
                    RaiseLocalCapabilitiesProperties();
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public TemporalGraph? CurrentGraph => _parent.CurrentGraph;
        public ObservableCollection<Device> AllDevices => _parent.AllDevices;

        public DeviceViewModel(WindowViewModel parent)
        {
            _parent = parent;

            _parent.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WindowViewModel.CurrentGraph))
                {
                    OnCurrentGraphChanged();
                }
                else if (e.PropertyName == nameof(WindowViewModel.AllDevices))
                {
                    Raise(nameof(AllDevices));
                }
            };
        }

        public void OnSelectedDeviceChanged()
        {
            RaiseAllCapabilitiesProperties();
        }

        public void OnCurrentGraphChanged()
        {
            Raise(nameof(CurrentGraph));
            RaiseLocalCapabilitiesProperties();
        }

        private void RaiseAllCapabilitiesProperties()
        {
            // Глобальные возможности
            Raise(nameof(GlobalCompute));
            Raise(nameof(GlobalStorage));
            Raise(nameof(GlobalTransfer));

            // Локальные возможности
            RaiseLocalCapabilitiesProperties();
        }

        private void RaiseLocalCapabilitiesProperties()
        {
            Raise(nameof(LocalCompute));
            Raise(nameof(LocalStorage));
            Raise(nameof(LocalTransfer));
            Raise(nameof(LocalCanSend));
            Raise(nameof(LocalCanReceive));
        }

        public void AllCompute()
        {
            foreach (var device in AllDevices)
            {
                device.AddCapability(GlobalCapabilities.Compute);
            }
            RaiseAllCapabilitiesProperties();
            _parent.OnDeviceCapabilitiesChanged(SelectedDevice);
        }

        public void AllTransfer()
        {
            foreach (var device in AllDevices)
            {
                device.AddCapability(GlobalCapabilities.Transfer);
            }
            RaiseAllCapabilitiesProperties();
            _parent.OnDeviceCapabilitiesChanged(SelectedDevice);
        }

        public void AllStorage()
        {
            foreach (var device in AllDevices)
            {
                device.AddCapability(GlobalCapabilities.Storage);
            }
            RaiseAllCapabilitiesProperties();
            _parent.OnDeviceCapabilitiesChanged(SelectedDevice);
        }
    }
}