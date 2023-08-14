# Parte 4 - Persistencia de datos locales
Para la parte final, se implementará el modo "offline", lo que significa que el usuario puede obtener la lista de recetas de Internet o de una base de datos local.

### Agregar paquetes NuGet SQLite (y relacionados) 
El motor de base de datos SQLite permite que las aplicaciones .NET MAUI carguen y guarden objetos de datos en código compartido. Puede integrar SQLite.NET en aplicaciones .NET MAUI agregando algunos paquetes NuGet:

1. Haz clic derecho en el proyecto y selecciona **Administrar paquetes NuGet**:

![Agregando nuevos paquetes Nuget](/Art/46-ManageNuGetPackage.png)

2. El paquete NuGet principal es `SQLite.NET`. Busca `sqlite-net-pcl` y asegúrate de seleccionar e instalar el paquete creado por **SQLite-net**:

![Agregando el paquete NuGet SQLite Net](/Art/47-AddSQLiteNetPCLNuGetPackage.png)

3. Además de sqlite-net-pcl, debes instalar temporalmente la dependencia subyacente que expone SQLite en cada plataforma. Busca `SQLitePCLRaw.bundle_green` e instálalo:

![Agregando el paquete NuGet SQLite PCL raw](/Art/48-AddSQLitePCLRawBundleGreenNuGetPackage.png)

4. Puedes continuar con la siguiente parte. Si obtienes un error al probar la aplicación en Android, Windows o iOS, regresa aquí e instala las siguientes dependencias (agrega un paquete y prueba la aplicación, si funciona, eso es todo; de lo contrario, instala el siguiente paquete de la lista y vuelve a probar).

* SQLitePCLRaw.provider.dynamic_cdecl
* SQLitePCLRaw.provider.sqlite3
* SQLitePCLRaw.core

![Agregando el paquete NuGet SQLite PCL Raw Bundle Provider Cdecl](/Art/49-AddSQLitePCLRawBundleProviderCdeclNuGetPackage.png)

### Configura constantes de la aplicación
Los datos de configuración, como el nombre de archivo y la ruta de la base de datos, se pueden almacenar como constantes en la app.

1. En la carpeta `Helpers`, agrega una nueva clase, `Constants.cs`:

![Agregando la clase Constants en Helpers](/Art/50-AddConstantsClass.png)

2. El código de esta nueva clase incluye el nombre del archivo de la base de datos y su ruta:

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

### Crea a una clase tabla
Una base de datos consta de tablas. Se agregarán atributos a las propiedades de la clase `Recipe`. Para promover una mejor generalización (en la clase de servicio), también se agregará una clase `BasicTable`, con propiedades y atributos que se heredarán a cualquier tabla (clase) que queramos.

1. En la carpeta `Models`, agrega una nueva clase, `BasicTable.cs`:

![Agregando la clase BasicTable](/Art/51-AddBasicTableClass.png)

2. El código de esta nueva clase se muestra a continuación. Puedes notar que la propiedad `Id` contiene dos atributos, `PrimaryKey` y `AutoIncrement` para crear efectivamente una llave primaria autoincremental. **NOTA:** El constructor vacío está puesto a propósito y será necesario cuando implementemos un servicio para acceder a la base de datos y las tablas locales.

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

3. Definir que la clase `Recipe` es una tabla en nuestra base de datos requiere dos atributos: `Table` y `PrimaryKey`. El primero es de nivel de clase y se puede agregar fácilmente antes de la definición de clase, mientras que el segundo es para una propiedad y se puede añadir heredándolo de la clase `BasicTable`, que ya contiene una definición de llave primaria. Simplemente edite la clase `Recipe` de la siguiente manera:

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

El atributo `MaxLength` es opcional.

### Crea un servicio de acceso a la base de datos
Para brindar acceso a la base de datos local, se incorporará un nuevo servicio en nuestra aplicación. Como es habitual, primero se definirá una interfaz, seguida de su implementación. Finalmente, se registrará.

1. En la carpeta `Services`, agrega una nueva interfaz (`ILocalDbService.cs`) y una nueva clase (`LocalDbService.cs`):

![Agregando clases de soporte de servicio de base de datos local](/Art/52-AddLocalDbServiceSupport.png)

2. La interfaz `ILocalDbService.cs` define tres métodos:

* Un método genérico que recupera todos los elementos de una tabla.
* Un método genérico que devuelve el número de elementos existentes en una tabla.
* Un método genérico para insertar nuevos datos.

Los métodos se definen como genéricos para que no se necesite definir métodos específicos para cada tabla que puedas tener en tu aplicación (reduciendo así el código que necesitas escribir). Se agregan un par de restricciones en cada método para evitar usarlos con clases o elementos que no son tablas. Este es el código de la interfaz:

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

3. Ahora podemos continuar con la implementación de la interfaz, la clase `LocalDbService`. En el código que se presenta a continuación se puede observar:

* Un constructor establece la ruta de la base de datos a un valor previamente especificado en la clase `Constants`.
* El método `Init` asegura que existe una única instancia de `SQLiteAsyncConnection`. Este objeto podrá trabajar con el archivo de la base de datos.
* Se implementan los tres métodos definidos en la interfaz. En cada caso, se invoca el método `Init` seguido de la acción específica (obtener, contar, agregar elementos).

Código:
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

4.  Finalmente, registra la interfaz y la implementación en `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<ILocalDbService, LocalDbService>();
```

### Integra el servicio 
¡Ahora estamos listos para integrar esta funcionalidad en nuestra aplicación! Modificaremos la View y el ViewModel de la colección de recetas.

1. Abre `RecipeCollectionViewModel.cs` y:

* Agrega una referencia a la interfaz `ILocalDbService`.

```csharp
ILocalDbService localDbService;
```

* Dado que el servicio se registró en `MauiProgram.cs`, podemos inyectarlo en el constructor, así que hagámoslo:

```csharp
    public RecipeCollectionViewModel(IRecipeService recipeService, ILocalDbService localDbService)
    {
        ...
        this.localDbService = localDbService;
    }
```

* En el método `GetRecipesAsync`, comenta la línea donde se obtiene la lista de recetas del servicio de Internet. Luego, agrega una nueva línea que llama al método `GetItems` desde el objeto `localDbService`, que efectivamente recupera las recetas de la base de datos local:

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

* A continuación, añade un nuevo método, `AddLocalRecipesAsync`, que primero obtiene la lista de recetas del servicio de Internet y luego procede a insertar estos datos en la base de datos local.

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

2. Y ahora, abre `RecipeCollectionView.xaml` y:

* Agrega una nueva fila en el grid, con una definición de tamaño `Auto`:

```xaml
<Grid ...
    RowDefinitions="*,Auto,Auto"
    ... />
```

* Debajo del botón **Get recipes**, agrega un nuevo botón prácticamente igual, excepto que muestra el texto **Add Local Recipes**, se encuentra en la tercera fila y ejecuta el comando `AddLocalRecipesCommand` cuando es presionado:

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

* Finalmente, edita la propiedad `Grid.RowSpan` del `ActivityIndicator` para que abarque las tres filas del `Grid`:

```xaml
<ActivityIndicator
    Grid.RowSpan="3"
    ... />
```

### Prueba la aplicación
¡Es hora de ver si esto funciona! Compila y depura la aplicación.

Primero, puedes ver el nuevo botón:

![Nueva interfaz de usuario](/Art/53-NewFunctionality.png)

Al hacer clic en el botón **Get recipes** no se muestra nada, ya que la base de datos está vacía. Haz clic en el botón **Add Local Recipes**.

![Agregando recetas locales](/Art/54-NewData.png)

¡Se ha añadido nueva información a la base de datos de la app! Haz clic nuevamente en el botón **Get recipes** y verás que la lista se muestra con la información local:

![Datos locales](/Art/55-GetDataFromLocalDB.png)

### Agrega preferencias del usuario 
Ahora agregaremos una preferencia, la cual le permite al usuario decidir de dónde quiere obtener los datos (ya sea de un servicio de Internet o de una base de datos local).

1. Agrega una nueva constante pública como parte de la clase `Constants`, la cual representa la clave (básicamente, el nombre) de la preferencia local:

```csharp
public const string OnlineModeKey = "online_mode";
```

2. Agrega una clase llamada `SettingsViewModel` (en la carpeta `ViewModels`) y una ContentPage llamada `SettingsView` (en la carpeta `Views`):

![Agregando la ViewModel y View de Settings](/Art/56-SettingsSupport.png)

3. En `SettingsViewModel` simplemente se define una propiedad booleana y un comando para guardar un valor en las preferencias locales. Además, éste se recupera al inicio en el constructor de la clase. El código es:

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

4. Ahora vamos a definir la interfaz de usuario relacionada con la View Model anterior. En `SettingsView.xaml`, agregamos los controles `Label`, `Switch` y `Button` que permitirán al usuario elegir si desea obtener datos de Internet (modo online habilitado) o de una base de datos local (modo online deshabilitado). Revisa el código de la interfaz de usuario mostrado a continuación, el cual incluye todas las dependencias (espacios de nombres) y controles:

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

5. Para que esto funcione, el code-behind de la interfaz de usuario anterior debe establecer el `BindingContext` de la página. Asumimos que la instancia de ViewModel está registrada (y lo estará, en un par de pasos más adelante), así que agrega el espacio de nombres, inyecta el objeto del modelo de vista en el constructor y asígnalo al BindingContext. El código es el siguiente:

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

6. A continuación, integramos esta nueva funcionalidad para cuando deseemos obtener la lista de recetas. El origen de datos cambia según el valor de la preferencia **onlineMode**. Para lograr esto, abre `RecipeCollectionViewModel` e implementa los siguientes cambios:

* Agrega el espacio de nombres `Helpers`:

```csharp
using RefreshingRecipes.Helpers;
```

* Agrega un nuevo campo booleano en la clase.

```csharp
bool onlineMode;
```

* Inicializa el campo en el constructor con una referencia a la clave definida en `Constants`; si la preferencia no existe, el valor predeterminado se establece en `true`:

```csharp
public RecipeCollectionViewModel(...)
{
    ...
    onlineMode = Preferences.Get(Constants.OnlineModeKey, true);
}
```

* En el método `GetRecipesAsync`, comenta la línea que recupera la lista de recetas de la base de datos local. Si recuerdas, anteriormente hicimos lo mismo con la línea que obtenía los datos de Internet. De hecho, ahora vamos a combinar ambas líneas para obtener las recetas de cualquiera de esas fuentes, dependiendo del valor del campo `onlineMode`. El código final es:

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

7. Modifica `AppShell.xaml`. Primero, coloca el `Shell.FlyoutBehavior` en `Flyout`; luego, agrega un segundo elemento `ShellContent` que le permita al usuario mostrar la página de Settings:

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

8. Registra `SettingsView` y `SettingsViewModel` en `MauiProgram.cs`:

```csharp
builder.Services.AddTransient<SettingsViewModel>();
builder.Services.AddTransient<SettingsView>();
```

9. Actualiza la URL en el método `GetRecipes` de la clase `RecipeService`. Esta nueva URL contiene una sexta receta, por lo que ahora el servicio de Internet tiene un número diferente de recetas (6) que la base de datos local (5).

```csharp
public async Task<IEnumerable<Recipe>> GetRecipes()
{
    var response = await httpClient.GetAsync("https://gist.githubusercontent.com/icebeam7/a6c1c7523e67272e294204aff0b115cc/raw/938694ed82fa34384c9704f6000fa0307ca72c06/recipes.json");
    ...
}
```

10. ¡Ejecuta la aplicación!

* Primero, verás un menú hamburguesa en nuestra aplicación.

![Menú Hamburguesa](/Art/57-HamburgerMenu.png)

* Pulsa sobre él para ver las opciones del menú:

![Opciones en el menú](/Art/58-ExpandedHamburgerMenu.png)

* Haz clic en **Settings** para accede a la nueva página:

![Página de configuración](/Art/59-SettingsView.png)

* Desactiva la opción de modo en línea y haz clic en el botón **Save**.

![Guardando la configuración](/Art/60-SettingsSaved.png)

* Vuelve a hacer clic en el menú de hamburguesas y elige **Recipes**. Luego, haz clic en el botón **Get recipes** y verás 5 recetas:
![Obteniendo recetas locales](/Art/61-LocalRecipes.png)

** Vuelve a la página de **Settings** y esta ocasión activa el modo en línea. Guarda la configuración:

![Guardando la nueva configuración](/Art/62-NewSettings.png)

* Finalmente, regresa a la página **Recipes** y haz clic en el botón **Get recipes**. Esta vez, deberías obtener 6 recetas:

![Obteniendo recetas de Internet](/Art/63-InternetRecipes)

¡Felicidades! ¡Has terminado la Parte 4!

