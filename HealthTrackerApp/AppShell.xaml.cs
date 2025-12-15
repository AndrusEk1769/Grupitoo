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
        }
    }
}