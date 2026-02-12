using System;
using System.Globalization;
using System.Windows.Data;

namespace masodikbeadando.Converters
{
    public class BoolToPauseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool paused = (bool)value;
            return paused ? "Resume" : "Pause";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
