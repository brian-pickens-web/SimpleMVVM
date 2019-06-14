using System;
using System.Windows;
using SimpleMVVM.Framework;

namespace SimpleMVVM
{
    // ReSharper disable once IdentifierTypo
    // ReSharper disable once InconsistentNaming
    public static class Program 
    {

        public static void SetView(IViewModelNavigator navigator, Window window)
        {
            navigator.NavigateToView(window);
        }

        public static void Run(IServiceProvider svcProvider, Window window)
        {
            // Set Initial View
            var viewNavigator = svcProvider.GetService(typeof(IViewModelNavigator)) as IViewModelNavigator;
            viewNavigator.NavigateToView(window);

            // Start App
            var app = svcProvider.GetService(typeof(Application)) as Application;
            app.Run(window);
        }

    }
}
