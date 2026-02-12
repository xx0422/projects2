using Avalonia.Data.Converters;
using Avalonia.Media;
using Snake.Core.ViewModels;
using System;
using System.Globalization;

namespace SnakeAvalonia.Converters
{
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not CellViewModel cell)
                return Brushes.Transparent;

            if (cell.IsSnake)
                return Brushes.Green;

            if (cell.IsFood)
                return Brushes.Red;

            if (cell.IsObstacle)
                return Brushes.Gray;

            return Brushes.Black; // háttérszín
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
