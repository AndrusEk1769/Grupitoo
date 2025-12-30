using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthTrackerApp.Models;

namespace HealthTrackerApp.Services
{
    /// <summary>
    /// Offline retseptide soovitaja: võtab Lao (Pantry) sisu ja arvutab sobivuse.
    /// Ei kasuta veeb API-sid ega AI-d – demo on alati stabiilne.
    /// </summary>
    public class RecipeService
    {
        private readonly DatabaseService _db;

        // Väike sünonüümide kaart, et "tomatid" ja "tomat" jms paremini klapiks.
        private static readonly Dictionary<string, string> Synonyms = new(StringComparer.OrdinalIgnoreCase)
        {
            { "tomatid", "tomat" },
            { "kartulid", "kartul" },
            { "munad", "muna" },
            { "piim", "piim" },
            { "kaer", "kaerahelbed" },
            { "kanafilee", "kana" },
        };

        public RecipeService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<List<RecipeSuggestion>> GetSuggestionsAsync(string? searchText = null)
        {
            var pantry = await _db.GetPantryItemsAsync();
            var pantrySet = BuildPantryNameSet(pantry);

            var recipes = await _db.GetRecipesAsync();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var s = Normalize(searchText);
                recipes = recipes
                    .Where(r => Normalize(r.Name).Contains(s) || Normalize(r.ShortDescription).Contains(s) || Normalize(r.Category).Contains(s))
                    .ToList();
            }

            var suggestions = new List<RecipeSuggestion>();

            foreach (var r in recipes)
            {
                var ingredients = await _db.GetRecipeIngredientsAsync(r.Id);
                var missing = new List<string>();
                int have = 0;

                foreach (var ing in ingredients)
                {
                    var key = Normalize(ing.IngredientName);
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    if (pantrySet.Contains(key))
                        have++;
                    else
                        missing.Add(ing.IngredientName);
                }

                var total = Math.Max(0, ingredients.Count);
                suggestions.Add(new RecipeSuggestion
                {
                    RecipeId = r.Id,
                    Name = r.Name,
                    Category = r.Category,
                    ShortDescription = r.ShortDescription,
                    TotalIngredients = total,
                    HaveIngredients = have,
                    Missing = missing
                });
            }

            // Sorteeri: parim sobivus + vähem puudu.
            return suggestions
                .OrderByDescending(s => s.MatchScore)
                .ThenBy(s => s.MissingIngredients)
                .ThenBy(s => s.Name)
                .ToList();
        }

        private static HashSet<string> BuildPantryNameSet(IEnumerable<PantryItem> pantry)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in pantry)
            {
                if (p == null) continue;
                if (p.IsOutOfStock) continue; // otsas olev ei loe "olemas"-eks
                if (p.Quantity <= 0) continue;

                var name = Normalize(p.Name);
                if (!string.IsNullOrWhiteSpace(name))
                    set.Add(name);
            }
            return set;
        }

        public static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var t = text.Trim().ToLowerInvariant();
            t = t.Replace("ä", "a").Replace("ö", "o").Replace("ü", "u").Replace("õ", "o");

            // Eemalda väga lihtsad lõpud (mitte perfektne, aga demo jaoks piisav)
            if (t.EndsWith("id") && t.Length > 4) t = t[..^2];
            if (t.EndsWith("d") && t.Length > 3) t = t[..^1];

            // Sünonüümid
            if (Synonyms.TryGetValue(t, out var mapped))
                t = mapped.ToLowerInvariant();

            return t;
        }
    }
}
