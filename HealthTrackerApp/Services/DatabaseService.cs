using SQLite;
using HealthTrackerApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

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
                    await _database.CreateTableAsync<PantryItem>();
                    await _database.CreateTableAsync<Recipe>();
                    await _database.CreateTableAsync<RecipeIngredient>();

                    // Kontrolli kas on kasutaja
                    var user = await _database.Table<User>().FirstOrDefaultAsync();
                    if (user == null)
                    {
                        user = new User { Name = "Kasutaja" };
                        await _database.InsertAsync(user);
                    }

                    // Lao (koduste toiduainete) baasandmed
                    var pantryCount = await _database.Table<PantryItem>().CountAsync();
                    if (pantryCount == 0)
                    {
                        var seedItems = new List<PantryItem>
                        {
                            new PantryItem { Name = "Piim", Quantity = 1, Unit = "l", Category = "Piimatooted" },
                            new PantryItem { Name = "Muna", Quantity = 10, Unit = "tk", Category = "Piimatooted" },
                            new PantryItem { Name = "Pasta", Quantity = 500, Unit = "g", Category = "Kuivained" },
                        };

                        await _database.InsertAllAsync(seedItems);
                    }

                    // Retseptide baasandmed (offline demo jaoks)
                    var recipeCount = await _database.Table<Recipe>().CountAsync();
                    if (recipeCount == 0)
                    {
                        await SeedRecipesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Andmebaasi viga: {ex.Message}");
            }
        }

        private async Task SeedRecipesAsync()
        {
            // Väike, kuid piisav komplekt demo jaoks. (Ei kasuta veeb API-t.)
            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Name = "Pasta tomatikastmega",
                    Category = "Kiire",
                    ShortDescription = "Klassikaline, lihtne ja kiire pasta.",
                    Instructions = "1) Keeda pasta soolaga vees.\n2) Prae sibul/küüslauk, lisa tomatikaste.\n3) Sega pasta kastmega, lisa juust ja serveeri."
                },
                new Recipe
                {
                    Name = "Omlett",
                    Category = "Hommikusöök",
                    ShortDescription = "Munast kiire soe söök.",
                    Instructions = "1) Klopi munad, lisa sool-pipar.\n2) Kuumuta pann, lisa veidi rasva.\n3) Küpseta, lisa soovi korral juust/ köögiviljad."
                },
                new Recipe
                {
                    Name = "Kaerapuder",
                    Category = "Hommikusöök",
                    ShortDescription = "Tervislik puder piima või veega.",
                    Instructions = "1) Kuumuta piim või vesi.\n2) Lisa kaerahelbed ja näpuots soola.\n3) Keeda 3–5 min, lisa marjad/mesi."
                },
                new Recipe
                {
                    Name = "Kana-riisikauss",
                    Category = "Tervislik",
                    ShortDescription = "Lihtne kauss riisi, kana ja köögiviljaga.",
                    Instructions = "1) Küpseta kana tükid.\n2) Keeda riis.\n3) Sega kaussi, lisa köögiviljad ja kaste."
                },
                new Recipe
                {
                    Name = "Kartuli-juustusahv",
                    Category = "Ahjus",
                    ShortDescription = "Ahjus küpsetatud kartul ja juust.",
                    Instructions = "1) Tükelda kartulid, maitsesta.\n2) Küpseta 200°C ~30–35 min.\n3) Lisa juust ja küpseta veel 5 min."
                },
            };

            await _database.InsertAllAsync(recipes);

            // Lisa koostisosad
            var list = new List<RecipeIngredient>();
            int idPasta = recipes[0].Id;
            int idOmlett = recipes[1].Id;
            int idPuder = recipes[2].Id;
            int idKana = recipes[3].Id;
            int idKartul = recipes[4].Id;

            // NB: InsertAllAsync täidab Id-d objektides, sest sqlite-net-pcl uuendab neid peale inserti.
            list.AddRange(new[]
            {
                new RecipeIngredient { RecipeId = idPasta, IngredientName = "Pasta", AmountText = "200 g" },
                new RecipeIngredient { RecipeId = idPasta, IngredientName = "Tomatikaste", AmountText = "200 ml" },
                new RecipeIngredient { RecipeId = idPasta, IngredientName = "Juust", AmountText = "50 g" },
                new RecipeIngredient { RecipeId = idPasta, IngredientName = "Sibul", AmountText = "1 tk" },
                new RecipeIngredient { RecipeId = idPasta, IngredientName = "Küüslauk", AmountText = "1–2 küünt" },

                new RecipeIngredient { RecipeId = idOmlett, IngredientName = "Muna", AmountText = "2–3 tk" },
                new RecipeIngredient { RecipeId = idOmlett, IngredientName = "Piim", AmountText = "50 ml" },
                new RecipeIngredient { RecipeId = idOmlett, IngredientName = "Juust", AmountText = "30 g" },

                new RecipeIngredient { RecipeId = idPuder, IngredientName = "Kaerahelbed", AmountText = "60 g" },
                new RecipeIngredient { RecipeId = idPuder, IngredientName = "Piim", AmountText = "250 ml" },
                new RecipeIngredient { RecipeId = idPuder, IngredientName = "Mesi", AmountText = "1 tl" },

                new RecipeIngredient { RecipeId = idKana, IngredientName = "Kana", AmountText = "200 g" },
                new RecipeIngredient { RecipeId = idKana, IngredientName = "Riis", AmountText = "100 g" },
                new RecipeIngredient { RecipeId = idKana, IngredientName = "Paprika", AmountText = "1 tk" },
                new RecipeIngredient { RecipeId = idKana, IngredientName = "Sibul", AmountText = "1 tk" },

                new RecipeIngredient { RecipeId = idKartul, IngredientName = "Kartul", AmountText = "500 g" },
                new RecipeIngredient { RecipeId = idKartul, IngredientName = "Juust", AmountText = "80 g" },
                new RecipeIngredient { RecipeId = idKartul, IngredientName = "Õli", AmountText = "1 spl" },
            });

            await _database.InsertAllAsync(list);
        }

        // RETSEPTID
        public async Task<List<Recipe>> GetRecipesAsync()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<Recipe>()
                .OrderBy(r => r.Category)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int recipeId)
        {
            await InitializeDatabaseAsync();
            return await _database.Table<Recipe>().Where(r => r.Id == recipeId).FirstOrDefaultAsync();
        }

        public async Task<List<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
        {
            await InitializeDatabaseAsync();
            return await _database.Table<RecipeIngredient>()
                .Where(i => i.RecipeId == recipeId)
                .OrderBy(i => i.IngredientName)
                .ToListAsync();
        }

        /// <summary>
        /// Lisab puuduvad koostisosad Lao vaatesse "otsas" olekus.
        /// (See toimib nagu ostunimekiri ilma eraldi tabelita.)
        /// </summary>
        public async Task AddMissingIngredientsToPantryAsync(IEnumerable<string> ingredientNames)
        {
            await InitializeDatabaseAsync();

            foreach (var name in ingredientNames)
            {
                var clean = (name ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(clean))
                    continue;

                var existing = await _database.Table<PantryItem>()
                    .Where(p => p.Name.ToLower() == clean.ToLower())
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    await _database.InsertAsync(new PantryItem
                    {
                        Name = clean,
                        Quantity = 0,
                        Unit = "tk",
                        Category = "Muu",
                        IsOutOfStock = true
                    });
                }
                else
                {
                    if (existing.Quantity <= 0)
                        existing.Quantity = 0;
                    existing.IsOutOfStock = true;
                    await _database.UpdateAsync(existing);
                }
            }
        }

        // LAO (PANTRY) OPERATSIOONID
        public async Task<List<PantryItem>> GetPantryItemsAsync()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<PantryItem>()
                .OrderBy(p => p.IsOutOfStock)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<int> AddPantryItemAsync(PantryItem item)
        {
            await InitializeDatabaseAsync();
            return await _database.InsertAsync(item);
        }

        public async Task<int> UpdatePantryItemAsync(PantryItem item)
        {
            await InitializeDatabaseAsync();
            return await _database.UpdateAsync(item);
        }

        public async Task<int> DeletePantryItemAsync(PantryItem item)
        {
            await InitializeDatabaseAsync();
            return await _database.DeleteAsync(item);
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

        // ---- LISA: Lisa-toidust -> Lao vaatesse (upsert nime järgi) ----
        public async Task UpsertPantryItemByNameAsync(string name, string? amountText = null, string? category = null)
        {
            await InitializeDatabaseAsync();

            var cleanName = (name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanName)) return;

            var (qty, unit) = TryParseAmount(amountText);
            var cleanCategory = string.IsNullOrWhiteSpace(category) ? "Muu" : category.Trim();

            // Leia olemasolev toode (case-insensitive)
            var existing = await _database.Table<PantryItem>()
                .Where(p => p.Name.ToLower() == cleanName.ToLower())
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                var item = new PantryItem
                {
                    Name = cleanName,
                    Quantity = qty,
                    Unit = unit,
                    Category = cleanCategory,
                    IsOutOfStock = false
                };

                await _database.InsertAsync(item);
                return;
            }

            // Kui ühik sobib, liida kogus; muidu liida lihtsalt 1 tk (lihtne ja turvaline)
            if (!string.IsNullOrWhiteSpace(unit) && string.Equals(existing.Unit, unit, StringComparison.OrdinalIgnoreCase))
                existing.Quantity += qty;
            else
                existing.Quantity += 1;

            if (existing.Quantity > 0) existing.IsOutOfStock = false;

            // Kui kategooria polnud enne täidetud, võta uus
            if (string.IsNullOrWhiteSpace(existing.Category) || existing.Category == "Muu")
                existing.Category = cleanCategory;

            await _database.UpdateAsync(existing);
        }

        // New: add recipe
        public async Task<int> AddRecipeAsync(Recipe recipe)
        {
            await InitializeDatabaseAsync();
            await _database.InsertAsync(recipe);
            return recipe.Id;
        }

        // New: update recipe
        public async Task<int> UpdateRecipeAsync(Recipe recipe)
        {
            await InitializeDatabaseAsync();
            return await _database.UpdateAsync(recipe);
        }

        private static (double qty, string unit) TryParseAmount(string? amountText)
        {
            // Ootame formaati "500 ml", "1 tk", "0.5 l" jne. Kui ei saa aru, tagasta 1 tk.
            if (string.IsNullOrWhiteSpace(amountText))
                return (1, "tk");

            var txt = amountText.Trim().ToLowerInvariant();
            txt = txt.Replace(",", ".");

            // Leia esimene number
            var m = Regex.Match(txt, @"(?<n>\d+(?:\.\d+)?)\s*(?<u>[a-zA-Zõäöü]+)?");
            if (!m.Success)
                return (1, "tk");

            if (!double.TryParse(m.Groups["n"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var qty))
                qty = 1;

            var unit = m.Groups["u"].Success ? m.Groups["u"].Value : "tk";

            // Normaliseeri ühikud laos kasutatavateks
            var allowed = new HashSet<string> { "tk", "g", "kg", "ml", "l", "pakk" };
            if (!allowed.Contains(unit))
                unit = "tk";

            if (qty <= 0) qty = 1;
            return (qty, unit);
        }
    }
}