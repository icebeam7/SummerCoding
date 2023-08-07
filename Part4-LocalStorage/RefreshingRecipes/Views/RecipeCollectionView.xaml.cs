using RefreshingRecipes.ViewModels;

namespace RefreshingRecipes.Views;

public partial class RecipeCollectionView : ContentPage
{
	public RecipeCollectionView(RecipeCollectionViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}