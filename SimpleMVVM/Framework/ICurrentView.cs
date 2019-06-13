using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SimpleMVVM.Framework
{
    public interface ICurrentView : INotifyPropertyChanged
    {
        UIElement CurrentView { get; }
        void ExecuteOnViewChange(ICommand command);
    }
}
