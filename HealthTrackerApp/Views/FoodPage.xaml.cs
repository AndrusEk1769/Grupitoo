using HealthTrackerApp.ViewModels;

namespace HealthTrackerApp.Views
{
    public partial class FoodPage : ContentPage
    {
        public FoodPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Värskendab andmeid lehe avamisel
            if (BindingContext is FoodViewModel viewModel)
            {
                // Kutsub load meetodit
                var method = viewModel.GetType().GetMethod("LoadTodayFoodEntries");
                method?.Invoke(viewModel, null);
            }
        }
    }
}