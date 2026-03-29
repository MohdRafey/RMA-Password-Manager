using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Wpf.Ui.Controls;
using RMA.Windows.ViewModels;

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
        System.Windows.MessageBox.Show("Access Denied: Incorrect PIN.");
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

      vm.OnSaveSuccess += () => {
        win.Close();
        // Here we would call RefreshData() on the Dashboard
      };

      win.ShowDialog();
    }
    #endregion
  }
}