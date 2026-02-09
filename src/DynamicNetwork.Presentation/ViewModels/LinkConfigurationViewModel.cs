using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Presentation.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DynamicNetwork.Presentation.ViewModels;

public class LinkConfigurationViewModel : ViewModelBase
{
    private readonly Action _onChanged;
    private readonly List<string> _originalActiveTransports = new();
    private string _nodeA;
    private string _nodeB;

    public string NodeA
    {
        get => _nodeA;
        set
        {
            if (SetField(ref _nodeA, value))
                _onChanged?.Invoke();
        }
    }

    public string NodeB
    {
        get => _nodeB;
        set
        {
            if (SetField(ref _nodeB, value))
                _onChanged?.Invoke();
        }
    }

    public ObservableCollection<string> EnabledTransports { get; }
        = new ObservableCollection<string>();

    private string? _selectedTransportToAdd;

    public string? SelectedTransportToAdd
    {
        get => _selectedTransportToAdd;
        set => SetField(ref _selectedTransportToAdd, value);
    }

    public LinkConfigurationViewModel(LinkConfiguration linkConfig, Action onChanged)
    {
        _onChanged = onChanged;
        _nodeA = linkConfig.NodeA;
        _nodeB = linkConfig.NodeB;

        foreach (var transport in linkConfig.EnabledTransports)
            EnabledTransports.Add(transport);

        _originalActiveTransports.AddRange(linkConfig.ActiveTransports);

        EnabledTransports.CollectionChanged += (s, e) => _onChanged?.Invoke();
    }

    public LinkConfiguration ToDomainModel()
    {
        var enabledTransports = EnabledTransports.ToList();

        var activeTransports = _originalActiveTransports
            .Where(transport => enabledTransports.Contains(transport))
            .ToList();

        return new LinkConfiguration(
            NodeA,
            NodeB,
            enabledTransports,
            activeTransports
        );
    }

    public void UpdateActiveTransports(IEnumerable<string> activeTransports)
    {
        _originalActiveTransports.Clear();
        _originalActiveTransports.AddRange(activeTransports);
        _onChanged?.Invoke();
    }

    public ICommand AddTransportCommand => new RelayCommand(AddTransport, CanAddTransport);
    public ICommand RemoveTransportCommand => new RelayCommand<string>(RemoveTransport);

    private void AddTransport()
    {
        if (!string.IsNullOrWhiteSpace(SelectedTransportToAdd) &&
            !EnabledTransports.Contains(SelectedTransportToAdd))
        {
            EnabledTransports.Add(SelectedTransportToAdd);
            SelectedTransportToAdd = null;
            _onChanged?.Invoke();
        }
    }

    private bool CanAddTransport() => !string.IsNullOrWhiteSpace(SelectedTransportToAdd);

    private void RemoveTransport(string? transportId)
    {
        if (!string.IsNullOrWhiteSpace(transportId) && EnabledTransports.Contains(transportId))
        {
            EnabledTransports.Remove(transportId);

            if (_originalActiveTransports.Contains(transportId))
            {
                _originalActiveTransports.Remove(transportId);
            }

            _onChanged?.Invoke();
        }
    }
}