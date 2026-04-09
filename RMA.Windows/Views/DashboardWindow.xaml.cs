using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Wpf.Ui.Controls;
using RMA.Windows.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace RMA.Windows.Views
{
  public partial class DashboardWindow : FluentWindow
  {
    private readonly DashboardViewModel _viewModel;

    public DashboardWindow()
    {
      _viewModel = new DashboardViewModel();
      DataContext = _viewModel;
      InitializeComponent();

      this.PreviewKeyDown += (s, e) =>
      {
        if (e.Key == Key.Space && LockOverlay.Visibility == Visibility.Collapsed)
        {
          if (!(Keyboard.FocusedElement is System.Windows.Controls.TextBox ||
                Keyboard.FocusedElement is Wpf.Ui.Controls.TextBox))
          {
            LockVault();
            e.Handled = true;
          }
        }
      };
    }

    #region Window Controls
    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    #endregion

    #region Lock Logic
    private void LockVault_Click(object sender, RoutedEventArgs e) => LockVault();

    private void LockVault()
    {
      LockOverlay.Visibility = Visibility.Visible;
      UnlockPasswordBox.Password = string.Empty;
      UnlockPasswordBox.Focus();

      VaultContent.Effect = new BlurEffect { Radius = 60, RenderingBias = RenderingBias.Quality };
    }

    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
      if (_viewModel.AttemptUnlock(UnlockPasswordBox.Password))
      {
        LockOverlay.Visibility = Visibility.Collapsed;
        VaultContent.Effect = null;
        UnlockPasswordBox.Password = string.Empty;
      }
      else
      {
        UnlockPasswordBox.Password = string.Empty;
        RmaDialog.Error("Access Denied", "Incorrect PIN.");
        UnlockPasswordBox.Focus();
      }
    }

    private void UnlockPasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter) Unlock_Click(null, null);
    }

    private void AddNewItem_Click(object sender, RoutedEventArgs e)
    {
      var vm = new AddCredentialViewModel();
      var win = new AddCredentialWindow { DataContext = vm, Owner = this };

      // Setup the close trigger
      vm.OnSaveSuccess += () => win.DialogResult = true;

      if (win.ShowDialog() == true)
      {
        (this.DataContext as DashboardViewModel)?.LoadCredentials();
      }
    }
    #endregion

    private void TestInfo_Click(object sender, RoutedEventArgs e)
    {
      RmaDialog.Info("System Update", "Your encrypted vault has been synchronized with the local database.");
    }

    private void TestWarn_Click(object sender, RoutedEventArgs e)
    {
      // Testing the boolean return logic
      if (RmaDialog.Warn("Security Alert", "You are about to export your passwords to a plain text file. Do you want to proceed?"))
      {
        // This only runs if they click "Okay"
        RmaDialog.Info("Export", "Data exported successfully.");
      }
    }

    private void TestError_Click(object sender, RoutedEventArgs e)
    {
      RmaDialog.Error("Database Error", "Unable to establish a connection to the local vault. Please check your file permissions.");
    }
  }
}