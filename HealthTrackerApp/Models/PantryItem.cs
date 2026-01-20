using SQLite;

namespace HealthTrackerApp.Models
{
    public class PantryItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kogus (nt 2 või 250).
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Ühik (nt tk, g, ml).
        /// </summary>
        public string Unit { get; set; } = "tk";

        public string Category { get; set; } = "Muu";

        /// <summary>
        /// Kui true, siis toode on otsas.
        /// </summary>
        public bool IsOutOfStock { get; set; }

        public string QuantityDisplay => $"{Quantity:0.##} {Unit}";
        public string StatusDisplay => IsOutOfStock ? "Otsas" : "Olemas";
    }
}
