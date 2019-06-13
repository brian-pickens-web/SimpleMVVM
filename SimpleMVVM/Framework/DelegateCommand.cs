using System;
using System.Windows.Input;

namespace SimpleMVVM.Framework
{

    public sealed class DelegateCommand : ICommand
    {
        private readonly Action _command;

        public DelegateCommand(Action command)
        {
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    public sealed class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _command;

        public DelegateCommand(Action<T> command)
        {
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command((T)parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}