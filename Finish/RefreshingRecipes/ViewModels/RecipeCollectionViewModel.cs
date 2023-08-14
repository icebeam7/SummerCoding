using System.Collections.ObjectModel;

using RefreshingRecipes.Views;
using RefreshingRecipes.Models;
using RefreshingRecipes.Services;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RefreshingRecipes.ViewModels
{
    public partial class RecipeCollectionViewModel : BaseViewModel
    {
        public ObservableCollection<Recipe> Recipes { get; } = new();

        IRecipeService recipeService;

        [ObservableProperty]
        Recipe selectedRecipe;

        public RecipeCollectionViewModel(IRecipeService recipeService)
        {
            Title = "Recipe List";
            this.recipeService = recipeService;
        }

        [RelayCommand]
        async Task GetRecipesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var recipes = (await recipeService.GetRecipes()).ToList();

                if (Recipes.Count != 0)
                    Recipes.Clear();

                foreach (var recipe in recipes)
                    Recipes.Add(recipe);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task GoToDetails()
        {
            if (SelectedRecipe == null)
                return;

            var data = new Dictionary<string, object>
            {
                {"Recipe", SelectedRecipe }
            };

            await Shell.Current.GoToAsync(nameof(RecipeDetailView), true, data);
        }
    }
}
