using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RefreshingRecipes.Helpers;

namespace RefreshingRecipes.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool onlineMode;

        public SettingsViewModel()
        {
            Title = "Settings";
            onlineMode = Preferences.Get(Constants.OnlineModeKey, true);
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            Preferences.Set(Constants.OnlineModeKey, OnlineMode);

            await Shell.Current.DisplayAlert("Success!", "Settings saved!", "OK");
        }
    }
}