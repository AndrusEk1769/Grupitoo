using SQLite;

namespace HealthTrackerApp.Models
{
    public class RecipeIngredient
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int RecipeId { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        /// <summary>
        /// Nt: "2 tk", "200 g" – ainult kuvamiseks.
        /// </summary>
        public string AmountText { get; set; } = string.Empty;
    }
}
