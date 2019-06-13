using System;
using System.Globalization;
using System.Windows.Data;
using SimpleMVVM.Serilog;

// ReSharper disable once CheckNamespace
namespace SimpleMVVM.Converters
{
    public class BindableSinkBindingConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindableSink sink && sink.Events.Count > 0)
                return string.Join(Environment.NewLine, sink.Events.ToArray());

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is BindableSink sink && sink.Events.Count > 0)
                return string.Join(Environment.NewLine, sink.Events.ToArray());

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}