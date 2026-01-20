using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HealthTrackerApp.Models;
using HealthTrackerApp.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace HealthTrackerApp.ViewModels
{
    public class RecipeDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _database;
        private readonly RecipeService _recipeService;

        private Recipe? _recipe;

        public Recipe? Recipe
        {
            get => _recipe;
            set => SetProperty(ref _recipe, value);
        }

        public ObservableCollection<IngredientRow> Ingredients { get; } = new();

        public Command AddMissingToPantryCommand { get; }
        public Command OpenWebSearchCommand { get; }

        private int _recipeId;
        public int RecipeId
        {
            get => _recipeId;
            set
            {
                if (SetProperty(ref _recipeId, value))
                    _ = LoadAsync(value);
            }
        }

        public RecipeDetailViewModel()
        {
            _database = new DatabaseService();
            _recipeService = new RecipeService(_database);

            AddMissingToPantryCommand = new Command(async () => await AddMissingAsync());
            OpenWebSearchCommand = new Command(async () => await OpenWebSearchAsync());
        }

        public async Task LoadAsync(int recipeId)
        {
            try
            {
                var r = await _database.GetRecipeByIdAsync(recipeId);
                if (r == null) return;

                var pantry = await _database.GetPantryItemsAsync();
                var pantrySet = new HashSet<string>(pantry
                    .Where(p => !p.IsOutOfStock && p.Quantity > 0)
                    .Select(p => RecipeService.Normalize(p.Name)),
                    StringComparer.OrdinalIgnoreCase);

                var ing = await _database.GetRecipeIngredientsAsync(recipeId);
                var rows = ing.Select(i => new IngredientRow
                {
                    Name = i.IngredientName,
                    AmountText = i.AmountText,
                    IsMissing = !pantrySet.Contains(RecipeService.Normalize(i.IngredientName))
                }).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Recipe = r;
                    Ingredients.Clear();
                    foreach (var row in rows)
                        Ingredients.Add(row);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Retsepti laadimise viga: {ex.Message}");
            }
        }

        private async Task AddMissingAsync()
        {
            try
            {
                var missing = Ingredients.Where(i => i.IsMissing).Select(i => i.Name).ToList();
                if (missing.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Info", "Sul on kõik koostisosad juba olemas!", "OK");
                    return;
                }

                await _database.AddMissingIngredientsToPantryAsync(missing);
                await Shell.Current.DisplayAlert("Lisatud", "Puuduvad koostisosad lisati lattu (otsas olekuga).", "OK");

                // Lae uuesti, et "puudu" olek oleks värske.
                if (Recipe != null)
                    await LoadAsync(Recipe.Id);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud lisada: {ex.Message}", "OK");
            }
        }

        private async Task OpenWebSearchAsync()
        {
            try
            {
                var name = Recipe?.Name ?? "retsept";
                var q = Uri.EscapeDataString($"{name} retsept");
                var url = new Uri($"https://www.google.com/search?q={q}");
                await Launcher.Default.OpenAsync(url);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud brauserit avada: {ex.Message}", "OK");
            }
        }
    }

    public class IngredientRow
    {
        public string Name { get; set; } = string.Empty;
        public string AmountText { get; set; } = string.Empty;
        public bool IsMissing { get; set; }

        public string Display => string.IsNullOrWhiteSpace(AmountText) ? Name : $"{Name} • {AmountText}";
        public string StatusText => IsMissing ? "Puudu" : "Olemas";
    }
}
