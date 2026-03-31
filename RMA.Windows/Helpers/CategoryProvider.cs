using System.Collections.ObjectModel;
using RMA.Windows.Models;

namespace RMA.Windows.Helpers
{
  public static class CategoryProvider
  {
    public static ObservableCollection<CategoryModel> GetDefaultCategories()
    {
      return new ObservableCollection<CategoryModel>
            {
                new CategoryModel { Name = "Default", Icon = "Star24" },
                new CategoryModel { Name = "Internet", Icon = "Globe24" },
                new CategoryModel { Name = "Confidential", Icon = "LockClosed24" },
                new CategoryModel { Name = "Personal", Icon = "Person24" },
                new CategoryModel { Name = "Work", Icon = "Briefcase24" },
                new CategoryModel { Name = "Finance", Icon = "CreditCard24" },
                new CategoryModel { Name = "Social Media", Icon = "ShareAndroid24" },
                new CategoryModel { Name = "Gaming", Icon = "Games24" },
                new CategoryModel { Name = "Shopping", Icon = "Cart24" },
                new CategoryModel { Name = "Streaming", Icon = "Video24" },
                new CategoryModel { Name = "Forums", Icon = "Chat24" },
                
                // Alphabetical Custom Categories
                new CategoryModel { Name = "Category A", Icon = "AlphaA24" },
                new CategoryModel { Name = "Category B", Icon = "AlphaB24" },
                new CategoryModel { Name = "Category C", Icon = "AlphaC24" }
            };
    }
  }
}