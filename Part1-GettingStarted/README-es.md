# Parte 1 - Primeros Pasos
Comencemos con una descripción general básica de .NET MAUI y cómo se estructuran los proyectos.

### Crea una nueva Solución en Visual Studio 
1. Abre **Visual Studio 2022** y haz clic en la opción **Crear un nuevo proyecto**.

![Creando un nuevo proyecto en Visual Studio 2022](/Art/01-OpenVS.png)

2. Elige la plantilla de proyecto **.NET MAUI App**. Puedes buscar el término "maui" para fácilmente encontrarlo.

![Seleccionando la plantilla .NET MAUI App](/Art/02-SelectNetMauiTemplate.png)

3. Configuremos el proyecto y luego hagamos clic en **Siguiente** posteriormente:
* Nombre del proyecto: **RefreshingRecipes**
* Ubicación del proyecto: Se recomienda utilizar una ruta corta.

![Configurando un proyecto .NET MAUI](/Art/03-ConfigureProject.png)

4. Selecciona la versión de Framework **.NET 7.0** y haz clic en **Crear**.

![Seleccionando la versión de .NET framework](/Art/04-SelectNetFrameworkVersion.png)

Después de unos segundos, se creará una solución que contiene un proyecto. El proyecto principal **RefreshingRecipes** de tipo .NET MAUI se puede implementar en Android, iOS, macOS y Windows. 

![Vista de la solución de la aplicación RefreshingRecipes](/Art/05-SolutionStructure.png)

El proyecto **RefreshingRecipes** también tiene archivos de código y páginas XAML que usaremos durante el taller.

Visita este [enlace](https://github.com/dotnet-presentations/dotnet-maui-workshop/blob/63c011c7dc29d50d063000714f6ee3a626a3e840/Part%200%20-%20Overview/README.md#understanding-the-net-maui-single-project) para obtener una maravillosa explicación sobre la estructura de proyecto único de .NET MAUI. Cada carpeta (Resources, Fonts, Images, ...) se explica en detalle y el archivo de inicio (MauiProgram.cs) se cubre a fondo.

### Agrega una nueva vista
Ahora vamos a crear una nueva vista que se modificará para mostrar información de nuestra propia aplicación.

1. En el **Explorador de Soluciones**, haz clic con el botón derecho en el nombre del proyecto y elige la opción **Agregar > Nueva carpeta**, luego asígnale el nombre **Views**.

![Agregando una nueva carpeta](/Art/06-AddNewFolder.png)

2. Haz clic derecho en la carpeta **Views** y elige la opción **Agregar > Nuevo elemento**.

![Agregando un nuevo elemento](/Art/07-AddNewItem.png)

3. Selecciona la categoría **.NET MAUI** y haz clic en la plantilla **.NET MAUI ContentPage (XAML)**. Nómbralo **RecipeListView.xaml**.

![Agregando una nueva página de contenido](/Art/08-AddNewContentPage.png)

Puedes observar que se han agregado dos nuevos elementos a la carpeta **Views**: un archivo **XAML** y un archivo **C#**.

![Contenido de la carpeta Views](/Art/09-ViewsFolderContent.png)

### Agregando información a la vista
Vamos a mostrar una vista básica de una bebida refrescante que incluye una imagen, su nombre y un botón que luego se utilizará para compartir información sobre la bebida.

1. Antes de eso, agreguemos una imagen a nuestro proyecto. Desde la carpeta [Resources](/Resources/) ubicada en este repositorio, descarga la imagen [smoothie.png](/Resources/smoothie.png).

![Un refrescante smoothie](/Resources/smoothie.png)

2. En el Explorador de soluciones, abre la carpeta **Resources** y haz clic con el botón derecho en la subcarpeta Images, elige la opción **Agregar > Elemento existente**:

![Agregando un elemento existente](/Art/10-AddExistingItem.png)

3. Selecciona el archivo **smoothie.png** que descargaste. Es posible que debas seleccionar la opción **Todos los archivos (*.*)** para poder encontrar la imagen. Haz clic en el botón **Agregar**.
  
![Agregando una imagen](/Art/11-AddImage.png)

La imagen ha sido añadida a nuestro proyecto.

![Nuevo recurso añadido al proyecto](/Art/12-NewResource.png)

4. Haz doble clic en el archivo **RecipeListView.xaml** para abrirlo. El editor muestra el código XAML, que representa la interfaz de usuario que se presenta cuando se ejecuta la aplicación.

![Contenido inicial de RecipeListView.xaml](/Art/13-InitialContent.png)

5. Reemplaza el código del elemento **VerticalStackLayout** con el siguiente contenido:

```xaml
    <VerticalStackLayout Margin="10" Spacing="10">
        <Image WidthRequest="600"
               HeightRequest="400"
               Source="smoothie.png"
               Aspect="AspectFill"/>

        <Label Text="Raspberry smoothie"
               FontSize="Medium"
               HorizontalOptions="Center"
               />

        <Button Text="Share"
                WidthRequest="400"
                x:Name="ShareButton"/>
    </VerticalStackLayout>
```

¿Qué se agregó en el código?

* El elemento **VerticalStackLayout** organiza vistas (controles) internas en una pila unidimensional verticalmente. Tiene un margen de 10 unidades y la cantidad de espacio entre cada control interno también es de 10 unidades.

* Un control **Image** muestra un archivo de imagen al que hace referencia el elemento **Source**, con el tamaño específicado (ancho y alto). **AspectFill** redimensiona la imagen para que llene el área de visualización mientras conserva la relación de aspecto.

* Un control **Label** muestra un texto con el contenido, el tamaño de fuente y la posición especificados.

* Un control **Button** muestra un botón con el ancho y el contenido designados. También incluye un identificador para uso posterior en el código C# asociado.


Como referencia, así es como debería verse el archivo **RecipeListView.xaml**:

![Código XAML de RecipeListView](/Art/14-RecipeListViewXAMLCode.png)

### Agregando funcionalidad a la vista
Ahora, agreguemos algo de funcionalidad a esta página. Abre el archivo **RecipeListView.xaml.cs**. El editor muestra el código C#, que se ejecuta cuando se presenta la página asociada en la aplicación. En este momento, solo contiene un constructor de clase:

![Contenido inicial de RecipeListView.xaml.cs](/Art/15-InitialContent.png)

1. Debajo de la llamada al método **InitializeComponent()** (y aún como parte del constructor **RecipeListView**), agrega el siguiente código que permite al usuario compartir información sobre esta receta con otra aplicación:

```csharp
		ShareButton.Clicked += async (s, a) =>
		{
			await Share.Default.RequestAsync(new ShareTextRequest
			{
				Text = "Enjoy a refreshing Raspberry smoothie.",
				Title = "Raspberry smoothie recipe"
			});
		};
```

Como referencia, así es como debería verse el archivo **RecipeListView.xaml.cs**:

![Código C# de RecipeListView.xaml.cs](/Art/16-RecipeListViewCSharpCode.png)

### Establece la vista como el elemento inicial
Finalmente, configuremos la página inicial que se mostrará cuando el usuario abra la aplicación.

1. Abre el archivo **AppShell.xaml**.

2. Agrega un nuevo espacio de nombres en la sección superior del archivo. Utiliza el identificador **views** e incluye una referencia a la ruta/espacio de nombres de **Views**.
  
```xaml
<Shell
    ...
    xmlns:local="clr-namespace:RefreshingRecipes"
    xmlns:views="clr-namespace:RefreshingRecipes.Views"
    Shell.FlyoutBehavior="Disabled">
```

3. Edita el elemento **ShellContent** para que haga referencia a la vista **RecipeListView** que creamos con anterioridad.

```xaml
    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate views:RecipeListView}"
        Route="Recipes" />
```

4. Guarda el archivo.

### Ejecuta la aplicación
(Instrucciones tomadas de este [enlace](https://github.com/dotnet-presentations/dotnet-maui-workshop/blob/main/Part%201%20-%20Displaying%20Data/README.md#run-the-app))
Asegúrate de tener la configuración correcta en tu equipo para poder implementar y depurar en las diferentes plataformas:

* [Configuración del emulador de Android](https://docs.microsoft.com/dotnet/maui/android/emulator/device-manager)
* [Configuración de Windows para desarrollo](https://docs.microsoft.com/dotnet/maui/windows/setup)

1. En Visual Studio, configura Android o Windows app como el proyecto de inicio: selecciona del menú desplegable en el menú de depuración y cambia el 'Framework'

![Visual Studio debug dropdown showing multiple frameworks](/Art/17-SelectFramework.png)

2. En Visual Studio, haz click en el botón "Depurar" o selecciona Herramientas -> Iniciar Depuración
    - Si tienes algún problema, consulta las guías de configuración para la plataforma seleccionada


Ejecutar la aplicación dará como resultado una vista que muestra información sobre una bebida:

Android:

![Aplicación ejecutándose en Android](/Art/18-AppRunning.png)

Windows:

![Aplicación ejecutándose en Windows](/Art/19-AppRunningWindows.png)

¡Felicidades! ¡Has terminado la Parte 1! Continuemos y aprendamos a organizar los elementos de la interfaz de usuario en la [Parte 2](/Part2-UIDesign/README-es.md).
