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
