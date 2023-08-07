namespace RefreshingRecipes.Views;

public partial class RecipeListView : ContentPage
{
	public RecipeListView()
	{
		InitializeComponent();

		ShareButton.Clicked += async (s, a) =>
		{
			await Share.Default.RequestAsync(new ShareTextRequest
			{
				Text = "Enjoy a refreshing Raspberry smoothie.",
				Title = "Raspberry smoothie recipe"
			});
		};
	}
}	