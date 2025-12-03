using CosmoDB.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CosmoDB
{
    public class Example
    {
        private string _primaryKey;
        private string _endpointUrl;

        private Database _database;
        private CosmosClient _cosmosClient;
        private Container _container;

        private string _databaseId = "FamilyDatabase";
        private string _containerId = "FamilyContainer";

        public Example(string endpointUrl, string primaryKey)
        {
            _endpointUrl = endpointUrl;
            _primaryKey = primaryKey;

            _cosmosClient = new CosmosClient(_endpointUrl, _primaryKey);
        }

        private async Task CreateDatabaseAsync()
        {
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
            Console.WriteLine("Created Database: {0}\n", _database.Id);
        }

        private async Task CreateContainerAsync()
        {
            _container = await _database.CreateContainerIfNotExistsAsync(_containerId, "/LastName");
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        }

        private async Task AddItemToContainerAsync(Family family)
        {
            try
            {
                ItemResponse<Family> familyResponse = await _container.CreateItemAsync<Family>(family, new PartitionKey(family.LastName));
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", familyResponse.Resource.Id, familyResponse.RequestCharge);
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("Cosmos DB Error: {0}", ex);
            }
        }

        private async Task QueryItemAsync(string lastname)
        {
            string sqlQueryText = $"SELECT * FROM c WHERE c.LastName = '{lastname}'";
            Console.WriteLine("Running query...");

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Family> queryResultSetIterator = _container.GetItemQueryIterator<Family>(queryDefinition);
            List<Family> families = new List<Family>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Family family in currentResultSet)
                {
                    families.Add(family);
                    Console.WriteLine("\t{0}\n", family);
                }
            }
        }

        private async Task DeleteDatabaseAndCleanupAsync()
        {
            await _database.DeleteAsync();
            Console.WriteLine("Deleted Database: {0}\n", _databaseId);

            _cosmosClient.Dispose();
        }

        public async Task GetStarted()
        {
            await CreateDatabaseAsync();
            await CreateContainerAsync();

            Family family = new Family
            {
                Id = Guid.NewGuid().ToString(),
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new Parent { FirstName = "Thomas" , FamilyName = "Andersen" },
                    new Parent { FirstName = "Mary Kay" , FamilyName = "Andersen" }
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        FamilyName = "Andersen",
                        Grade = 5,
                        Age = 10,
                        Pets = new Pet[]
                        {
                            new Pet { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address
                {
                    Line1 = "1234 Main St.",
                    City = "Seattle",
                    County = "WA"
                }
            };

            await AddItemToContainerAsync(family);

            await QueryItemAsync("Andersen");

            await DeleteDatabaseAndCleanupAsync();
        }
    }
}
