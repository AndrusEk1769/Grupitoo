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
                var name = await Shell.Current.DisplayPromptAsync("Uus retsept", "Retsepti nimi:", "OK", "Cancel", placeholder: "Näide: Kõrvitsapuder");
                if (string.IsNullOrWhiteSpace(name))
                    return;

                var category = await Shell.Current.DisplayPromptAsync("Uus retsept", "Kategooria:", "OK", "Cancel", placeholder: "nt Hommikusöök", initialValue: "Muu");
                if (category == null)
                    category = "Muu";

                var shortDesc = await Shell.Current.DisplayPromptAsync("Uus retsept", "Lühikirjeldus (valikuline):", "OK", "Cancel");
                if (shortDesc == null) shortDesc = string.Empty;

                var instructions = await Shell.Current.DisplayPromptAsync("Uus retsept", "Valmistamisjuhised (valikuline):", "OK", "Cancel");
                if (instructions == null) instructions = string.Empty;

                var r = new Recipe
                {
                    Name = name.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? "Muu" : category.Trim(),
                    ShortDescription = shortDesc.Trim(),
                    Instructions = instructions.Trim()
                };

                // Insert and reload
                await _database.AddRecipeAsync(r);
                await Shell.Current.DisplayAlert("Lisatud", "Uus retsept lisati.", "OK");
                await LoadAsync(SearchText);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud lisada retsepti: {ex.Message}", "OK");
            }
        }

        private async Task EditRecipeAsync(RecipeSuggestion? suggestion)
        {
            if (suggestion == null) return;

            try
            {
                var existing = await _database.GetRecipeByIdAsync(suggestion.RecipeId);
                if (existing == null)
                {
                    await Shell.Current.DisplayAlert("Viga", "Retsepti ei leitud.", "OK");
                    return;
                }

                var name = await Shell.Current.DisplayPromptAsync("Muuda retsepti", "Nimi:", "OK", "Cancel", initialValue: existing.Name);
                if (name == null) return;

                var category = await Shell.Current.DisplayPromptAsync("Muuda retsepti", "Kategooria:", "OK", "Cancel", initialValue: existing.Category ?? "Muu");
                if (category == null) category = existing.Category;

                var shortDesc = await Shell.Current.DisplayPromptAsync("Muuda retsepti", "Lühikirjeldus:", "OK", "Cancel", initialValue: existing.ShortDescription ?? string.Empty);
                if (shortDesc == null) shortDesc = existing.ShortDescription ?? string.Empty;

                var instructions = await Shell.Current.DisplayPromptAsync("Muuda retsepti", "Juhised:", "OK", "Cancel", initialValue: existing.Instructions ?? string.Empty);
                if (instructions == null) instructions = existing.Instructions ?? string.Empty;

                existing.Name = name.Trim();
                existing.Category = string.IsNullOrWhiteSpace(category) ? "Muu" : category.Trim();
                existing.ShortDescription = shortDesc.Trim();
                existing.Instructions = instructions.Trim();

                await _database.UpdateRecipeAsync(existing);
                await Shell.Current.DisplayAlert("Salvestatud", "Retsept salvestatud.", "OK");
                await LoadAsync(SearchText);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud muuta retsepti: {ex.Message}", "OK");
            }
        }
    }
}
