using SQLite;

namespace HealthTrackerApp.Models
{
    public class Recipe
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nt: "Kiire", "Tervislik", "Magustoit".
        /// </summary>
        public string Category { get; set; } = "Muu";

        /// <summary>
        /// Lühike ja kompaktne juhend. (AI-d ei kasutata – seeditud käsitsi.)
        /// </summary>
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// Näitamiseks retseptide nimekirjas.
        /// </summary>
        public string ShortDescription { get; set; } = string.Empty;
    }
}
