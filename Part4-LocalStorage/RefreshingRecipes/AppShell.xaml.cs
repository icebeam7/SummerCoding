using RefreshingRecipes.Views;

namespace RefreshingRecipes;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(RecipeDetailView), typeof(RecipeDetailView));
    }
}
