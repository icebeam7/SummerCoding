# Part 2 - UI Design
Let's improve the visual appearance of our app! 

### Respond to system theme changes (dark/light mode)
1. Open **RecipeListView.xaml**. Add the following code as part of the **ContentPage** properties:

```xaml
BackgroundColor="{AppThemeBinding Light={StaticResource Secondary}, Dark={StaticResource Primary}}"
```

We are specifying the **background color** for the Page that changes based on whether the device is using its light or dark theme. `Primary` and `Secondary` are defined in `Resources/Styles/Colors.xaml`, as you can see in the next picture. 

![Displaying predefined colors in .NET MAUI App](/Art/20-StyleColors.png)

2. For the `Label` let's make it larger, align it to the left, make it bold, and define a text color that also changes based on the user's theme preference. Replace the previous `Label` definition with this new code:

```xaml
<Label Text="Raspberry smoothie"
        FontSize="Large"
        HorizontalOptions="Start"
        FontAttributes="Bold"
        TextColor="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource Secondary}}"
        />
```

3. What about the button? We will respect the dark/light selected theme for the background and text colors, so this is the new code for it (we chose a simpler definition this time):


```xaml
<Button Text="Share"
        WidthRequest="400"
        TextColor="{AppThemeBinding Light=White, Dark=Black}"
        BackgroundColor="{AppThemeBinding Light=Black, Dark=White}"
        x:Name="ShareButton"/>
```

4. Compile and run the application! First, let's see how our application looks like in Android:

![App running in light mode](/Art/21-AppRunningInLightMode.png)

Now, enable the Dark mode on your device. For example, here are the settings for an Android emulator.

![Enabling dark mode](/Art/22-EnablingDarkMode.png)

Switch back to your app and... 

![App running in dark mode](/Art/23-AppRunningInDarkMode.png)

...magic just happened! You can see how the page background color is different between light and dark themes, and the same happens with the label text color and the button colors (text and background).

**Important:** Rather than specifying a dynamic property for each View in your page, define a `Style` and apply it (this maximizes code reutilization). Read more about [Styles](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/styles/xaml) and [App theme bindings](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/system-theme-changes). 


### Round corners on the image

1. You can round the image corners by wrapping the `Image` into a `Frame`. We add this View and move the Image definition inside as follows:

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

2. Run the app again, and you will see that the corners on the picture have been rounded:

![Image with rounderd corners](/Art/24-RoundImageCorners.png)

### Add custom fonts
Finally, let's add a popular set of icons into our app! 

1. Download the free and open-source [Material Design Icons](https://pictogrammers.com/library/mdi/) set by running the `npm install` command.

![Downloading Material Design Icons](/Art/25-DownloadMDIFont.png)

2. Once downloaded, you will find the **materialdesignicons-webfont.ttf** file in the fonts folder.

![Finding the MDI Font file](/Art/26-MDIFontFile.png)

3. Next, you need to generate a class containing all the glyphs so you can reference your icons from C#. Visit the **[Icon Font to #Code](https://andreinitescu.github.io/IconFont2Code/)** website.

![Visiting Icon Font to C# Code website](/Art/27-IconFontToCSharpCode.png)

4. Now upload the font file and you will see that a C# helper class code is shown. **Copy it to the clipboard**.

![Generating C# Helper Class Code from font file](/Art/28-MDIHelperClassCode.png)

5. Create a new folder (`Helpers`) and C# file (`IconFont.cs`) in your project. Replace the default class code with the code generated from the tool (keep the default namespace from your project).

![IconFont C# Helper Class Code](/Art/29-IconFontClassCode.png)

6. Go to the `Fonts` folder (under the `Resources` folder), then add the `materialdesignicons-webfont.ttf` existing file, and make sure the `Build Action` is set to `MauiFont`.

![Adding the custom font file](/Art/30-AddFontFile.png)

7. Since you need to register the font in your **MauiProgram.cs** class, add the following line into the `ConfigureFonts` extension, which references the font file and sets an alias we can use in the code:

```csharp
fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
```

8. Back into `RefreshingRecipes.xaml` file, add the `Helpers` namespace reference at the top `ContentPage` definition:

```xaml
xmlns:helpers="clr-namespace:RefreshingRecipes.Helpers"
```

9. Then, we are ready to add icons into our app! Let's use one for the button that we use to share the recipe. To do that, the `FontFamily` property must reference the alias that was previously defined in `MauiProgram.cs`, which is `MaterialDesignIcons`.

Moreover, the button will be displayed as a circle button.

This is the new code for the button:

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

Learn more about [Adding custom fonts in .NET MAUI](https://cedricgabrang.medium.com/custom-fonts-material-design-icons-in-net-maui-acf59c9f98fe) and [Creating circle buttons in XAML](https://askxammy.com/easy-way-to-create-circle-buttons-in-xamarin-forms/)

10. Let's test this implementation! Run the app:

![Icon Button](/Art/31-IconButton.png)

Congratulations! You have finished Part 2! Let's continue and learn about the MVVM pattern in [Part 3](/Part3-MVVM/README.md).

### Community Contributions - Improving the UI

Thanks to [Bryan Oroxon](https://github.com/BryanOroxon/) for the following implementation.

1. Add a new Color definition in `Resources/Styles/Colors.xaml`:

```xaml
    <Color x:Key="Blue500">#3b65ff</Color>
```

2. Take a look at the improved [`RecipeListView.xaml`](/Part3-MVVM/RefreshingRecipes/Views/RecipeListView.xaml), where `AppThemeBinding` is implemented for different controls. Moreover, the layout changes a bit to display the `Label` over the `Image`. 

3. When you run the app, this is how it should look like (when Dark mode is enabled):

![UI improved](/Art/32-ImprovingUI.png)