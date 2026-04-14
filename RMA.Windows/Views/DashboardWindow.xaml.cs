using RMA.Windows.Services;
using RMA.Windows.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace RMA.Windows.Views
{
  public partial class DashboardWindow : FluentWindow
  {
    private readonly DashboardViewModel _viewModel;
    private readonly ISnackbarService _snackbarService;

    public DashboardWindow()
    {
      InitializeComponent();

      // 1. Initialize the VM and set the DataContext
      _viewModel = new DashboardViewModel();
      DataContext = _viewModel;

      // 2. Setup the UI Snackbar Presenter
      _snackbarService = new SnackbarService();
      _snackbarService.SetSnackbarPresenter(DashboardSnackbar);

      // 3. Create the Notification Service using our EXISTING _viewModel
      // No need to cast (DashboardViewModel)this.DataContext anymore
      var notificationService = new NotificationService(_snackbarService, _viewModel);

      // 4. Inject it
      _viewModel.SetNotificationService(notificationService);

      // 5. Space-to-Lock (Your important logic)
      this.PreviewKeyDown += (s, e) =>
      {
        if (e.Key == Key.Space && LockOverlay.Visibility == Visibility.Collapsed)
        {
          var focused = Keyboard.FocusedElement;
          if (!(focused is System.Windows.Controls.TextBox || focused is Wpf.Ui.Controls.TextBox))
          {
            LockVault();
            e.Handled = true;
          }
        }
      };

      notificationService.Notify("Vault Online", "RMA is ready and encrypted.", NotificationType.Success);
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

    private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      // 1. Handle Double-Click to Maximize/Restore
      if (e.ClickCount == 2)
      {
        MaximizeButton_Click(sender, e);
        return;
      }

      // 2. Handle Dragging from Fullscreen/Maximized
      if (e.ChangedButton == MouseButton.Left)
      {
        if (this.WindowState == WindowState.Maximized)
        {
          // 1. Get the absolute mouse position on the screen
          Point screenMousePos = PointToScreen(e.GetPosition(this));

          // 2. Calculate the horizontal percentage (where on the bar are we?)
          double resumeWidth = this.RestoreBounds.Width;
          double xRatio = e.GetPosition(this).X / this.ActualWidth;

          // 3. Restore window state
          this.WindowState = WindowState.Normal;

          // 4. Calculate new Left/Top so the mouse stays in the same relative spot
          this.Left = screenMousePos.X - (resumeWidth * xRatio);
          this.Top = screenMousePos.Y - e.GetPosition(this).Y;
        }

        // 5. Drag the window
        this.DragMove();
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