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

  }
}