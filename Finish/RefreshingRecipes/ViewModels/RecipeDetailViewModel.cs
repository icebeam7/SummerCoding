using CommunityToolkit.Mvvm.ComponentModel;
using RefreshingRecipes.Models;

namespace RefreshingRecipes.ViewModels
{
    [QueryProperty(nameof(Recipe), "Recipe")]
    public partial class RecipeDetailViewModel : BaseViewModel
    {
        public RecipeDetailViewModel()
        {

        }

        [ObservableProperty]
        Recipe recipe;
    }
}
