using SQLite;
using HealthTrackerApp.Models;

namespace HealthTrackerApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            InitializeDatabaseAsync();
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                if (_database == null)
                {
                    string dbPath = Path.Combine(FileSystem.AppDataDirectory, "health.db3");
                    _database = new SQLiteAsyncConnection(dbPath);

                    await _database.CreateTableAsync<User>();
                    await _database.CreateTableAsync<FoodEntry>();

                    // Kontrolli kas on kasutaja
                    var user = await _database.Table<User>().FirstOrDefaultAsync();
                    if (user == null)
                    {
                        user = new User { Name = "Kasutaja" };
                        await _database.InsertAsync(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Andmebaasi viga: {ex.Message}");
            }
        }

        // TOIDU OPERATSIOONID
        public async Task<List<FoodEntry>> GetTodayFoodEntriesAsync()
        {
            await InitializeDatabaseAsync();
            var today = DateTime.Today;
            return await _database.Table<FoodEntry>()
                .Where(f => f.Date.Date == today)
                .OrderByDescending(f => f.Date)
                .ToListAsync();
        }

        public async Task<int> AddFoodEntryAsync(FoodEntry entry)
        {
            await InitializeDatabaseAsync();
            return await _database.InsertAsync(entry);
        }

        public async Task<int> DeleteFoodEntryAsync(FoodEntry entry)
        {
            await InitializeDatabaseAsync();
            return await _database.DeleteAsync(entry);
        }

        public async Task<int> GetTodayCaloriesAsync()
        {
            var entries = await GetTodayFoodEntriesAsync();
            return entries.Sum(e => e.Calories);
        }

        // KASUTAJA OPERATSIOONID
        public async Task<User> GetUserAsync()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<User>().FirstOrDefaultAsync() ?? new User();
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            await InitializeDatabaseAsync();
            return await _database.UpdateAsync(user);
        }
    }
}