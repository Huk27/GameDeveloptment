using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FarmDashboard.UI;

/// <summary>
/// Basic <see cref="INotifyPropertyChanged"/> helper to reduce boilerplate in view models.
/// </summary>
internal abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
