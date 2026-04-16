using RMA.Windows.Helpers;
using RMA.Windows.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

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
      if (this.DataContext is LoginViewModel vm)
      {
        vm.PinHasError = false; // Border turns normal immediately
        if (!vm.IdHasError) vm.ErrorMessage = string.Empty;
      }
    }

    private void SetupPinBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is LoginViewModel vm && sender is Wpf.Ui.Controls.PasswordBox pb)
      {
        vm.SetupPin = pb.Password;
      }
    }

    private void ConfirmPinBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is LoginViewModel vm && sender is Wpf.Ui.Controls.PasswordBox pb)
      {
        vm.SetupConfirmPin = pb.Password;
      }
    }



    #region Animations
    public void ExecuteShake()
    {
      // 1. Check if the Border exists yet
      if (MainBorder == null) return;

      // 2. Try to find the resource globally
      var sb = Application.Current.TryFindResource("RmaShakeAnimation") as Storyboard;

      if (sb != null)
      {
        // 3. Start the animation on the border
        sb.Begin(MainBorder);
      }
      else
      {
        // Debugging: If it's still null, the key in Animations.xaml doesn't match
        System.Diagnostics.Debug.WriteLine("Animation 'RmaShakeAnimation' not found!");
      }
    }
    #endregion
  }
}