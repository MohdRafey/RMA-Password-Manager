using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using RMA.Windows.Helpers;

namespace RMA.Windows
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        this.DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void LoginPinBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      // Regex checks if the incoming text is NOT a digit
      Regex regex = new Regex("[^0-9]+");
      e.Handled = regex.IsMatch(e.Text);
    }

    private void LoginPinBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
      if (LoginPinBox == null || LoginCounter == null) return;

      int length = LoginPinBox.Password.Length;

      // Update text as "X/6"
      LoginCounter.Text = $"{length}/6";

      // Update color: Max 6, No warning (null)
      CounterHelper.UpdateCounter(LoginCounter, length, 6);
    }

  }
}