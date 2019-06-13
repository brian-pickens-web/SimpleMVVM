using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shell;

namespace SimpleMVVM.Converters
{
    public class BoolProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLoading)
                return isLoading ? TaskbarItemProgressState.Indeterminate : TaskbarItemProgressState.None;

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskbarItemProgressState state)
                return state != TaskbarItemProgressState.None;

            return string.Empty;
        }
    }
}