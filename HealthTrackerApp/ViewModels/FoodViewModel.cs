using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using HealthTrackerApp.Models;
using HealthTrackerApp.Services;

namespace HealthTrackerApp.ViewModels
{
    public class FoodViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private string _foodName;
        private string _amount;
        private int _calories;
        private string _selectedCategory = "other";

        public string FoodName
        {
            get => _foodName;
            set => SetProperty(ref _foodName, value);
        }

        public string Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public int Calories
        {
            get => _calories;
            set => SetProperty(ref _calories, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<FoodEntry> TodayFoodEntries { get; set; }

        public Command AddFoodCommand { get; }
        public Command DeleteFoodCommand { get; }
        public Command QuickAddCommand { get; }
        public Command OpenPantryCommand { get; }

        public FoodViewModel()
        {
            _databaseService = new DatabaseService();

            Categories = new ObservableCollection<string>
            {
                "hommikusöök",
                "lõunasöök",
                "õhtusöök",
                "suupisted",
                "joogid",
                "muu"
            };

            TodayFoodEntries = new ObservableCollection<FoodEntry>();

            AddFoodCommand = new Command(async () => await AddFoodAsync());
            DeleteFoodCommand = new Command<FoodEntry>(async (entry) => await DeleteFoodAsync(entry));
            QuickAddCommand = new Command<string>(async (foodType) => await QuickAddFoodAsync(foodType));
            OpenPantryCommand = new Command(async () => await OpenPantryAsync());

            LoadTodayFoodEntries();
        }

        private async void LoadTodayFoodEntries()
        {
            try
            {
                var entries = await _databaseService.GetTodayFoodEntriesAsync();
                TodayFoodEntries.Clear();
                foreach (var entry in entries)
                {
                    TodayFoodEntries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Toidu sissekannete laadimise viga: {ex.Message}");
            }
        }

        private async Task AddFoodAsync()
        {
            if (string.IsNullOrWhiteSpace(FoodName))
            {
                await Shell.Current.DisplayAlert("Viga", "Palun sisesta toidu nimi!", "OK");
                return;
            }

            try
            {
                // Lao kategooria (ära kasuta "other", vaid suuna see "muu" alla)
                var catForPantry = (string.IsNullOrWhiteSpace(SelectedCategory) || SelectedCategory == "other")
                    ? "muu"
                    : SelectedCategory;

                // 1) Kui kaloreid pole (0 või vähem), siis lisa ainult lattu (ei lähe päevikusse)
                if (Calories <= 0)
                {
                    await _databaseService.UpsertPantryItemByNameAsync(FoodName, Amount, catForPantry);

                    FoodName = string.Empty;
                    Amount = string.Empty;
                    Calories = 0;

                    await Shell.Current.DisplayAlert("Edukas", "Lisatud lattu (Lao vaatesse)!", "OK");
                    return;
                }

                // 2) Muidu lisa päevikusse + lisa ka lattu
                var foodEntry = new FoodEntry
                {
                    FoodName = FoodName,
                    Calories = Calories,
                    Amount = string.IsNullOrWhiteSpace(Amount) ? "1 portsjon" : Amount,
                    Category = SelectedCategory,
                    Date = DateTime.Now
                };

                await _databaseService.AddFoodEntryAsync(foodEntry);
                TodayFoodEntries.Insert(0, foodEntry);

                // Lisa sama toode ka lattu ja salvesta DB-sse
                await _databaseService.UpsertPantryItemByNameAsync(FoodName, Amount, catForPantry);

                // Tühjendab väljad
                FoodName = string.Empty;
                Amount = string.Empty;
                Calories = 0;

                await Shell.Current.DisplayAlert("Edukas", "Toidu sissekanne lisatud!", "OK");

                // Uuendab main page'i (kui sul see loogika olemas on)
                var mainPage = Shell.Current?.CurrentPage?.BindingContext as MainViewModel;
                mainPage?.RefreshData();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud toitu lisada: {ex.Message}", "OK");
            }
        }

        private async Task OpenPantryAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(HealthTrackerApp.Views.PantryPage));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Viga", $"Ei saanud avada Lao vaadet: {ex.Message}", "OK");
            }
        }

        private async Task DeleteFoodAsync(FoodEntry entry)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Kustuta",
                $"Kas oled kindel, et soovid kustutada '{entry.FoodName}'?",
                "Jah",
                "Ei"
            );

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteFoodEntryAsync(entry);
                    TodayFoodEntries.Remove(entry);

                    await Shell.Current.DisplayAlert("Edukas", "Toidu sissekanne kustutatud!", "OK");

                    // Uuendab main page'i (kui sul see loogika olemas on)
                    var mainPage = Shell.Current?.CurrentPage?.BindingContext as MainViewModel;
                    mainPage?.RefreshData();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Viga", $"Ei saanud toitu kustutada: {ex.Message}", "OK");
                }
            }
        }

        private async Task QuickAddFoodAsync(string foodType)
        {
            switch (foodType)
            {
                case "apple":
                    FoodName = "Õun";
                    Calories = 95;
                    Amount = "1 tk (keskmine)";
                    SelectedCategory = "suupisted";
                    break;

                case "banana":
                    FoodName = "Banaan";
                    Calories = 105;
                    Amount = "1 tk (keskmine)";
                    SelectedCategory = "suupisted";
                    break;

                case "sandwich":
                    FoodName = "Võileib juustuga";
                    Calories = 350;
                    Amount = "1 tk";
                    SelectedCategory = "lõunasöök";
                    break;
            }

            await AddFoodAsync();
        }
    }
}
