using System;
using System.Windows;
using System.Windows.Input;

namespace SimpleMVVM.Framework
{
    public sealed class GlobalViewModel : ViewModel, IViewModelNavigator, ICurrentView
    {
        private IServiceProvider _container;

        public GlobalViewModel(IServiceProvider container)
        {
            _container = container;
        }

        public void NavigateToView(UIElement viewToNavigate)
        {
            this["CurrentView"] = viewToNavigate;
        }

        public void NavigateToView<TUIElement>() where TUIElement : UIElement
        {
            NavigateToView((UIElement)_container.GetService(typeof(TUIElement)));
        }

        public UIElement CurrentView => this["CurrentView"];

        public void ExecuteOnViewChange(ICommand command)
        {
            ExecuteOnPropertyChange("CurrentView", command);
        }
    }
}
