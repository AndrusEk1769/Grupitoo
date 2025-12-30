using System;

namespace HealthTrackerApp.Models
{
    /// <summary>
    /// Vaate (UI) jaoks arvutatud soovitus – ei salvestata andmebaasi.
    /// </summary>
    public class RecipeSuggestion
    {
        public int RecipeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;

        public int TotalIngredients { get; set; }
        public int HaveIngredients { get; set; }
        public int MissingIngredients => Math.Max(0, TotalIngredients - HaveIngredients);

        /// <summary>
        /// 0..1
        /// </summary>
        public double MatchScore => TotalIngredients <= 0 ? 0 : (double)HaveIngredients / TotalIngredients;

        public int MatchPercent => (int)Math.Round(MatchScore * 100);

        public List<string> Missing { get; set; } = new();
    }
}
