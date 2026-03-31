using System;
using System.Globalization;
using System.Windows.Data;

namespace RMA.Windows.Converters
{
  public class EqualityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length < 2) return false;

      // Compare the two numbers (CurrentCount vs MaxLimit)
      return values[0]?.ToString() == values[1]?.ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}