using System.Collections.ObjectModel;
using System.Linq;
using HealthTrackerApp.Models;
using HealthTrackerApp.Services;

namespace HealthTrackerApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private User _currentUser;
        private int _todayCalories;
        private int _todayWater;
        private int _todayExercise;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public int TodayCalories
        {
            get => _todayCalories;
            set => SetProperty(ref _todayCalories, value);
        }

        public int TodayWater
        {
            get => _todayWater;
            set => SetProperty(ref _todayWater, value);
        }

        public int TodayExercise
        {
            get => _todayExercise;
            set => SetProperty(ref _todayExercise, value);
        }

        public double CaloriesProgress => CurrentUser?.DailyCalorieGoal > 0
            ? Math.Min((double)TodayCalories / CurrentUser.DailyCalorieGoal, 1.0)
            : 0;

        public double WaterProgress => CurrentUser?.DailyWaterGoal > 0
            ? Math.Min((double)TodayWater / CurrentUser.DailyWaterGoal, 1.0)
            : 0;

        public double ExerciseProgress => CurrentUser?.DailyExerciseGoal > 0
            ? Math.Min((double)TodayExercise / CurrentUser.DailyExerciseGoal, 1.0)
            : 0;

        public ObservableCollection<RecentActivity> RecentActivities { get; set; }

        public Command GoToFoodCommand { get; }
        public Command GoToRecipeCommand { get; }
        public Command GoToPantryCommand { get; }

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            CurrentUser = new User();
            RecentActivities = new ObservableCollection<RecentActivity>();

            GoToFoodCommand = new Command(async () => await GoToFoodPage());
            GoToRecipeCommand = new Command(async () => await GoToRecipePage());
            GoToPantryCommand = new Command(async () => await GoToPantryPage());

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                CurrentUser = await _databaseService.GetUserAsync();
                TodayCalories = await _databaseService.GetTodayCaloriesAsync();

                // Testandmed
                TodayWater = 1500; // ml
                TodayExercise = 45; // minutit

                await LoadRecentActivitiesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Andmete laadimise viga: {ex.Message}");
            }
        }

        private async Task LoadRecentActivitiesAsync()
        {
            RecentActivities.Clear();

            try
            {
                // Load actual food entries from database
                var foodEntries = await _databaseService.GetTodayFoodEntriesAsync();
                
                // Convert food entries to recent activities
                foreach (var entry in foodEntries.Take(10)) // Show last 10 entries
                {
                    RecentActivities.Add(new RecentActivity
                    {
                        Icon = "🍎",
                        Description = $"{entry.FoodName} - {entry.Calories} kcal",
                        Time = entry.DisplayTime
                    });
                }

                // If no entries, show a message
                if (RecentActivities.Count == 0)
                {
                    RecentActivities.Add(new RecentActivity
                    {
                        Icon = "ℹ️",
                        Description = "Pole veel sissekandeid",
                        Time = ""
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Viimaste tegevuste laadimise viga: {ex.Message}");
            }
        }

        private async Task GoToFoodPage()
        {
            // Navigate to the existing FoodPage tab
            await Shell.Current.GoToAsync("//FoodPage");
        }

        private async Task GoToPantryPage()
        {
            await Shell.Current.GoToAsync(nameof(Views.PantryPage));
        }

        private async Task GoToRecipePage()
        {
            await Shell.Current.GoToAsync("//RecipeListPage");
        }

        public async Task RefreshData()
        {
            TodayCalories = await _databaseService.GetTodayCaloriesAsync();
            await LoadRecentActivitiesAsync();
        }
    }
}