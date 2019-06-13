using System.Windows;

namespace SimpleMVVM.Framework
{
    public interface IViewModelNavigator
    {
        void NavigateToView(UIElement viewToNavigate);
        void NavigateToView<TUIElement>() where TUIElement : UIElement;
    }
}
