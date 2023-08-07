using RefreshingRecipes.ViewModels;

namespace RefreshingRecipes.Views;

public partial class RecipeDetailView : ContentPage
{
	public RecipeDetailView(RecipeDetailViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;

        ShareButton.Clicked += async (s, a) =>
		{
			await Share.Default.RequestAsync(new ShareTextRequest
			{
				Text = vm.Recipe.RecipeInstructions,
				Title = vm.Recipe.RecipeName
			});
		};
	}
}	