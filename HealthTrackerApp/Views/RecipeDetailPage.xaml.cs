using HealthTrackerApp.ViewModels;

namespace HealthTrackerApp.Views
{
    [QueryProperty(nameof(RecipeId), "recipeId")]
    public partial class RecipeDetailPage : ContentPage
    {
        private int _recipeId;

        public string RecipeId
        {
            get => _recipeId.ToString();
            set
            {
                if (int.TryParse(value, out var id))
                    _recipeId = id;

                if (BindingContext is RecipeDetailViewModel vm)
                {
                    vm.RecipeId = _recipeId;
                }
            }
        }

        public RecipeDetailPage()
        {
            InitializeComponent();
        }
    }
}
