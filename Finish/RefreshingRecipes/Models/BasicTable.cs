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
