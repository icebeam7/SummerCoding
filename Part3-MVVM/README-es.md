# Part 3 - MVVM
En lugar de mostrar solo una receta, nuestra aplicación ahora mostrará una lista de ellas -obteniendo la colección de recetas de una URL-, el usuario además podrá seleccionar una y ver sus detalles. ¡Implementemos el patrón MVVM en nuestra aplicación! 

### Renombra RecipeListView a RecipeDetailView
Antes de eso, hagamos un pequeño cambio. Nuestra vista actual (`RecipeListView`) en realidad no muestra una lista de recetas; en cambio, muestra el detalle de una receta. Así que vamos a cambiarle el nombre por uno más adecuado:

1. Cambie el nombre del archivo `XAML` (el archivo C# se renombra automáticamente):

![Renombrando la vista](/Art/33-RenameView.png)

2. Cambie el nombre de la clase en la vista `XAML`.

![Renombrando en XAML](/Art/34-RenameXaml.png)

3. Cambie el nombre de la clase y el constructor en el archivo C#:

![Renombrando la clase](/Art/35-RenameClass.png)

4. Cambie el nombre de la referencia a la vista en `AppShell.xaml`:

![Renombrando en AppShell](/Art/36-RenameAppShell.png)

Ahora sí estamos listos.

### Agrega el paquete NuGet CommunityToolkit.MVVM:

`CommunityToolkit.MVVM` es un paquete NuGet que simplifica la implementación de MVVM en nuestra aplicación mediante la generación automática de código fuente, por lo que se escribe menos código. ¡Vamos a incorporarlo a nuestra aplicación!

1. Haz clic derecho en el nombre de su proyecto y elige la opción **Administrar paquetes NuGet**:

![Administrar NuGet Packages](/Art/37-ManageNuGetPackage.png)

2. Haz clic en **Examinar**, busca "**communitytoolkit.mvvm**", selecciona el paquete correcto e instálalo en tu proyecto. Acepta los términos y la licencia.

![Instalando el paquete](/Art/38-InstallCommunityToolkitMVVM.png)

### Agrega los modelos
El patrón MVVM comienza con los modelos.

1. Agrega una nueva carpeta al proyecto: **Models**, luego crea una nueva clase de C# allí, **Recipe**:

![Agregando modelos](/Art/39-AddModels.png)

2. Esta clase representa la información que tendremos disponible en algún momento para una receta. Este es el código:

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

### Agrega los servicios
Por lo general, una aplicación obtiene información de otra fuente (un archivo, una REST API, una base de datos local). Para esto, se puede crear una capa de servicios que incorpore una clase reutilizable. Aún mejor, se pueden incluir interfaces para tener una separación clara de la implementación real (por ejemplo, podemos obtener la lista de recetas en nuestra aplicación desde un recurso en línea o un archivo local dependiendo de la disponibilidad o no de una conexión a Internet).

1. Agrega una nueva carpeta al proyecto: **Services** que incluirá dos nuevos elementos: una interfaz (**IRecipeService**) y una clase (**RecipeService**):

![Agregando los servicios](/Art/40-AddServices.png)

2. Como se mencionó anteriormente, una interfaz proporciona una clara separación entre la definición de un requisito (funcionalidad) y la implementación real (envío de una solicitud a un recurso en línea). Definamos el "contrato" (especificaciones) en la interfaz, que básicamente consiste en un método, `GetRecipes`, que debe devolver un `IEnumerable` de objetos `Recipe`:

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

3. Ahora, escribamos el código para la clase que implementa la interfaz anterior. Como puedes observar, se proporcionan los detalles sobre el envío de una solicitud a una URL pública. La clase cumple con todos los requisitos definidos en la interfaz, es decir, el método `GetRecipes` que devuelve un `IEnumerable` de objetos `Recipe`.

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

3. Ahora, aprovechamos el contenedor de **Inyección de dependencias** incorporado en las aplicaciones .NET MAUI para registrar el servicio de interfaz y su implementación en `MauiProgram.cs`. Agrega el espacio de nombres para `Services` y registra ambos elementos con `AddSingleton` antes de `builder.Build()`. Singleton significa que habrá una sola instancia de la clase y el contenedor devolverá una referencia a ese objeto existente cuando sea necesario. El código es el siguiente:

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

### Agrega view models
Volviendo a la implementación de MVVM, escribamos los ViewModels para la aplicación.

1. Agrega una nueva carpeta al proyecto: **ViewModels**, que tiene tres nuevas clases de C#: **BaseViewModel, RecipeCollectionViewModel, RecipeDetailViewModel**:

![Agregando view models](/Art/41-AddViewModels.png)

2. Comencemos con **BaseViewModel**, que es una clase base para todos nuestros view models e incluye tres propiedades que pueden usar las clases secundarias. Esta clase hereda de `ObservableObject`, que implementa la interfaz `INotifyPropertyChanged`, lo que significa que los bindings serán notificados cuando haya un cambio en el valor de estas propiedades. Su código es:

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

3. La clase **RecipeCollectionViewModel** será el view model de una página que obtiene y muestra una lista de recetas del servicio que se creó anteriormente. Hereda de `BaseViewModel`, define una propiedad de solo lectura para la colección de recetas y otra propiedad para la receta seleccionada por el usuario. Además, define dos comandos, uno que obtiene la colección de recetas y otro que navega a una segunda página. Cabe mencionar que se inyecta la interfaz `IRecipeService` en el constructor, lo cual está permitido ya que se registró previamente en el contenedor de inyección de dependencias. El código es el siguiente:

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

4. El último view model que se implementará es **RecipeDetailViewModel**, que simplemente define una propiedad para la receta que se mostrará. El atributo `QueryProperty` define un argumento que se envía desde otra página, es decir, la receta seleccionada. Este es el código:

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

5. El contenedor de inyección de dependencias es excelente para crear instancias de view models porque queremos inyectarlas más tarde en nuestras vistas, así que registrémoslas en `MauiProgram.cs`. Agrega el espacio de nombres para la carpeta `ViewModels` y registra los view models con `AddTransient` (se crea un objeto cada vez que se requiere) o `AddSingleton` (una única instancia durante el ciclo de vida de la aplicación) antes de `builder.Build()`:

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

### Agrega views:
El último elemento de MVVM es Vistas, que representa la interfaz de usuario.

1. Ya tenemos la carpeta **Views**, agreguemos una nueva `ContentPage` con el nombre **RecipeCollectionView**:

![Agregando vistas](/Art/42-AddViews.png)

2. Esta página obtiene y muestra una lista de recetas. Primero, inyectemos el modelo de vista asociado en el constructor y lo configuramos como `BindingContext` de la página, en el código de la clase C#:

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

3. Luego, definimos la interfaz de usuario con 3 elementos: un `CollectionView` que muestra la lista de recetas, un `Button` que obtiene la colección de una URL y un `ActivityIndicator` que muestra una animación de carga mientras se transfieren los datos desde Internet a nuestra aplicación. El código XAML correspondiente es el siguiente, presta atención a cómo se están referenciando las propiedades y comandos de `RecipeCollectionViewModel` mediante bindings en cada uno de los 3 elementos de la interfaz de usuario:

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

4. Del mismo modo, ahora configuremos `BindingContext` para la página `RecipeDetailView` con una inyección en el constructor. **Reto: ¿Puedes implementar la funcionalidad `ShareButton` como un comando en el view model?**

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

5. Ya tenemos la interfaz de usuario para esta vista de detalles. Sin embargo, aún necesitamos vincular los elementos de la interfaz de usuario a la propiedad `Recipe` de `RecipeDetailViewModel` y las propiedades del modelo. Así que configuremos los bindings en `XAML` como se indica a continuación:

Primero, agrega los espacios de nombres y modifica el título:

```xaml
             xmlns:vm="clr-namespace:RefreshingRecipes.ViewModels"
             xmlns:model="clr-namespace:RefreshingRecipes.Models"
             x:DataType="vm:RecipeDetailViewModel"
             Title="{Binding Recipe.RecipeName}">
```

Identifique la propiedad `Source` del control `Image`. En lugar de usar un valor estático, usaremos la dirección URL de la receta seleccionada:

```xaml
                <Image 
                ...
                   Source="{Binding Recipe.RecipePhotoUrl}"
                   />
```

Y finalmente, reemplaza el valor de la propiedad `Text` del `Label` con el nombre de la receta seleccionada:

```xaml
                    <Label Text="{Binding Recipe.RecipeName}"
                           ...
                           />
```

6. Las vistas se pueden registrar y resolver en `MauiProgram.cs` de la misma manera que lo hicimos antes para los servicios y los view models. Por lo tanto, agrega el espacio de nombres para la carpeta `Vistas` y registra las vistas con `AddTransient` antes de `builder.Build()`:

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

7. Ahora queremos mostrar una lista de recetas cuando se ejecuta la aplicación. Para hacer eso, modifica `ContentTemplate` para el único `ShellContent` existente en `AppShell.xaml` para que despliegue la página `RecipeCollectionView` al iniciar la aplicación:

```xaml
    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate views:RecipeCollectionView}"
        Route="Recipes" />
```

8. Y también debemos registrar una nueva ruta en `AppShell.xaml.cs` dentro del constructor que se usa cuando se selecciona una receta de la lista, lo que permite la navegación a la vista de detalles:

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

9. ¡Eso es todo! Ahora podemos probar la aplicación. Aquí están los resultados:

En primer lugar, esta es la lista de recetas:
![Lista de recetas](/Art/43-RecipeCollection.png)

Cuando seleccionas uno, la aplicación navega a una segunda vista con los detalles de la receta:
![Detalles de la receta](/Art/44-RecipeDetail.png)

¡Felicidades! ¡Has terminado la Parte 3! Continuemos y aprendamos sobre el almacenamiento local en la [Parte 4](/Part4-LocalStorage/README-es.md).