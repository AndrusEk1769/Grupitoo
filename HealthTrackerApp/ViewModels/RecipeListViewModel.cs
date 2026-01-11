using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HealthTrackerApp.Models;
using HealthTrackerApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;


namespace HealthTrackerApp.ViewModels
{
    public class RecipeListViewModel : BaseViewModel
    {
        private readonly DatabaseService _database;
        private readonly RecipeService _recipeService;

        private string _searchText = string.Empty;

        public ObservableCollection<RecipeSuggestion> Suggestions { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public Command RefreshCommand { get; }
        public Command SearchCommand { get; }
        public Command<RecipeSuggestion> OpenRecipeCommand { get; }
        public Command NewRecipeCommand { get; }
        public Command<RecipeSuggestion> EditRecipeCommand { get; }

        public RecipeListViewModel()
        {
            _database = new DatabaseService();
            _recipeService = new RecipeService(_database);

            RefreshCommand = new Command(async () => await LoadAsync());
            SearchCommand = new Command(async () => await LoadAsync(SearchText));
            OpenRecipeCommand = new Command<RecipeSuggestion>(async (s) => await OpenRecipeAsync(s));
            NewRecipeCommand = new Command(async () => await NewRecipeAsync());
            EditRecipeCommand = new Command<RecipeSuggestion>(async (s) => await EditRecipeAsync(s));

            _ = LoadAsync();
        }

        public async Task LoadAsync(string? search = null)
        {
            try
            {
                var list = await _recipeService.GetSuggestionsAsync(search);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Suggestions.Clear();
                    foreach (var item in list)
                        Suggestions.Add(item);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Retseptide laadimise viga: {ex.Message}");
            }
        }

        private static async Task OpenRecipeAsync(RecipeSuggestion? suggestion)
        {
            if (suggestion == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(HealthTrackerApp.Views.RecipeDetailPage)}?recipeId={suggestion.RecipeId}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud retsepti avada: {ex.Message}", "OK");
            }
        }

        private async Task NewRecipeAsync()
        {
            try
            {
                // Avame uue retsepti redigeerimisvaate (recipeId=0 tähistab uut retsepti)
                await Shell.Current.GoToAsync($"{nameof(HealthTrackerApp.Views.RecipeEditPage)}?recipeId=0");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud avada uue retsepti vormi: {ex.Message}", "OK");
            }
        }

        private async Task EditRecipeAsync(RecipeSuggestion? suggestion)
        {
            if (suggestion == null) return;

            try
            {
                // Navigeeri otse redigeerimisvaatele; vaade laadib retsepti oma viewmodeli kaudu.
                await Shell.Current.GoToAsync($"{nameof(HealthTrackerApp.Views.RecipeEditPage)}?recipeId={suggestion.RecipeId}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud avada redigeerijat: {ex.Message}", "OK");
            }
        }
    }
}
