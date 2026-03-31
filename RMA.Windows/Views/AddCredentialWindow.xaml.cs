using RMA.Windows.Models;
using RMA.Windows.ViewModels;
using System.Linq;
using System.Windows;
using Wpf.Ui.Controls;

namespace RMA.Windows.Views
{
  public partial class AddCredentialWindow : FluentWindow
  {
    private AddCredentialViewModel ViewModel => (AddCredentialViewModel)DataContext;

    public AddCredentialWindow()
    {
      // Set the DataContext BEFORE the UI is built
      if (DataContext == null)
      {
        DataContext = new AddCredentialViewModel();
      }

      InitializeComponent();

      this.Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      try
      {
        System.Diagnostics.Debug.WriteLine($"Window loaded.");
        System.Diagnostics.Debug.WriteLine($"DataContext is null: {DataContext == null}");
        System.Diagnostics.Debug.WriteLine($"ViewModel is null: {ViewModel == null}");
        System.Diagnostics.Debug.WriteLine($"ViewModel FilteredTemplates is null: {ViewModel?.FilteredTemplates == null}");
        System.Diagnostics.Debug.WriteLine($"ViewModel FilteredTemplates count: {ViewModel?.FilteredTemplates?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"ServiceSearchBox is null: {ServiceSearchBox == null}");
        System.Diagnostics.Debug.WriteLine($"ServiceSearchBox ItemsSource is null: {ServiceSearchBox?.ItemsSource == null}");
      }
      catch (System.Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error in Loaded: {ex.Message}");
      }
    }

    private void ServiceSearchBox_TextChanged(object sender, Wpf.Ui.Controls.AutoSuggestBoxTextChangedEventArgs args)
    {
      // ONLY open if the user is typing
      if (args.Reason == Wpf.Ui.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
      {
        var asb = sender as Wpf.Ui.Controls.AutoSuggestBox;
        if (asb != null)
        {
          // If we have items, show them
          asb.IsSuggestionListOpen = ViewModel.FilteredTemplates.Count > 0;

          // DEBUG: This will tell us if the ASB can see the data
          System.Diagnostics.Debug.WriteLine($"UI Source Count: {asb.ItemsSource?.Cast<object>().Count()}");
        }
      }
    }

    private void ServiceSearchBox_SuggestionChosen(object sender, Wpf.Ui.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
    {
      if (args.SelectedItem is ServiceTemplate selected)
      {
        ServiceSearchBox.Text = selected.Name;
        // The ViewModel binding will handle the rest via OnServiceNameChanged
      }
    }
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}