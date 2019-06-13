using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace SimpleMVVM.Serilog
{
    public class BindableSink : ILogEventSink, INotifyPropertyChanged, INotifyCollectionChanged, IDisposable
    {
        private readonly IFormatProvider _formatProvider;

        internal readonly ObservableConcurrentCollection<string> Events;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public BindableSink(IFormatProvider formatProvider = null)
        {
            _formatProvider = formatProvider;

            Events = new ObservableConcurrentCollection<string>();
            Events.PropertyChanged += OnPropertyChanged;
            Events.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            Events.AddFromEnumerable(new[] { message });
        }

        private void ReleaseUnmanagedResources()
        {
            PropertyChanged = null;
            CollectionChanged = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~BindableSink()
        {
            ReleaseUnmanagedResources();
        }
    }

    public static class BindableSinkExtensions
    {
        public static LoggerConfiguration BindableSink(this LoggerSinkConfiguration loggerConfiguration, BindableSink sink)
        {
            return loggerConfiguration.Sink(sink);
        }

        public static LoggerConfiguration BindableSink(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new BindableSink(formatProvider));
        }
    }
}
