using Microsoft.Maui.Controls;
using System;
using HealthTrackerApp.ViewModels;

namespace HealthTrackerApp.Views
{
    [QueryProperty(nameof(RecipeIdQuery), "recipeId")]
    public partial class RecipeEditPage : ContentPage
    {
        // internal backing to receive route query
        public string RecipeIdQuery
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    // BindingContext is created in XAML; forward load to viewmodel instance
                    if (BindingContext is RecipeEditViewModel vm)
                    {
                        _ = vm.LoadAsync(id);
                    }
                }
            }
        }

        public RecipeEditPage()
        {
            InitializeComponent();
        }
    }
}