# Part 4 - Local Data Persistence
For the final part, offline mode will be implemented, meaning that the user can obtain the recipe list from either Internet or a local database.

### Add SQLite (and related) NuGet packages 
The SQLite database engine allows .NET MAUI apps to load and save data objects in shared code. You can integrate SQLite.NET into .NET MAUI apps by adding a few NuGet packages:

1. Right-click on your project and choose **Manage NuGet packages**:

![Add new Nuget packages](/Art/46-ManageNuGetPackage.png)

2. The main NuGet package is `SQLite.NET`. Search for `sqlite-net-pcl` and make sure to select and install the one authored by **SQLite-net**:

![Add SQLite Net NuGet package](/Art/47-AddSQLiteNetPCLNuGetPackage.png)

3. In addition to sqlite-net-pcl, you temporarily need to install the underlying dependency that exposes SQLite on each platform. Look for `SQLitePCLRaw.bundle_green` and install it:

![Add SQLite PCL raw NuGet package](/Art/48-AddSQLitePCLRawBundleGreenNuGetPackage.png)

4. You can proceed with the next part. If you get an error when testing your app on Android, Windows, or iOS, come back here and install the following dependencies (add one package and test the app, if your app now works, that's it; otherwise, install the next package from the list and test again). 

* SQLitePCLRaw.provider.dynamic_cdecl
* SQLitePCLRaw.provider.sqlite3
* SQLitePCLRaw.core

![Add SQLite PCL Raw Bundle Provider Cdecl NuGetPackage](/Art/49-AddSQLitePCLRawBundleProviderCdeclNuGetPackage.png)

### Configure app constants
Configuration data, such as database filename and path, can be stored as constants in your app.

1. In the `Helpers` folder, add a new class, `Constants.cs`:

![Add Constants class in Helpers](/Art/50-AddConstantsClass.png)

2. The code of this new class includes the database filename and its path:

```csharp
namespace RefreshingRecipes.Helpers
{
    public static class Constants
    {
        public const string DatabaseFilename = "RecipesDb-v1_0.db3";
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }
}

```

### Create a table class
A database consists of tables. Attributes will be added to the `Recipe` class properties. In order to promote a better generalization (in the service class), a `BasicTable` class will also be added, with properties and attributes that will be inherited to any table (class) we want.

1. In the `Models` folder, add a new class, `BasicTable.cs`:

![Add BasicTable class](/Art/51-AddBasicTableClass.png)

2. The code for this new class goes as follows. You can notice that the `Id` property contains two attributes, `PrimaryKey` and `AutoIncrement` to effectively create an identity primary key. **NOTE:** The empty constructor is intended to be there and will be needed when we implement a service to access the local database and tables. 

```csharp
using SQLite;

namespace RefreshingRecipes.Models
{
    public class BasicTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public BasicTable()
        {

        }
    }
}
```

3. Defining that `Recipe` class is a table in our database requires two attributes: `Table` and `PrimaryKey`. The first one is class-level and can be easily added before the class definition, while the second one is for a property and it can also be added by inheriting from the `BasicTable` class, which already contains a primary key definition. Just edit the `Recipe` class as follows:

```csharp
using SQLite;

namespace RefreshingRecipes.Models
{
    [Table("Recipes")]
    public class Recipe : BasicTable
    {
        public int RecipeId { get; set; }

        [MaxLength(255)]
        public string RecipeName { get; set; }
        public string RecipePhotoUrl { get; set; }
        public string RecipeInstructions { get; set; }
    }
}
```

The `MaxLength` attribute is optional.

### Create a database access service  
To provide access to the local database, a new service will be incorporated in our app. As usual, an interface will be defined first, followed by its implementation. Finally, it will be registered.

1. In the `Services` folder, add a new interface (`ILocalDbService.cs`) and class (`LocalDbService.cs`):

![Add local database service support classes](/Art/52-AddLocalDbServiceSupport.png)

2. The `ILocalDbService.cs` interface defines three methods:

* A generic method that retrieves all elements from one table.
* A generic method that returns the number of existing items in one table.
* A generic method for inserting new data.

The methods are defined as generic so that you don't need to define specific methods for each table you might have in your application (thus, reducing the code you need to write). A couple of restrictions are added in each method to avoid using them with classes or elements that aren't tables. This is the code for the interface:

```csharp
using RefreshingRecipes.Models;

namespace RefreshingRecipes.Services
{
    public interface ILocalDbService
    {
        Task<List<T>> GetItems<T>() where T : BasicTable, new();
        Task<int> CountItems<T>() where T : BasicTable, new();
        Task<bool> AddItems<T>(List<T> items) where T : BasicTable, new();
    }
}
```

3. Now we can proceed with the interface implementation, the `LocalDbService` class. In the code that is presented below you can observe:

* A constructor sets the database path to a value previously specified in `Constants` class.
* The `Init` method ensures that an `SQLiteAsyncConnection` single instance exists. This object will be able to work with the database file.
* The three methods defined in the interface are implemented. In each case, the `Init` method is invoked followed by the specific action (get, count, add items).

Code:
```csharp
using SQLite;

using RefreshingRecipes.Models;
using RefreshingRecipes.Helpers;

namespace RefreshingRecipes.Services
{
    public class LocalDbService : ILocalDbService
    {
        private string dbPath;
        private SQLiteAsyncConnection connection;

        public LocalDbService()
        {
            dbPath = Constants.DatabasePath;
        }

        private async Task Init()
        {
            if (connection != null)
                return;

            try
            {
                connection = new SQLiteAsyncConnection(dbPath);

                connection.Tracer = new Action<string>(q =>
                    System.Diagnostics.Debug.WriteLine(q));
                connection.Trace = true;

                await connection.CreateTableAsync<Recipe>();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<List<T>> GetItems<T>() where T : BasicTable, new()
        {
            await Init();
            return await connection.Table<T>().ToListAsync();
        }

        public async Task<int> CountItems<T>() where T : BasicTable, new()
        {
            await Init();
            return await connection.Table<T>().CountAsync();
        }

        public async Task<bool> AddItems<T>(List<T> items) where T : BasicTable, new()
        {
            await Init();

            var op = await connection.InsertAllAsync(items);
            return op == items.Count;
        }
    }
}
```

4. Finally, register the interface and implementation in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<ILocalDbService, LocalDbService>();
```

### Integrate the service 
Now we are ready to integrate this functionality in our app! We are modifying both the View and ViewModel part of the recipe collection.

1. Open `RecipeCollectionViewModel.cs` and:

* Add a reference to the `ILocalDbService` interface.

```csharp
ILocalDbService localDbService;
```

* Since it was registered in `MauiProgram.cs`, it means we can inject it into the constructor, so let's do that:

```csharp
    public RecipeCollectionViewModel(IRecipeService recipeService, ILocalDbService localDbService)
    {
        ...
        this.localDbService = localDbService;
    }
```

* In the `GetRecipesAsync` method, comment the line where the list of recipes is obtained from the Internet service. Then, add a new line that calls the `GetItems` method from the `localDbService` object, which effectively retrieves the recipes from the local database:

```csharp
[RelayCommand]
async Task GetRecipesAsync()
{
    ...
    try
    {
        ...
        //var recipes = (await recipeService.GetRecipes()).ToList();
        var recipes = await localDbService.GetItems<Recipe>();
        ...
    }
    ...
}
```

* Next, add a new method, `AddLocalRecipesAsync` which first obtains the recipe list from the Internet service and then proceeds to insert this data into the local database.

```csharp
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
```

2. And now, open `RecipeCollectionView.xaml` and:

* Add a new row in the grid, with an `Auto` definition

```xaml
<Grid ...
    RowDefinitions="*,Auto,Auto"
    ... />
```

* Below the **Get recipes** button, add a new one that looks similar, except that it displays the text **Add Local Recipes**, is located in the third row, and it executes the `AddLocalRecipesCommand` command when clicked:

```xaml
<Button
    FontSize="20"
    CornerRadius="25"
    BorderWidth="2"
    BorderColor="{AppThemeBinding Light={StaticResource Blue500}, Dark={StaticResource Cyan100Accent}}"
    TextColor="{AppThemeBinding Light={StaticResource Blue500}, Dark={StaticResource Cyan100Accent}}"
    BackgroundColor="{AppThemeBinding Light={StaticResource White}, Dark={StaticResource Black}}"
    Grid.Row="2"
    Margin="8"
    Command="{Binding AddLocalRecipesCommand}"
    Text="Add Local Recipes" />
```

* Finally, edit the `Grid.RowSpan` property of the `ActivityIndicator` to include the three `Grid` rows:

```xaml
<ActivityIndicator
    Grid.RowSpan="3"
    ... />
```

### Test the app
It is time to see if this works! Build and debug your app. 

First, you can see the new button:

![New UI](/Art/53-NewFunctionality.png)

Clicking on the **Get recipes** button doesn't retrieve anything, as the database is empty. Click on the **Add Local Recipes** button.

![Add local recipes](/Art/54-NewData.png)

New data has been added to the app! Click again on **Get recipes** button and you'll see that the list is populated with the local information:

![Local data](/Art/55-GetDataFromLocalDB.png)

### Add SQLite (and related) NuGet packages 
We are now adding a preference that allows the user to decide where they want to get the data from (either from an Internet service or a local database).

1. Add a new public constant as part of the `Constants` class, which represents the key (basically, the name) of the local preference:

```csharp
public const string OnlineModeKey = "online_mode";
```

2. Add a `SettingsViewModel` (in `ViewModels` folder) and `SettingsView` (in `Views` folder):

![Add Settings ViewModel and View](/Art/56-SettingsSupport.png)

3. The code for `SettingsViewModel` simply defines a boolean property and a command to save its value into the local Preferences. Moreover, the value is initially retrieved in the class constructor. The code is:

```csharp
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
```

4. Now we are going to define the UI for the previous View Model. In `SettingsView.xaml` we add `Label`, `Switch`, and `Button` controls that allows the user to choose if they want to obtain data from Internet (online mode enabled) or from a local database (online mode disabled). Take a look at the UI code, which includes all dependencies (namespaces) and controls:

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="RefreshingRecipes.Views.SettingsView"
             xmlns:vm="clr-namespace:RefreshingRecipes.ViewModels"
             xmlns:model="clr-namespace:RefreshingRecipes.Models"
             x:DataType="vm:SettingsViewModel"
             Title="{Binding Title}">
    <Grid RowSpacing="10" 
          RowDefinitions="Auto,Auto" 
          ColumnDefinitions="*,*">

        <Label Text="Online mode?"
               VerticalOptions="Center"/>

        <Switch Grid.Column="1" 
                HorizontalOptions="Start"
                VerticalOptions="Center"
                IsToggled="{Binding OnlineMode}" />

        <Button Grid.Row="1"
                Grid.ColumnSpan="2"
                Text="Save"
                Command="{Binding SaveSettingsCommand}"/>
    </Grid>
</ContentPage>
```

5. For this to work, the code-behind of the previous UI must set the page `BindingContext`. We assume the ViewModel instance is registered (and it will, in a couple steps later). So add the namespace, inject the view model object in the constructor, and assign it to the BindingContext. The code is:

```csharp
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
```

6. Next, we integrate this new functionality when we want to obtain the recipe list. The source changes according to the value of the **online mode** preference. To achieve this, open `RecipeCollectionViewModel` and implement the following changes:

* Add the `Helpers` namespace:

```csharp
using RefreshingRecipes.Helpers;
```

* Add a new boolean field in the class.

```csharp
bool onlineMode;
```

* Initialize the field in the constructor with a reference to the key defined in `Constants`; if the preference does not exist, the default value is set to `true`:

```csharp
public RecipeCollectionViewModel(...)
{
    ...
    onlineMode = Preferences.Get(Constants.OnlineModeKey, true);
}
```

* In the `GetRecipesAsync` method, comment the line that retrieves the recipe list from the local database. If you remember, we previously did the same for the line that obtained the list from Internet. In fact, we are now combining both lines to get the recipes from either of those sources, depending on the value of the `onlineMode` field. The final code is:

```csharp
[RelayCommand]
async Task GetRecipesAsync()
{
    ...
    try
    {
        ...
        //var recipes = (await recipeService.GetRecipes()).ToList();
        //var recipes = await localDbService.GetItems<Recipe>();

        var recipes = onlineMode
            ? (await recipeService.GetRecipes()).ToList()
            : await localDbService.GetItems<Recipe>();

        ...
    }
    ...
}
```

7. Modify `AppShell.xaml`. First, replace the `Shell.FlyoutBehavior` to `Flyout`; then, add a second `ShellContent` element that allows the user to show the Settings page:

```xaml
<Shell
    ...
    Shell.FlyoutBehavior="Flyout">

    ...

    <ShellContent
        Title="Settings"
        ContentTemplate="{DataTemplate views:SettingsView}"
        Route="Settings" />
</Shell>
```

8. Register the `SettingsView` and `SettingsViewModel` in `MauiProgram.cs`:

```csharp
builder.Services.AddTransient<SettingsViewModel>();
builder.Services.AddTransient<SettingsView>();
```

9. Update the URL in `GetRecipes` method in the `RecipeService` class. This new URL contains a sixth recipe, so now the Internet service has a different number of recipes (6) than the local database (5).

```csharp
public async Task<IEnumerable<Recipe>> GetRecipes()
{
    var response = await httpClient.GetAsync("https://gist.githubusercontent.com/icebeam7/a6c1c7523e67272e294204aff0b115cc/raw/938694ed82fa34384c9704f6000fa0307ca72c06/recipes.json");
    ...
}
```

10. Run the app!

* First, you'll see a hamburger menu.

![Hamburger Menu](/Art/57-HamburgerMenu.png)

* Click on it to expand its options:

![Options in hambuger menu](/Art/58-ExpandedHamburgerMenu.png)

* Click on **Settings** to see the new view:

![Settings View](/Art/59-SettingsView.png)

* Switch off the online mode option and click on the **Save** button.

![Saving settings](/Art/60-SettingsSaved.png)

* Click again on the hamburger menu and choose **Recipes**. Then, click on the **Get Recipes** button, and you will see 5 recipes:
![Getting local recipes](/Art/61-LocalRecipes.png)

** Go back to the **Settings** view and turn on the online mode this time. Save the settings:

![Saving new settings](/Art/62-NewSettings.png)

** Finally, go back to the **Recipes** page, and click on the  **Get Recipes** button. This time, you should obtain 6 recipes:

![Getting internet recipes](/Art/63-InternetRecipes)

Congratulations! You have finished Part 4! 

