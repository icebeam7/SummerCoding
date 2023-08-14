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