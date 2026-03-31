using System.Windows.Controls;
using System.Windows.Media;

namespace RMA.Windows.Helpers
{
  public static class CounterHelper
  {
    // Define your colors once for consistency
    private static readonly SolidColorBrush DefaultGreen = new SolidColorBrush(Color.FromRgb(136, 150, 116));
    private static readonly SolidColorBrush WarningYellow = new SolidColorBrush(Color.FromRgb(233, 196, 106));
    private static readonly SolidColorBrush ErrorRed = new SolidColorBrush(Color.FromRgb(231, 111, 81));

    /// <summary>
    /// Updates the character counter text and applies conditional formatting based on limits.
    /// </summary>
    /// <param name="target">The TextBlock control displaying the count.</param>
    /// <param name="current">The current number of characters entered.</param>
    /// <param name="max">The maximum allowed characters (turns Red at this limit).</param>
    /// <param name="warnAt">Optional: The threshold to trigger a Warning (Yellow) color. If null, warning is skipped.</param>
    public static void UpdateCounter(TextBlock target, int current, int max, int? warnAt = null)
    {
      // Update the text first
      target.Text = current.ToString();

      // Logic for Color
      if (current >= max)
      {
        target.Foreground = ErrorRed;
      }
      else if (warnAt.HasValue && current >= warnAt.Value)
      {
        target.Foreground = WarningYellow;
      }
      else
      {
        target.Foreground = DefaultGreen;
      }
    }
  }
}