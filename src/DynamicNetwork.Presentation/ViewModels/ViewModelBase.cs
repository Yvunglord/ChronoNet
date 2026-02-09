using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynamicNetwork.Presentation.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string? prop)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
