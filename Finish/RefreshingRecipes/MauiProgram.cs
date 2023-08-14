using Microsoft.Extensions.Logging;
using RefreshingRecipes.Services;
using RefreshingRecipes.ViewModels;
using RefreshingRecipes.Views;

namespace RefreshingRecipes;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
			});

        builder.Services.AddSingleton<IRecipeService, RecipeService>();

		builder.Services.AddSingleton<RecipeCollectionViewModel>();
		builder.Services.AddTransient<RecipeDetailViewModel>();

        builder.Services.AddTransient<RecipeCollectionView>();
        builder.Services.AddTransient<RecipeDetailView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
