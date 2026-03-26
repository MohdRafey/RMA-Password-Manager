using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace RMA.Windows.Views
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : FluentWindow
  {
        public DashboardWindow()
        {
            InitializeComponent();
        }

    // 1. Minimize
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }

    // 2. Maximize / Restore
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
      if (this.WindowState == WindowState.Maximized)
        this.WindowState = WindowState.Normal;
      else
        this.WindowState = WindowState.Maximized;
    }

    // 3. Close Application
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      Application.Current.Shutdown();
    }

    // 4. Lock (Return to Login)
    private void LockVault_Click(object sender, RoutedEventArgs e)
    {
      // Create a new instance of your Login window (MainWindow)
      var loginWindow = new MainWindow();

      // Set it as the primary window again
      Application.Current.MainWindow = loginWindow;

      loginWindow.Show();

      // Close this dashboard
      this.Close();
    }
  }
}
