using RefreshingRecipes.Models;

namespace RefreshingRecipes.Services
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetRecipes();
    }
}
