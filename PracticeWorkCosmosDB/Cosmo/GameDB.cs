

using PracticeWorkCosmosDB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PracticeWorkCosmosDB.Cosmo
{
    public class GameDB : CosmoDB<Game>
    {
        public GameDB(string endpointUrl, string primaryKey) : base(endpointUrl, primaryKey)
        {
            _databaseId = "GameDB";
            _containerId = "Games";
        }

        protected override string GetPartitionKey(Game item)
        {
            return item.GameId;
        }

        public async Task<List<Game>> FindByGenreAsync(string genre, int? minimum_year = null)
        {
            string query = $"SELECT * FROM c WHERE c.Genre = '{genre}'";
            if (minimum_year.HasValue)
            {
                query += $" AND c.ReleaseYear >= {minimum_year.Value}";
            }
            return await QueryItemAsync(query);
        }
    }
}
