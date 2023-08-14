using System.Collections.ObjectModel;

using RefreshingRecipes.Views;
using RefreshingRecipes.Models;
using RefreshingRecipes.Services;
using RefreshingRecipes.Helpers;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RefreshingRecipes.ViewModels
{
    public partial class RecipeCollectionViewModel : BaseViewModel
    {
        public ObservableCollection<Recipe> Recipes { get; } = new();

        IRecipeService recipeService;
        ILocalDbService localDbService;
        bool onlineMode;

        [ObservableProperty]
        Recipe selectedRecipe;

        public RecipeCollectionViewModel(IRecipeService recipeService, ILocalDbService localDbService)
        {
            Title = "Recipe List";
            this.recipeService = recipeService;
            this.localDbService = localDbService;
            onlineMode = Preferences.Get(Constants.OnlineModeKey, true);
        }

        [RelayCommand]
        async Task GetRecipesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var recipes = onlineMode
                    ? (await recipeService.GetRecipes()).ToList()
                    : await localDbService.GetItems<Recipe>();

                //var recipes = (await recipeService.GetRecipes()).ToList();
                //var recipes = await localDbService.GetItems<Recipe>();

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

        [RelayCommand]
        async Task AddLocalRecipesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if (await localDbService.CountItems<Recipe>() == 0)
                {
                    var recipes = (await recipeService.GetRecipes()).ToList();

                    var result = await localDbService.AddItems<Recipe>(recipes);

                    await Shell.Current.DisplayAlert(
                        "Result",
                        result ? "The local database now contains new information!" : "There was an error",
                        "OK");
                }
                else
                    await Shell.Current.DisplayAlert("Error!", "The local database already contains information", "OK");
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
    }
}
