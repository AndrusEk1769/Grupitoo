using SQLite;

namespace HealthTrackerApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "Kasutaja";
        public int DailyCalorieGoal { get; set; } = 2000;
        public int DailyWaterGoal { get; set; } = 2000;
        public int DailyExerciseGoal { get; set; } = 30;
    }

    public class FoodEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public string Amount { get; set; } = string.Empty;
        public string Category { get; set; } = "other";
        public DateTime Date { get; set; } = DateTime.Now;

        public string DisplayInfo => $"{FoodName} - {Calories} kcal";
        public string DisplayTime => Date.ToString("HH:mm");
    }

    public class RecentActivity
    {
        public string Icon { get; set; } = "🍎";
        public string Description { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}