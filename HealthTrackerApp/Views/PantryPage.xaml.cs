namespace HealthTrackerApp.Views
{
    public partial class PantryPage : ContentPage
    {
        public PantryPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is HealthTrackerApp.ViewModels.PantryViewModel vm)
            {
                await vm.LoadAsync();
            }
        }

    }
}
