using System;
using System.Windows.Data;
using System.Windows.Media;

namespace DirectorySync.ViewModels.Converters
{
    /// <summary>
    /// Конвертер для преобразования типа сообщения в цвет кисти.
    /// </summary>
    public class MessageTypeToColorConverter : IValueConverter
    {
        /// <summary>
        /// Цвет кисти, для типа сообщения Default.
        /// </summary>
        public SolidColorBrush DefaultColor { get; set; }
        /// <summary>
        /// Цвет кисти, для типа сообщения Warning.
        /// </summary>
        public SolidColorBrush WarningColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MessageTypeEnum messageType;
            if (Enum.TryParse(value.ToString(), out messageType))
            if (messageType == MessageTypeEnum.Warning)
                return WarningColor;

            return DefaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}