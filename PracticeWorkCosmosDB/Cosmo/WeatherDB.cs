using PracticeWorkCosmosDB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PracticeWorkCosmosDB.Cosmo
{
    public class WeatherDB : CosmoDB<CityInfo>
    {
        public WeatherDB(string endpointUrl, string primaryKey) : base(endpointUrl, primaryKey)
        {
            _databaseId = "WeatherDatabase";
            _containerId = "Cities";
        }

        public async Task<List<CityInfo>> FindByLowerTemperatureAsync(double temperature)
        {
            string query = $"SELECT * FROM c WHERE c.Weather.Temperature > {temperature}";
            var results = await QueryItemAsync(query);
            return results;
        }

        protected override string GetPartitionKey(CityInfo item)
        {
            return item.CityId;
        }
    }
}
