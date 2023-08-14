using SQLite;

namespace RefreshingRecipes.Models
{
    [Table("Recipes")]
    public class Recipe : BasicTable
    {
        public int RecipeId { get; set; }

        [MaxLength(255)]
        public string RecipeName { get; set; }
        public string RecipePhotoUrl { get; set; }
        public string RecipeInstructions { get; set; }
    }
}
