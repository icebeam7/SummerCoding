namespace RefreshingRecipes.Helpers
{
    public static class Constants
    {
        public const string DatabaseFilename = "RecipesDb-v1_0.db3";
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        public const string OnlineModeKey = "online_mode";
    }
}
