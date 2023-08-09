# Part 3 - MVVM
Instead of displaying just one recipe, our application now will show a list of them -obtaining the recipe collection from a URL-, the user will be able to select one and see its details. Let's implement the MVVM pattern in our app! 

### Rename RecipeListView to RecipeDetailView
Before that, let's do a small change. Our current view (`RecipeListView`) doesn't actually display a list of recipes; instead, it shows the detail of one recipe. So let's rename it to a proper name:

1. Rename the `XAML` file (the C# file is renamed automatically)

![Rename View](/Art/33-RenameView.png)

2. Rename the class name in the `XAML` view.

![Rename Xaml](/Art/34-RenameXaml.png)

3. Rename the class name and constructor in the C# file:

![Rename class](/Art/35-RenameClass.png)

4. Rename the class reference in `AppShell.xaml`:

![Rename in AppShell](/Art/36-RenameAppShell.png)

Now we are ready.

### Add CommunityToolkit.MVVM NuGet package:

`CommunityToolkit.MVVM` is a NuGet package that simplifies the implementation of MVVM in our application by auto-generating source code, thus writing less code. Let's incorporate it into our app!

1. Right-click on your project name and choose **Manage NuGet Packages**:

![Manage NuGet Packages](/Art/37-ManageNuGetPackage.png)

2. Click on **Browse**, search "**communitytoolkit.mvvm**", select the right package, and install it on your project. Accept the terms and license.

![Install the package](/Art/38-InstallCommunityToolkitMVVM.png)

### Add models
The MVVM pattern starts with **Models**.

1. Add a new folder to the project: **Models**, then create a new C# class there, **Recipe**:

![Add models](/Art/39-AddModels.png)

2. This class represents the information we will have available at some point for a recipe. This is the code:

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
Usually, an application obtains information from another source (a file, a REST API, a local database). A **Services** layer that incorporates a reusable class can be created for this. Even better, interfaces can be included as well in order to have a clean separation from the actual implementation (for example, we can obtain the list of recipes in our app from either an online resource or a local file depending on the availability of an Internet connection).

1. Add a new folder to the project: **Services** that will include two new elements: an interface (**IRecipeService**) and a class (**RecipeService**):

![Add services](/Art/40-AddServices.png)

2. As previously mentioned, an interface provides a clean separation between the definition of a requirement (functionality) and the actual implementation (sending a request to an online resource). Let's define the "contract" (specifications) in the interface, which basically consists of one method, `GetRecipes`, which must return an `IEnumerable` of `Recipe` objects:

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

3. Now, let's write the code for the class, which implements the above interface. As you can observe, the specifics on sending a request to a public URL are provided. The class meets all requirements defined in the interface, that is, the method `GetRecipes`, which returns an `IEnumerable` of `Recipe` objects.

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

3. Now, we take advantage of the built-in **Dependency Injection** container to register the interface service and its implementation in `MauiProgram.cs`. Add the namespace for `Services` and register both elements with `AddSingleton` before `builder.Build()`. Singleton means that there will be a single instance of the class, and the container will return a reference to that existing object when it is required. Code goes as follows:

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
Back to the MVVM implementation, let's write the ViewModels for the app.

1. Add a new folder to the project: **ViewModels**, which has three new C# classes: **BaseViewModel, RecipeCollectionViewModel, RecipeDetailViewModel**:

![Add view models](/Art/41-AddViewModels.png)

2. Let's start with **BaseViewModel**, which is a base class for all our view models, and it includes three properties that can be used by children classes. This class inherits from `ObservableObject`, which implements the `INotifyPropertyChanged` interface, meaning that bindings will be notified when there's a change in the value of these properties. Its code is:

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

3. The **RecipeCollectionViewModel** class will be the View Model of a page that obtains and displays a list of recipes from the service that was created earlier. It inherits from `BaseViewModel`, defines a read-only property for the recipe collection, and another property for the recipe selected by the user. Moreover, it defines two commands, one that gets the recipe collection, and a second one that navigates to a second page. It is worth mentioning that the interface `IRecipeService` is injected into the constructor, which is allowed since it was previously registered in the dependency injection container. The code goes as follows:

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

4. The last viewmodel to be implemented is **RecipeDetailViewModel**, which simply defines a property for the recipe that will be displayed. The `QueryProperty` attribute defines an argument that is sent from another page, that is, the selected recipe. This is the code:

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

5. The dependency injection container is great for creating view model instances because we want to inject them later in our views, so let's register them in `MauiProgram.cs`. Add the namespace for `ViewModels` folder and register the view models with `AddTransient` (an object is created each time it is required) or `AddSingleton` (one single instance during the app lifecycle) before `builder.Build()`:

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
The last element from MVVM is Views, which stands for the UI. 

1. We already have the **Views** folder, let's just add a new `ContentPage` with the name **RecipeCollectionView**:

![Add views](/Art/42-AddViews.png)

2. This page obtains and shows a list of recipes. First, let's inject the associated view model in the constructor and set it as the page's `BindingContext` in the C# code:

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

3. Then, we define the UI with 3 elements: A `CollectionView` that displays the recipe list, a `Button` that obtains the collection from a URL, and an `ActivityIndicator` that shows a loading animation while the data is transferred from the Internet to our app. The corresponding XAML code goes as follows, please notice the different bindings to the `RecipeCollectionViewModel` properties and commands in each of the 3 UI elements:

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

4. Likewise, we are setting the `BindingContext` for `RecipeDetailView` page with a constructor injection. **Challenge: Can you implement the `ShareButton` functionality as a command in the view model?**

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

5. We already have the UI for this detail view. However, we still need to bind the UI elements to the `Recipe` property from `RecipeDetailViewModel` and the model properties. So let's set the bindings in `XAML` like this:

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

And finally, replace the `Text` value for the `Label` with the name of the selected recipe:
```xaml
                    <Label Text="{Binding Recipe.RecipeName}"
                           ...
                           />
```

6. Views can ve registered an resolved in `MauiProgram.cs` the same way we did it before for services and view models. So, add the namespace for `Views` folder and register the views with `AddTransient` before `builder.Build()`:

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

7. We want now to show a list of recipes when the application runs. In order to do that, modify the `ContentTemplate` for the only `ShellContent` in `AppShell.xaml` so it displays the `RecipeCollectionView` page at the beginning:

```xaml
    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate views:RecipeCollectionView}"
        Route="Recipes" />
```

8. And we also must register a new `Route` in `AppShell.xaml.cs` inside the constructor that is used when a recipe is selected from the list, enabling navigation to the details view:

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

9. That's it! We can now test the application. Here are the results:

First, this is the list of recipes:
![List of recipes](/Art/43-RecipeCollection.png)

When you tap on one, the app navigates to a second view with the recipe details:
![Recipe details](/Art/44-RecipeDetail.png)

Congratulations! You have finished Part 3! Let's continue and learn about local storage in [Part 4](/Part4-LocalStorage/README.md).