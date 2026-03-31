using RMA.Windows.Helpers;
using RMA.Windows.Models;
using RMA.Windows.ViewModels;
using System.Diagnostics; // Added for Debug.WriteLine
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace RMA.Windows.Views
{
  public partial class AddCredentialWindow : FluentWindow
  {
    private AddCredentialViewModel ViewModel => (AddCredentialViewModel)DataContext;

    public AddCredentialWindow()
    {
      // Set the DataContext BEFORE the UI is built to ensure bindings are ready
      if (DataContext == null)
      {
        DataContext = new AddCredentialViewModel();
      }
      InitializeComponent();
    }

    private void NotesBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      int length = NotesBox.Text.Length;

      // For Notes: Max 120, Warn at 110
      CounterHelper.UpdateCounter(NotesCounter, length, 120, 110);

      // 2. Trigger the subtle pop animation
      PulseCounter(CounterScale);
    }

    private void ServiceSearchBox_SuggestionChosen(object sender, Wpf.Ui.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
    {
      if (args.SelectedItem is ServiceTemplate selected)
      {
        Debug.WriteLine($"[ASB] Suggestion Chosen: {selected.Name}");

        if (DataContext is AddCredentialViewModel vm)
        {
          // Manually updating VM properties to ensure they sync immediately
          vm.ServiceName = selected.Name;
          vm.ServiceUrl = selected.DefaultUrl ?? "";
          vm.Tag = selected.Category ?? "";

          Debug.WriteLine($"[ASB] VM Updated -> URL: {vm.ServiceUrl}, Tag: {vm.Tag}");
        }
      }
      else
      {
        Debug.WriteLine("[ASB] Suggestion Chosen fired but SelectedItem was null or wrong type.");
      }
    }

    private void ServiceSearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      // We want to handle Enter or Tab
      if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Tab)
      {
        var asb = sender as Wpf.Ui.Controls.AutoSuggestBox;
        if (asb == null) return;

        // Use the internal 'FilteredItems' or look at the bound list
        // Since we are using OriginalItemsSource, the control handles filtering.
        // We can manually grab the first match from the ViewModel's list.
        string query = asb.Text?.ToLower() ?? "";

        if (ViewModel != null && !string.IsNullOrEmpty(query))
        {
          var firstMatch = ViewModel.AllTemplates
              .FirstOrDefault(t => t.Name.ToLower().Contains(query));

          if (firstMatch != null)
          {
            Debug.WriteLine($"[ASB] Auto-selecting first match: {firstMatch.Name}");

            // 1. Update the text box
            asb.Text = firstMatch.Name;

            // 2. Update the VM (This fills URL/Tag via OnServiceNameChanged)
            ViewModel.ServiceName = firstMatch.Name;

            // 3. Close the popup
            asb.IsSuggestionListOpen = false;

            // 4. If it was Tab, we let it move focus. If it was Enter, we stop the "Ding" sound
            if (e.Key == System.Windows.Input.Key.Enter)
            {
              e.Handled = true;
            }
          }
        }
      }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      Debug.WriteLine("AddCredentialWindow: Closing.");
      this.Close();
    }

    #region Animation
    private void PulseCounter(ScaleTransform scale)
    {
      // Define the "Pop" - scaling from 1.0 to 1.2
      DoubleAnimation pulseAnimation = new DoubleAnimation
      {
        From = 1.0,
        To = 1.2,
        Duration = TimeSpan.FromSeconds(0.1), // Very fast jump
        AutoReverse = true, // Automatically returns to 1.0
        EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
      };

      // Apply to both X and Y simultaneously
      scale.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
      scale.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
    }
    #endregion
  }
}