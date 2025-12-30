namespace HealthTrackerApp.Views
{
    public partial class RecipeListPage : ContentPage
    {
        public RecipeListPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is HealthTrackerApp.ViewModels.RecipeListViewModel vm)
            {
                await vm.LoadAsync(vm.SearchText);
            }
        }
    }
}
