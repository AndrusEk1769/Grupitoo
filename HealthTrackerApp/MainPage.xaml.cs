namespace HealthTrackerApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Värskendab andmeid lehe avamisel
            if (BindingContext is ViewModels.MainViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
    }
}