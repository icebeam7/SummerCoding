using System.Net.Http.Json;

using RefreshingRecipes.Models;

namespace RefreshingRecipes.Services
{
    public class RecipeService : IRecipeService
    {
        HttpClient httpClient;

        public RecipeService()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<IEnumerable<Recipe>> GetRecipes()
        {
            var response = await httpClient.GetAsync("https://gist.githubusercontent.com/icebeam7/a6c1c7523e67272e294204aff0b115cc/raw/938694ed82fa34384c9704f6000fa0307ca72c06/recipes.json");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<IEnumerable<Recipe>>();

            return default;
        }
    }
}
