using System;
using System.Windows;
using System.Windows.Data;

namespace DirectorySync.ViewModels.Converters
{
    /// <summary>
    /// Конвертер для получения стиля по его наименованию.
    /// </summary>
    public class StyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FrameworkElement targetElement = values[0] as FrameworkElement;
            var styleName = values[1] as string;

            if (styleName == null)
                return null;

            return (Style)targetElement.TryFindResource(styleName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}