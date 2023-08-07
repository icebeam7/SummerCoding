# Part 3 - MVVM
Let's implement the MVVM pattern in our app!

### Rename RecipeListView to RecipeDetailView
Before that, let's do a small change. Our current view (RecipeListView) doesn't actually display a list of recipes; instead, it shows the detail of one recipe. So let's rename it to a proper name:

1. Rename the XAML file (the C# file is renamed automatically)

![Rename View](/Art/33-RenameView.png)

2. Rename the class name in the XAML view.

![Rename Xaml](/Art/34-RenameXaml.png)

3. Rename the class name and constructor in the C# file>

![Rename class](/Art/35-RenameClass.png)

4. Rename the class reference in AppShell:

![Rename in AppShell](/Art/36-RenameAppShell.png)

Now we are ready.

### Add CommunityToolkit.MVVM NuGet package:

1. Right-click on your project name and choose **Manage NuGet Packages**:

![Manage NuGet Packages](/Art/37-ManageNuGetPackage.png)

2. Click on Browse, search "communitytoolkit.mvvm", select the right package, and install it on your project. Accept the terms and license.

![Install the package](/Art/38-InstallCommunityToolkitMVVM.png)

### Add models

1. Add a new folder to the project: **Models**, and also add a new C# class inside, **Recipe**:

![Add new folders](/Art/39-AddNewFolders.png)

2. This is the code of the newly created class:

```csharp
namespace RefreshingRecipes.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string RecipePhotoUrl { get; set; }
        public string RecipeInstructions { get; set; }
    }
}
```

### Add services

1. Add a new folder to the project: **Services**, and also add two new elements: an interface (**IRecipeService**) and a C# class (**RecipeService**):

![Add services](/Art/40-AddServices.png)

2. This is the code for the newly created interface:

```csharp
using RefreshingRecipes.Models;

namespace RefreshingRecipes.Services
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetRecipes();
    }
}
```

3. Next, add this code for the class:

```csharp
using System.Net.Http.Json;

using RefreshingRecipes.Models;

namespace RefreshingRecipes.Services
{
    public class RecipeService : IRecipeService
    {
        HttpClient httpClient;

        public RecipeService()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<IEnumerable<Recipe>> GetRecipes()
        {
            var response = await httpClient.GetAsync("https://gist.githubusercontent.com/icebeam7/a6c1c7523e67272e294204aff0b115cc/raw/84d3d9c1b25d924630b15d901fa38dbbd13f20fc/recipes.json");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<IEnumerable<Recipe>>();

            return default;
        }
    }
}
```

3. Now, we need to register the service in `MauiProgram.cs`. Add the namespace for `Services` folder and register the service with `AddSingleton` before `builder.Build()`:

```csharp
using RefreshingRecipes.Services;
...
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
...
        builder.Services.AddSingleton<IRecipeService, RecipeService>();
...
```

### Add view models

1. Add a new folder to the project: **ViewModels**, and also add three new C# classes inside, **BaseViewModel, RecipeCollectionViewModel, RecipeDetailViewModel**:

![Add view models](/Art/41-AddViewModels.png)

2. Let's start with **BaseViewModel**, which is a base class for all our view models. The code is:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace RefreshingRecipes.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        string title;

        public bool IsNotBusy => !IsBusy;
    }
}
```

3. The next class we are implementing is **RecipeCollectionViewModel**:

```csharp
using System.Collections.ObjectModel;

using RefreshingRecipes.Views;
using RefreshingRecipes.Models;
using RefreshingRecipes.Services;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RefreshingRecipes.ViewModels
{
    public partial class RecipeCollectionViewModel : BaseViewModel
    {
        public ObservableCollection<Recipe> Recipes { get; } = new();

        IRecipeService recipeService;

        [ObservableProperty]
        Recipe selectedRecipe;

        public RecipeCollectionViewModel(IRecipeService recipeService)
        {
            Title = "Recipe List";
            this.recipeService = recipeService;
        }

        [RelayCommand]
        async Task GetRecipesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var recipes = (await recipeService.GetRecipes()).ToList();

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
    }
}
```

4. The final viewmodel to be implemented is **RecipeDetailViewModel**. This is the code:

```csharp
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
```

5. Register the view models in `MauiProgram.cs`. Add the namespace for `ViewModels` folder and register the view models with `AddTransient` or `AddSingleton` before `builder.Build()`:

```csharp
...
using RefreshingRecipes.ViewModels;
...
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
...
		builder.Services.AddSingleton<RecipeCollectionViewModel>();
		builder.Services.AddTransient<RecipeDetailViewModel>();
...
```

### Add views:
1. We already have the **Views** folder, let's just add a new ContentPage: **RecipeCollectionView**:

![Add view models](/Art/42-AddViews.png)

2. This is the C# code:

```csharp
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
```

3. And the corresponding XAML code, where we bind everything:

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="RefreshingRecipes.Views.RecipeCollectionView"
             xmlns:vm="clr-namespace:RefreshingRecipes.ViewModels"
             xmlns:model="clr-namespace:RefreshingRecipes.Models"
             x:DataType="vm:RecipeCollectionViewModel"
             Title="{Binding Title}">
    <Grid Margin="5"
          RowDefinitions="*,Auto"
          RowSpacing="0">

        <CollectionView ItemsSource="{Binding Recipes}"
                        SelectionMode="Single"
                        SelectedItem="{Binding SelectedRecipe}"
                        SelectionChangedCommand="{Binding GoToDetailsCommand}"
                        BackgroundColor="Transparent">

            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="model:Recipe">
                    <HorizontalStackLayout Spacing="10">
                        <Image WidthRequest="300"
                               HeightRequest="200"
                               Source="{Binding RecipePhotoUrl}"
                               Aspect="AspectFill"/>

                        <Label Text="{Binding RecipeName}"
                               FontSize="Medium"
                               VerticalOptions="Center"
                               TextColor="Black"/>
                    </HorizontalStackLayout>

                </DataTemplate>
            </CollectionView.ItemTemplate>

        </CollectionView>

        <Button
            Grid.Row="1"
            Margin="8"
            Command="{Binding GetRecipesCommand}"
            Text="Get Recipes" />

        <ActivityIndicator
            Grid.RowSpan="2"
            HorizontalOptions="FillAndExpand"
            IsRunning="{Binding IsBusy}"
            IsVisible="{Binding IsBusy}"
            VerticalOptions="CenterAndExpand" />

    </Grid>
```


</ContentPage>

4. Now, modify the C# code for **RecipeDetailView** class:

```csharp
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
```

5. Next, set the bindings in XAML like this:

First, add namespaces and modify the title:

```xaml
             xmlns:vm="clr-namespace:RefreshingRecipes.ViewModels"
             xmlns:model="clr-namespace:RefreshingRecipes.Models"
             x:DataType="vm:RecipeDetailViewModel"
             Title="{Binding Recipe.RecipeName}">
```

Identify the `Source` property of the `Image`. Instead of using a static value, we'll use the one from the selected recipe:

```xaml
                <Image 
                ...
                   Source="{Binding Recipe.RecipePhotoUrl}"
                   />
```

And now, replace the Text value for the Label with the name of the selected recipe:
```xaml
                    <Label Text="{Binding Recipe.RecipeName}"
                           ...
                           />
```

6. Now, register the view models in `MauiProgram.cs`. Add the namespace for `Views` folder and register the views with `AddTransient` before `builder.Build()`:

```csharp
...
using RefreshingRecipes.Views;
...
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
...
		builder.Services.AddTransient<RecipeCollectionView>();
		builder.Services.AddTransient<RecipeDetailView>();
...
```

7. Modify AppShell.xaml so it displays RecipeCollectionView when the application runs:

```xaml
    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate views:RecipeCollectionView}"
        Route="Recipes" />
```

8. And register a new Route in AppShell.xaml.cs inside the constructor:

```csharp
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
```

9. Test the app!

![Testing the app](/Art/43-RecipeCollection.png)

![Testing the app](/Art/44-RecipeDetail.png)
