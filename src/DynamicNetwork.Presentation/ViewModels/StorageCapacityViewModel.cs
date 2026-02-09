namespace DynamicNetwork.Presentation.ViewModels;

public class StorageCapacityViewModel : ViewModelBase
{
    private readonly Action _onChanged;
    private string _storageType;
    private double _capacity;

    public string StorageType
    {
        get => _storageType;
        set
        {
            if (SetField(ref _storageType, value))
                _onChanged?.Invoke();
        }
    }

    public double Capacity
    {
        get => _capacity;
        set
        {
            if (SetField(ref _capacity, value))
                _onChanged?.Invoke();
        }
    }

    public StorageCapacityViewModel(string storageType, double capacity, Action onChanged)
    {
        _storageType = storageType;
        _capacity = capacity;
        _onChanged = onChanged;
    }
}