using System;
using System.Windows.Data;

namespace DirectorySync.ViewModels.Converters
{
    /// <summary>
    /// Конвертер для преобразования bool в текст. 
    /// </summary>
    public class BoolToTextConverter : IValueConverter
    {
        /// <summary>
        /// Текст, который будет возвращён конвертером при значении true.
        /// </summary>
        public string True { get; set; }
        /// <summary>
        /// Текст, который будет возвращён конвертером при значении false.
        /// </summary>
        public string False { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value as bool?) == true ? True : False;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}