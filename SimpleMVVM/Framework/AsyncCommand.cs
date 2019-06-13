using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleMVVM.Framework
{

    public class AsyncCommand<TResult> : AsyncCommand<object, TResult>, IAsyncCommand
    {
        public AsyncCommand(Func<CancellationToken, Task<TResult>> command):
            base((param, token) => command(token))
        {
        }

        public async Task ExecuteAsync()
        {
            await ExecuteAsync(new object());
        }
    }

    public class AsyncCommand<TParameter, TResult> : AbstractCommand, IAsyncCommand<TParameter>, INotifyPropertyChanged
    {
        private readonly Func<TParameter, CancellationToken, Task<TResult>> _command;
        private readonly CancelAsyncCommand _cancelCommand;
        private NotifyTaskCompletion<TParameter, TResult> _execution;

        public AsyncCommand (Func<TParameter, CancellationToken, Task<TResult>> command)
        {
            _command = command;
            _cancelCommand = new CancelAsyncCommand();
        }

        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted;
        }

        public override async void Execute(object parameter)
        {
            await ExecuteAsync((TParameter)parameter);
        }

        public async Task ExecuteAsync(TParameter parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = new NotifyTaskCompletion<TParameter, TResult>(_command, parameter, _cancelCommand.Token, OnPropertyChanged);
            RaiseCanExecuteChanged();
            Execution.Task.Start();
            await Execution.Task;
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        public NotifyTaskCompletion<TParameter, TResult> Execution
        {
            get { return _execution; }
            private set
            {
                _execution = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ExecuteOnPropertyChange(string propertyName, ICommand callback)
        {
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != propertyName) return;
                callback?.Execute(null);
            };
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private sealed class CancelAsyncCommand : ICommand
        {
            private CancellationTokenSource _cts = new CancellationTokenSource();
            private bool _commandExecuting;

            public CancellationToken Token { get { return _cts.Token; } }

            public void NotifyCommandStarting()
            {
                _commandExecuting = true;
                if (!_cts.IsCancellationRequested)
                    return;
                _cts = new CancellationTokenSource();
                RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished()
            {
                _commandExecuting = false;
                RaiseCanExecuteChanged();
            }

            bool ICommand.CanExecute(object parameter)
            {
                return _commandExecuting && !_cts.IsCancellationRequested;
            }

            void ICommand.Execute(object parameter)
            {
                _cts.Cancel();
                RaiseCanExecuteChanged();
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            private void RaiseCanExecuteChanged()
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public static class AsyncCommand
    {
        public static AsyncCommand<object> Create(Func<Task> command)
        {
            return new AsyncCommand<object>(async _ => { await command(); return null; });
        }
        public static AsyncCommand<TParameter, object> Create<TParameter>(Func<TParameter, Task> command)
        {
            return new AsyncCommand<TParameter, object>(async (parameter, token) => { await command(parameter); return null; });
        }

        public static AsyncCommand<TResult> Create<TResult>(Func<Task<TResult>> command)
        {
            return new AsyncCommand<TResult>(_ => command());
        }

        public static AsyncCommand<TParameter, TResult> Create<TParameter, TResult>(Func<TParameter, Task<TResult>> command)
        {
            return new AsyncCommand<TParameter, TResult>((parameter, token) => command(parameter));
        }

        public static AsyncCommand<object> Create(Func<CancellationToken, Task> command)
        {
            return new AsyncCommand<object>(async token => { await command(token); return null; });
        }

        public static AsyncCommand<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> command)
        {
            return new AsyncCommand<TResult>(command);
        }

        public static AsyncCommand<TParameter, TResult> Create<TParameter, TResult>(Func<TParameter, CancellationToken, Task<TResult>> command)
        {
            return new AsyncCommand<TParameter, TResult>(command);
        }
    }
}