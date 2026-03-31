using System.Globalization;
using System.Windows.Data;

namespace RMA.Windows.Converters
{
  public class GreaterThanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int intValue && int.TryParse(parameter?.ToString(), out int threshold))
      {
        return intValue > threshold;
      }
      return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}