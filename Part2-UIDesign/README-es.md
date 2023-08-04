# Part 2 - Diseño de la Interfaz de Usuario
¡Mejoremos la apariencia visual de nuestra aplicación!

### Responder a los cambios de tema del sistema (modo oscuro/claro)
1. Abre **RecipeListView.xaml**. Agrega el siguiente código como parte de las propiedades de **ContentPage**:

```xaml
BackgroundColor="{AppThemeBinding Light={StaticResource Secondary}, Dark={StaticResource Primary}}"
```

Estamos especificando el **color de fondo** para la página que cambia dependiendo si el dispositivo está usando su tema claro u oscuro. `Primary` y `Secondary` se definen en `Resources/Styles/Colors.xaml`, como puedes observar en la siguiente imagen.

![Mostrando colores predefinidos en la aplicación](/Art/20-StyleColors.png)

2. Para el `Label`, vamos a hacerla más grande, alinearla a la izquierda, ponerla en negrita y definirle un color de texto que también cambie según la preferencia de tema del usuario. Reemplaza la definición anterior del `Label` con este nuevo código:

```xaml
<Label Text="Raspberry smoothie"
        FontSize="Large"
        HorizontalOptions="Start"
        FontAttributes="Bold"
        TextColor="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource Secondary}}"
        />
```
3. ¿Qué pasa con el botón? Respetaremos el tema oscuro/claro seleccionado para los colores de fondo y texto, por lo que este es el nuevo código (esta vez elegimos una definición más simple):

```xaml
<Button Text="Share"
        WidthRequest="400"
        TextColor="{AppThemeBinding Light=White, Dark=Black}"
        BackgroundColor="{AppThemeBinding Light=Black, Dark=White}"
        x:Name="ShareButton"/>
```

4. ¡Compila y ejecuta la aplicación! Primero, veamos nuestra aplicación corriendo en Android:

![Aplicación ejecutándose en modo claro](/Art/21-AppRunningInLightMode.png)

Ahora, habilita el modo oscuro en tu dispositivo. Por ejemplo, a continuación se muestra cómo configurarlo en un emulador de Android.

![Habilitando el modo oscuro](/Art/22-EnablingDarkMode.png)

Vuelve a tu aplicación y...

![Aplicación ejecutándose en modo oscuro](/Art/23-AppRunningInDarkMode.png)

... ¡la magia acaba de suceder! Puedes ver como el color de fondo de la página es diferente entre para cada tema, y lo mismo ocurre tanto con el color del texto del Label como con los colores del botón (texto y fondo).

**Importante:** En lugar de especificar una propiedad dinámica para cada View en tu página, define un `Estilo` y aplícalo (esto maximiza la reutilización del código). Obtén más información sobre la clase [Style] (https://learn.microsoft.com/es-mx/dotnet/maui/user-interface/styles/xaml) y [App theme binding] (https://learn.microsoft.com/es-mx/dotnet/maui/user-interface/system-theme-changes).

### Esquinas redondeadas en la imagen

1. Puedes redondear las esquinas de la imagen envolviendo el control `Image` en un `Frame`. Agregamos este control y movemos la definición de la imagen adentro de la siguiente manera:

```xaml
<Frame Padding="0"
        CornerRadius="18"
        HasShadow="False"
        IsClippedToBounds="True">
    <Image WidthRequest="600"
            HeightRequest="400"
            Source="smoothie.png"
            Aspect="AspectFill"/>
</Frame>
```

2. Vuelve a ejecutar la aplicación y verás que las esquinas de la imagen se han redondeado:

![Imagen con esquinas redondeadas](/Art/24-RoundImageCorners.png)

### Agrega fuentes personalizadas
Finalmente, ¡agreguemos un conjunto popular de íconos a nuestra aplicación!

1. Descargue el set gratuito y de código abierto [Material Design Icons](https://pictogrammers.com/library/mdi/) ejecutando el comando `npm install`, tal como se muestra en la imagen:

![Descargando Material Design Icons](/Art/25-DownloadMDIFont.png)

2. Una vez descargado, encontrará el archivo **materialdesignicons-webfont.ttf** en la carpeta Fonts.

![Localizando el archivo de fuentes MDI](/Art/26-MDIFontFile.png)

3. A continuación, debes generar una clase que contenga todos los glifos para que puedas hacer referencia a los iconos desde C#. Visita el sitio web **[Icon Font to #Code](https://andreinitescu.github.io/IconFont2Code/)**.

![Visitando el sitio web Icon Font to C# Code](/Art/27-IconFontToCSharpCode.png)

4. Ahora, carga el archivo de fuentes y verás que se muestra un código auxiliar de una clase de C#. **Cópiala en el portapapeles**.

![Generando la clase auxiliar de C# a partir del archivo de fuente](/Art/28-MDIHelperClassCode.png)

5. Crea una nueva carpeta (`Helpers`) y un archivo C# (`IconFont.cs`) en tu proyecto. Reemplaza el código predeterminado con el código generado a partir de la herramienta (manteniendo el espacio de nombres predeterminado del proyecto).

![Código de la clase auxiliar de C# IconFont](/Art/29-IconFontClassCode.png)

6. Ve a la carpeta `Fonts` (en la carpeta `Resources`), luego agrega el archivo existente `materialdesignicons-webfont.ttf` y asegúrate de que `Build Action` está configurado en `MauiFont`.

![Agregando el archivo de fuente personalizado](/Art/30-AddFontFile.png)

7. Dado que necesitas registrar la fuente en tu clase **MauiProgram.cs**, agrega la siguiente línea en la extensión `ConfigureFonts`, la cual hace referencia al archivo de fuente y establece un alias que podemos usar en el código:

```csharp
fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
```

8. Vuelve al archivo `RefreshingRecipes.xaml` y agrega la referencia del espacio de nombres `Helpers` en la definición superior de `ContentPage`:

```xaml
xmlns:helpers="clr-namespace:RefreshingRecipes.Helpers"
```

9. Entonces, ¡estamos listos para agregar íconos a nuestra aplicación! Usemos uno para el botón que comparte la receta. Para ello, la propiedad `FontFamily` debe hacer referencia al alias que se definió previamente en `MauiProgram.cs`, el cual es `MaterialDesignIcons`.

Además, el botón se mostrará como un botón circular.

Este es el nuevo código para el botón:

```xaml
<Button FontFamily="MaterialDesignIcons" 
        Text="{x:Static helpers:IconFont.ShareVariant}"
        FontSize="25"
        WidthRequest="50"
        HeightRequest="50"
        CornerRadius="25"
        TextColor="{AppThemeBinding Light=Black, Dark=White}"
        BackgroundColor="{AppThemeBinding Light=White, Dark=Black}"
        x:Name="ShareButton"/>
```

Obtén más información sobre [Agregando fuentes personalizadas en .NET MAUI](https://cedricgabrang.medium.com/custom-fonts-material-design-icons-in-net-maui-acf59c9f98fe) y [Creando botones circulares en XAML]( https://askxammy.com/sencilla-manera-de-crear-botones-circulares-en-formularios-xamarin/).

10. ¡Probemos esta implementación! Ejecuta la aplicación:

![Botón con ícono](/Art/31-IconButton.png)

¡Felicidades! ¡Has terminado la Parte 2! Aprendamos ahora sobre el patrón MVVM en la [Parte 3](/Part3-MVVM/README.md).

### Contribuciones de la comunidad: mejorando la interfaz de usuario

Gracias a [Bryan Oroxon](https://github.com/BryanOroxon/) por la siguiente implementación.

1. Agrega una nueva definición de Color en `Resources/Styles/Colors.xaml`:

```xaml
    <Color x:Key="Blue500">#3b65ff</Color>
```

2. Echa un vistazo a la nueva y mejorada versión de código de [`RecipeListView.xaml`](/Part3-MVVM/RefreshingRecipes/Views/RecipeListView.xaml), donde se implementa `AppThemeBinding` para diferentes controles. Además, el diseño cambia un poco para mostrar el nombre de la bebida refrescante sobre la imagen.

3. Cuando ejecutas la aplicación implementando el código anterior (puedes copiar y pegar), así es como debería verse (cuando el modo oscuro está habilitado):

![Interfaz de usuario mejorada](/Art/32-ImprovingUI.png)