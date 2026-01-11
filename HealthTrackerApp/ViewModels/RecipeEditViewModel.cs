using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HealthTrackerApp.Models;
using HealthTrackerApp.Services;
using Microsoft.Maui.Controls;

namespace HealthTrackerApp.ViewModels
{
    public class RecipeEditViewModel : BaseViewModel
    {
        private readonly DatabaseService _database;

        public string PageTitle => RecipeId == 0 ? "Uus retsept" : "Muuda retsepti";

        public int RecipeId { get; set; }

        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        public ObservableCollection<string> Categories { get; } = new();
        private string? _selectedCategory;
        public string? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }

        private string _newCategory = string.Empty;
        public string NewCategory { get => _newCategory; set => SetProperty(ref _newCategory, value); }

        private string _shortDescription = string.Empty;
        public string ShortDescription { get => _shortDescription; set => SetProperty(ref _shortDescription, value); }

        private string _instructions = string.Empty;
        public string Instructions { get => _instructions; set => SetProperty(ref _instructions, value); }

        public ObservableCollection<IngredientEditRow> Ingredients { get; } = new();

        private string _newIngredientName = string.Empty;
        public string NewIngredientName { get => _newIngredientName; set => SetProperty(ref _newIngredientName, value); }

        private string _newIngredientAmount = string.Empty;
        public string NewIngredientAmount { get => _newIngredientAmount; set => SetProperty(ref _newIngredientAmount, value); }

        public Command AddIngredientCommand { get; }
        public Command<IngredientEditRow> RemoveIngredientCommand { get; }
        public Command UseNewCategoryCommand { get; }
        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        public RecipeEditViewModel()
        {
            _database = new DatabaseService();

            AddIngredientCommand = new Command(AddIngredient);
            RemoveIngredientCommand = new Command<IngredientEditRow>(RemoveIngredient);
            UseNewCategoryCommand = new Command(UseNewCategory);
            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await CancelAsync());

            _ = LoadCategoriesAsync();
        }

        private void AddIngredient()
        {
            var name = (NewIngredientName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            Ingredients.Add(new IngredientEditRow { Name = name, AmountText = (NewIngredientAmount ?? string.Empty).Trim() });
            NewIngredientName = string.Empty;
            NewIngredientAmount = string.Empty;
        }

        private void RemoveIngredient(IngredientEditRow? row)
        {
            if (row == null) return;
            Ingredients.Remove(row);
        }

        private void UseNewCategory()
        {
            if (!string.IsNullOrWhiteSpace(NewCategory))
                SelectedCategory = NewCategory.Trim();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var recipes = await _database.GetRecipesAsync();
                var cats = recipes.Select(r => (r.Category ?? "Muu").Trim()).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Categories.Clear();
                    foreach (var c in cats) Categories.Add(c);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Laadi kategooriad viga: {ex.Message}");
            }
        }

        public async Task LoadAsync(int recipeId)
        {
            RecipeId = recipeId;
            await LoadCategoriesAsync();

            if (recipeId == 0)
            {
                Name = string.Empty;
                SelectedCategory = Categories.FirstOrDefault() ?? "Muu";
                ShortDescription = string.Empty;
                Instructions = string.Empty;
                Ingredients.Clear();
                return;
            }

            try
            {
                var r = await _database.GetRecipeByIdAsync(recipeId);
                if (r == null) return;

                var ingr = await _database.GetRecipeIngredientsAsync(recipeId);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = r.Name ?? string.Empty;
                    SelectedCategory = string.IsNullOrWhiteSpace(r.Category) ? "Muu" : r.Category;
                    ShortDescription = r.ShortDescription ?? string.Empty;
                    Instructions = r.Instructions ?? string.Empty;
                    Ingredients.Clear();
                    foreach (var i in ingr)
                        Ingredients.Add(new IngredientEditRow { Name = i.IngredientName, AmountText = i.AmountText });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Laadi retsept viga: {ex.Message}");
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Viga", "Nimi ei tohi olla tühi.", "OK");
                return;
            }

            var category = !string.IsNullOrWhiteSpace(NewCategory) ? NewCategory.Trim() : (SelectedCategory ?? "Muu");

            try
            {
                if (RecipeId == 0)
                {
                    var r = new Recipe
                    {
                        Name = Name.Trim(),
                        Category = string.IsNullOrWhiteSpace(category) ? "Muu" : category,
                        ShortDescription = ShortDescription?.Trim(),
                        Instructions = Instructions?.Trim()
                    };

                    var newId = await _database.AddRecipeAsync(r);

                    // write ingredients
                    foreach (var ing in Ingredients)
                    {
                        if (string.IsNullOrWhiteSpace(ing.Name)) continue;
                        await _database.AddRecipeIngredientAsync(new RecipeIngredient
                        {
                            RecipeId = newId,
                            IngredientName = ing.Name,
                            AmountText = ing.AmountText
                        });
                    }

                    await Shell.Current.DisplayAlert("Salvestatud", "Retsept lisatud.", "OK");
                }
                else
                {
                    var existing = await _database.GetRecipeByIdAsync(RecipeId);
                    if (existing == null)
                    {
                        await Shell.Current.DisplayAlert("Viga", "Retsepti ei leitud.", "OK");
                        return;
                    }

                    existing.Name = Name.Trim();
                    existing.Category = string.IsNullOrWhiteSpace(category) ? "Muu" : category;
                    existing.ShortDescription = ShortDescription?.Trim();
                    existing.Instructions = Instructions?.Trim();

                    await _database.UpdateRecipeAsync(existing);

                    // replace ingredients
                    await _database.DeleteRecipeIngredientsForRecipeAsync(RecipeId);
                    foreach (var ing in Ingredients)
                    {
                        if (string.IsNullOrWhiteSpace(ing.Name)) continue;
                        await _database.AddRecipeIngredientAsync(new RecipeIngredient
                        {
                            RecipeId = RecipeId,
                            IngredientName = ing.Name,
                            AmountText = ing.AmountText
                        });
                    }

                    await Shell.Current.DisplayAlert("Salvestatud", "Retsept salvestatud.", "OK");
                }

                // Go back to list
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud salvestada: {ex.Message}", "OK");
            }
        }

        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public class IngredientEditRow
    {
        public string Name { get; set; } = string.Empty;
        public string AmountText { get; set; } = string.Empty;
        public string Display => string.IsNullOrWhiteSpace(AmountText) ? Name : $"{Name} • {AmountText}";
    }
}