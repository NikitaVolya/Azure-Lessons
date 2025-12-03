
using PracticeWorkCosmosDB.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PracticeWorkCosmosDB.Cosmo
{
    public class CafeDB : CosmoDB<MenuItem>
    {
        public CafeDB(string endpointUrl, string primaryKey) : base(endpointUrl, primaryKey)
        {
            _databaseId = "CafeDB";
            _containerId = "MenuItems";
        }

        public async Task<List<MenuItem>> FindByMinimumPriceAsync(float price, MenuCategory? category = null)
        {

            string query = $"SELECT * FROM c WHERE c.Price >= {price}";
            if (category.HasValue)
            {
                query += $" AND c.Category = {category.GetHashCode()}";
            }
            var results = await QueryItemAsync(query);
            return results;
        }

        protected override string GetPartitionKey(MenuItem item)
        {
            return item.itemId;
        }
    }
}
