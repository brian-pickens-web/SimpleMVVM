using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;

namespace SimpleMVVM.Framework
{
    public abstract class ViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableConcurrentDictionary<string, dynamic> PropertyBag = new ObservableConcurrentDictionary<string, dynamic>();

        public dynamic this[string key]
        {
            get
            {
                PropertyBag.TryGetValue(key, out var value);
                return value;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PropertyBag[key] = value;
                    OnPropertyChanged(key);
                    OnPropertyChanged("Item[]");
                });
            }
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged(string propertyName = null)
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

        public void ExecuteOnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public void Dispose()
        {
            ClearProperties();
            PropertyChanged = null;
        }

        protected void ClearProperties()
        {
            foreach (var propertyBagKey in PropertyBag.Keys)
            {
                PropertyBag.Remove(propertyBagKey);
            }
        }
    }
}
