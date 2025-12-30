using System.Collections.ObjectModel;
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
        public Command GoToWaterCommand { get; }
        public Command GoToExerciseCommand { get; }
        public Command GoToStatsCommand { get; }
        public Command GoToPantryCommand { get; }

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            CurrentUser = new User();
            RecentActivities = new ObservableCollection<RecentActivity>();

            GoToFoodCommand = new Command(async () => await GoToFoodPage());
            GoToWaterCommand = new Command(async () => await GoToWaterPage());
            GoToExerciseCommand = new Command(async () => await GoToExercisePage());
            GoToStatsCommand = new Command(async () => await GoToStatsPage());
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

                LoadRecentActivities();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Andmete laadimise viga: {ex.Message}");
            }
        }

        private void LoadRecentActivities()
        {
            RecentActivities.Clear();

            // Testandmed - hiljem asendadad päris andmetega
            RecentActivities.Add(new RecentActivity
            {
                Icon = "🍎",
                Description = "Õun - 95 kcal",
                Time = "10:30"
            });
            RecentActivities.Add(new RecentActivity
            {
                Icon = "💧",
                Description = "Vesi - 500 ml",
                Time = "11:15"
            });
            RecentActivities.Add(new RecentActivity
            {
                Icon = "🏃",
                Description = "Jooksmine - 30 min",
                Time = "08:00"
            });
            RecentActivities.Add(new RecentActivity
            {
                Icon = "🥪",
                Description = "Võileib - 350 kcal",
                Time = "12:45"
            });
        }

        private async Task GoToFoodPage()
        {
            await Shell.Current.GoToAsync("FoodPage");
        }

        private async Task GoToPantryPage()
        {
            await Shell.Current.GoToAsync(nameof(Views.PantryPage));
        }

        private async Task GoToWaterPage()
        {
            await Shell.Current.DisplayAlert("Info", "Vee leht tuleb hiljem!", "OK");
        }

        private async Task GoToExercisePage()
        {
            await Shell.Current.DisplayAlert("Info", "Treeningute leht tuleb hiljem!", "OK");
        }

        private async Task GoToStatsPage()
        {
            await Shell.Current.DisplayAlert("Info", "Statistika leht tuleb hiljem!", "OK");
        }

        private async Task GoToPantryPages()
        {
            await Shell.Current.GoToAsync(nameof(Views.PantryPage));
        }

        public async Task RefreshData()
        {
            TodayCalories = await _databaseService.GetTodayCaloriesAsync();
            LoadRecentActivities();
        }
    }
}