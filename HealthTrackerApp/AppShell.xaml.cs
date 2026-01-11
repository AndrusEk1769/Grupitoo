namespace HealthTrackerApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registreeritud marsruudid
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(Views.FoodPage), typeof(Views.FoodPage));
            Routing.RegisterRoute(nameof(Views.PantryPage), typeof(Views.PantryPage));
            Routing.RegisterRoute(nameof(Views.RecipeListPage), typeof(Views.RecipeListPage));
            Routing.RegisterRoute(nameof(Views.RecipeDetailPage), typeof(Views.RecipeDetailPage));
            Routing.RegisterRoute(nameof(HealthTrackerApp.Views.RecipeEditPage), typeof(HealthTrackerApp.Views.RecipeEditPage));
        }
    }
}