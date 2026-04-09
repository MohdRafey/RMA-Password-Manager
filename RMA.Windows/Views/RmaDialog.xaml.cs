using System.Windows;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Controls;
using System.Windows.Media; // For Color and ColorConverters;

namespace RMA.Windows.Views
{
  /// <summary>
  /// Interaction logic for RmaDialog.xaml
  /// </summary>
  public partial class RmaDialog : Window
  {
    public enum DialogType { Info, Warning, Error }

    public RmaDialog()
    {
      InitializeComponent();
    }

    public static void Info(string title, string message)
        => Show(title, message, DialogType.Info, false);

    public static void Error(string title, string message)
        => Show(title, message, DialogType.Error, false);

    public static bool Warn(string title, string message, bool showCancel = true)
        => Show(title, message, DialogType.Warning, showCancel);

    private static bool Show(string title, string message, DialogType type, bool showCancel)
    {
      var dialog = new RmaDialog(); // This is the instance

      dialog.TitleTxt.Text = title.ToUpper();
      dialog.MessageTxt.Text = message;

      switch (type)
      {
        case DialogType.Info:
          // Use 'dialog.' before the control names
          dialog.TypeIcon.Symbol = SymbolRegular.Info24;
          dialog.TypeIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A5A40"));
          break;

        case DialogType.Warning:
          dialog.TypeIcon.Symbol = SymbolRegular.Warning24;
          dialog.TypeIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B5838D"));
          dialog.OkBtn.Appearance = ControlAppearance.Caution;
          break;

        case DialogType.Error:
          dialog.TypeIcon.Symbol = SymbolRegular.ErrorCircle24;
          dialog.TypeIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D00000"));
          dialog.OkBtn.Appearance = ControlAppearance.Danger;
          break;
      }

      if (showCancel) dialog.CancelBtn.Visibility = Visibility.Visible;

      dialog.Owner = Application.Current.MainWindow;
      return dialog.ShowDialog() ?? false;
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e) { this.DialogResult = true; this.Close(); }
    private void CancelBtn_Click(object sender, RoutedEventArgs e) { this.DialogResult = false; this.Close(); }
    private void CloseBtn_Click(object sender, RoutedEventArgs e) { this.Close(); }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
