# Part 1 - Getting Started
Let's start by getting a basic overview of .NET MAUI and how projects are structured.

### Create a new Solution in Visual Studio
1. Open **Visual Studio 2022** and click on the **Create a new project** option.

![Creating a new project in Visual Studio 2022](/Art/01-OpenVS.png)

2. Select the **.NET MAUI App** project template. You can search for "maui" to easily find it.

![Selecting a .NET MAUI App template](/Art/02-SelectNetMauiTemplate.png)

3. Let's configure the project and click on **Next** afterwards:
* Project name: **RefreshingRecipes**
* Project location: It is recommended to use a short path

![Configuring a .NET MAUI project](/Art/03-ConfigureProject.png)

4. Select the **.NET 7.0** Framework version and click on **Create**.

![Selecting the .NET framework version](/Art/04-SelectNetFrameworkVersion.png)

After a few seconds, a solution containing one project should be created. The main **RefreshingRecipes** .NET MAUI project that targets Android, iOS, macOS, and Windows. It includes all scaffolding for the app including Models, Views, ViewModels, and Services.

![Solution for the refreshing recipes app with multiple folders](/Art/05-SolutionStructure.png)

The **RefreshingRecipes** project also has blank code files and XAML pages that we will use during the workshop. 

Refer to this [link](https://github.com/dotnet-presentations/dotnet-maui-workshop/blob/63c011c7dc29d50d063000714f6ee3a626a3e840/Part%200%20-%20Overview/README.md#understanding-the-net-maui-single-project) for a wonderful explanation of a .NET MAUI Single project structure. Each folder (Resources, Fonts, Images, ...) is explained in detail, and the startup file (MauiProgram.cs) is covered thoroughly.

### Add a new view
Now we are going to create a new view that will be modified in order to display information for our own application.

1. In the **Solution Explorer**, right-click on the project name and choose the **Add > New folder** option, then name it **Views**.

![Adding a new folder](/Art/06-AddNewFolder.png)

2. Right-click on the **Views** folder and choose the **Add > New item** option.

![Adding a new item](/Art/07-AddNewItem.png)

3. Select the **.NET MAUI** category and click on the **.NET MAUI ContentPage (XAML)** template. Name it **RecipeListView.xaml**.

![Adding a new content page](/Art/08-AddNewContentPage.png)

You can observe that two new elements were added to the **Views** folder: a **XAML** file and a **C#** file.

![Views folder content](/Art/09-ViewsFolderContent.png)

### Add information to the view
We are going to display a basic view of a refreshing beverage that includes a picture, its name and a button that will be used later to share information about the beverage.

1. Before that, let's add a picture to our project. From the [Resources](/Resources/), folder in this repository, download the [smoothie.png](/Resources/smoothie.png) image.

![A refreshing smoothie](/Resources/smoothie.png)

2. In the Solution Explorer, open the **Resources** folder and right-click on the Images sub-folder, choose the **Add > Existing item** option:

![Add existing item](/Art/10-AddExistingItem.png)

3. Select the **smoothie.png** file that you downloaded. You might need to select the **All Files (*.*)** option to be able to find the image. Click on the **Add** button.

![Add image](/Art/11-AddImage.png)

The picture has been added to our project.

![New resource added to the project](/Art/12-NewResource.png)

4. Double-click the **RecipeListView.xaml** file to open it. The editor displays XAML code, which represents the UI that is displayed when the app runs. 

![Initial content of RecipeListView.xaml](/Art/13-InitialContent.png)

3. Replace the **VerticalStackLayout** element code with the following content:

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

What was added in the code?

* The **VerticalStackLayout** element organizes child views in a one-dimensional stack vertically. It has a margin of 10 units and the amount of space between each child view is 10 units as well.

* An **Image** view displays an image file as referenced by the **Source** element, with the specific size (width and height). **AspectFill** clips the image so that it fills the display area while preserving the aspect ratio.

* A **Label** view displays a text with the specified content, font size, and position.

* A **Button** view shows a button with the designated width and content. It also includes an identifier for later use in the associated C# code.

For reference, this is how the **RecipeListView.xaml** file should look like:

![Recipe List View XAML Code](/Art/14-RecipeListViewXAMLCode.png)

### Add functionality to the view
Now, let's add some functionality to this page. Open the **RecipeListView.xaml.cs** file. The editor displays C# code, which is executed when the associated page is displayed. Right now, it only contains a class constructor:

![Initial content of RecipeListView.xaml.cs](/Art/15-InitialContent.png)

1. Below the **InitializeComponent()** call (and still as part of the **RecipeListView** constructor), add the following code that allows the user to share information about this recipe to another application:

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

For reference, this is how the **RecipeListView.xaml.cs** file should look like:

![Recipe List View C# Code](/Art/16-RecipeListViewCSharpCode.png)

### Set the view as the initial element
Finally, let's set the initial page that will be displayed when the app is opened by the user. 

1. Open the **AppShell.xaml** file.

2. Add a new namespace on the top section of the file. Use the **views** identifier and include a reference to the **Views** path/namespace.

```xaml
<Shell
    ...
    xmlns:local="clr-namespace:RefreshingRecipes"
    xmlns:views="clr-namespace:RefreshingRecipes.Views"
    Shell.FlyoutBehavior="Disabled">
```

3. Edit the **ShellContent** element to reference the **RecipeListView** view that we created earlier.

```xaml
    <ShellContent
        Title="Home"
        ContentTemplate="{DataTemplate views:RecipeListView}"
        Route="Recipes" />
```

4. Save the file.

### Run the App
(Instructions taken from this [link](https://github.com/dotnet-presentations/dotnet-maui-workshop/blob/main/Part%201%20-%20Displaying%20Data/README.md#run-the-app))
Ensure that you have your machine setup to deploy and debug to the different platforms:

* [Android Emulator Setup](https://docs.microsoft.com/dotnet/maui/android/emulator/device-manager)
* [Windows setup for development](https://docs.microsoft.com/dotnet/maui/windows/setup)

1. In Visual Studio, set the Android or Windows app as the startup project by selecting the drop down in the debug menu and changing the `Framework`

![Visual Studio debug dropdown showing multiple frameworks](/Art/17-SelectFramework.png)

2. In Visual Studio, click the "Debug" button or Tools -> Start Debugging
    - If you are having any trouble, see the Setup guides for your runtime platform

Running the app will result in a view that displays information about a beverage:

![App running on Android](/Art/18-AppRunning.png)

Congratulations! You have finished Part 1! Let's continue and learn about arranging UI elements in [Part 2](/Part2-UIDesign/README.md)