using System;
using System.Globalization;
using System.Windows.Data;

namespace MabiMultiClientHelper.ValueConverters
{
    /// <summary>
    /// true <-> false
    /// </summary>
    public class BoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(bool))
            {
                return !(bool)value;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
