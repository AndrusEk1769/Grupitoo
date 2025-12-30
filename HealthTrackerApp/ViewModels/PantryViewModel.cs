using System.Collections.ObjectModel;
using HealthTrackerApp.Models;
using HealthTrackerApp.Services;

namespace HealthTrackerApp.ViewModels
{
    public class PantryViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        private string _newName = string.Empty;
        private double _newQuantity = 1;
        private string _newUnit = "tk";
        private string _newCategory = "Muu";

        public ObservableCollection<PantryItem> Items { get; } = new();

        public List<string> UnitOptions { get; } = new() { "tk", "g", "kg", "ml", "l", "pakk" };
        public List<string> CategoryOptions { get; } = new() { "Piimatooted", "Kuivained", "Juurviljad", "Puu- ja köögiviljad", "Liha", "Kala", "Muu" };

        public string NewName
        {
            get => _newName;
            set => SetProperty(ref _newName, value);
        }

        public double NewQuantity
        {
            get => _newQuantity;
            set => SetProperty(ref _newQuantity, value);
        }

        public string NewUnit
        {
            get => _newUnit;
            set => SetProperty(ref _newUnit, value);
        }

        public string NewCategory
        {
            get => _newCategory;
            set => SetProperty(ref _newCategory, value);
        }

        public Command RefreshCommand { get; }
        public Command AddItemCommand { get; }
        public Command<PantryItem> IncreaseCommand { get; }
        public Command<PantryItem> DecreaseCommand { get; }
        public Command<PantryItem> ToggleOutOfStockCommand { get; }
        public Command<PantryItem> DeleteCommand { get; }

        public PantryViewModel()
        {
            _databaseService = new DatabaseService();

            RefreshCommand = new Command(async () => await LoadAsync());
            AddItemCommand = new Command(async () => await AddAsync());
            IncreaseCommand = new Command<PantryItem>(async (item) => await ChangeQuantityAsync(item, +1));
            DecreaseCommand = new Command<PantryItem>(async (item) => await ChangeQuantityAsync(item, -1));
            ToggleOutOfStockCommand = new Command<PantryItem>(async (item) => await ToggleOutOfStockAsync(item));
            DeleteCommand = new Command<PantryItem>(async (item) => await DeleteAsync(item));

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _databaseService.GetPantryItemsAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                    foreach (var it in list)
                        Items.Add(it);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lao andmete laadimise viga: {ex.Message}");
            }
        }

        private async Task AddAsync()
        {
            if (string.IsNullOrWhiteSpace(NewName))
                return;

            var item = new PantryItem
            {
                Name = NewName.Trim(),
                Quantity = NewQuantity <= 0 ? 1 : NewQuantity,
                Unit = string.IsNullOrWhiteSpace(NewUnit) ? "tk" : NewUnit,
                Category = string.IsNullOrWhiteSpace(NewCategory) ? "Muu" : NewCategory,
                IsOutOfStock = false
            };

            await _databaseService.AddPantryItemAsync(item);

            // reset
            NewName = string.Empty;
            NewQuantity = 1;
            NewUnit = "tk";
            NewCategory = "Muu";

            await LoadAsync();
        }

        private async Task ChangeQuantityAsync(PantryItem? item, double delta)
        {
            if (item == null) return;

            var newValue = item.Quantity + delta;
            if (newValue < 0) newValue = 0;

            item.Quantity = newValue;
            if (item.Quantity == 0)
                item.IsOutOfStock = true;

            await _databaseService.UpdatePantryItemAsync(item);
            await LoadAsync();
        }

        private async Task ToggleOutOfStockAsync(PantryItem? item)
        {
            if (item == null) return;
            item.IsOutOfStock = !item.IsOutOfStock;
            await _databaseService.UpdatePantryItemAsync(item);
            await LoadAsync();
        }

        private async Task DeleteAsync(PantryItem? item)
        {
            if (item == null) return;
            await _databaseService.DeletePantryItemAsync(item);
            await LoadAsync();
        }
    }
}
