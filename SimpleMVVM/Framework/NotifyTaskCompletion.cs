using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleMVVM.Framework
{
    public sealed class NotifyTaskCompletion<TParameter, TResult> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyTaskCompletion(Func<TParameter, CancellationToken, Task<TResult>> command, TParameter parameter, CancellationToken token, PropertyChangedEventHandler onPropertyChanged = null)
        {
            if (onPropertyChanged != null)
                PropertyChanged += onPropertyChanged;

            Task = new Task<TResult>(() => command(parameter, token).Result);
            Task.ContinueWith(WatchTaskAsync);
            TaskCompletion = Task;
        }

        private async Task<TResult> WatchTaskAsync(Task<TResult> task)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.Status.ToString()));
                PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.IsCompleted.ToString()));
                PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.IsNotCompleted.ToString()));
                if (task.IsCanceled)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.IsCancelled.ToString()));
                }
                else if (task.IsFaulted)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.IsFaulted.ToString()));
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.Exception.ToString()));
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.InnerException.ToString()));
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.ErrorMessage.ToString()));
                }
                else
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.IsSuccessfullyCompleted.ToString()));
                    PropertyChanged(this, new PropertyChangedEventArgs(NotifyTaskCompletionPropertyType.Result.ToString()));
                }
            }

            PropertyChanged = null;
            return await task;
        }

        public Task<TResult> Task { get; }
        public Task TaskCompletion { get; }

        public TResult Result => 
            Task.Status == TaskStatus.RanToCompletion 
                ? Task.Result
                : default(TResult);

        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException Exception => Task.Exception?.InnerException as AggregateException ?? Task.Exception;
        public Exception InnerException => Exception?.InnerException;
        public string ErrorMessage => InnerException?.Message;
    }
}