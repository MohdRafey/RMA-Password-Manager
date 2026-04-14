using RMA.Windows.ViewModels;
using System;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace RMA.Windows.Services
{
  public enum NotificationType { Success, Info, Warning, Error }

  public interface INotificationService
  {
    void Notify(string title, string message, NotificationType type = NotificationType.Info);
  }

  public class NotificationService : INotificationService
  {
    private readonly ISnackbarService _snackbarService;
    private readonly DashboardViewModel _dashboardVm;

    public NotificationService(ISnackbarService snackbarService, DashboardViewModel dashboardVm)
    {
      _snackbarService = snackbarService;
      _dashboardVm = dashboardVm;
    }

    public void Notify(string title, string message, NotificationType type)
    {
      // 1. Map NotificationType to WPF UI Appearance
      var appearance = type switch
      {
        NotificationType.Success => ControlAppearance.Success,
        NotificationType.Error => ControlAppearance.Danger,
        NotificationType.Warning => ControlAppearance.Caution,
        _ => ControlAppearance.Info
      };

      // 2. Show the Floating Snackbar
      _snackbarService.Show(
          title,
          message,
          appearance,
          new SymbolIcon(GetIcon(type)),
          TimeSpan.FromSeconds(3)
      );

      // 3. Update the Dashboard Status Bar (Persistent log)
      _dashboardVm.StatusText = $"[{DateTime.Now:HH:mm}] {title}: {message}";
    }

    private SymbolRegular GetIcon(NotificationType type) => type switch
    {
      NotificationType.Success => SymbolRegular.CheckmarkCircle24,
      NotificationType.Error => SymbolRegular.ErrorCircle24,
      NotificationType.Warning => SymbolRegular.Warning24,
      _ => SymbolRegular.Info24
    };
  }
}