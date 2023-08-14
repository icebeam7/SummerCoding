using RefreshingRecipes.ViewModels;

namespace RefreshingRecipes.Views;

public partial class SettingsView : ContentPage
{
	public SettingsView(SettingsViewModel vm)
	{
		InitializeComponent();

        this.BindingContext = vm;
    }
}