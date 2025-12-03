using CosmoDB.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CosmoDB
{
    public class FriendsDB
    {
        private string _primaryKey;
        private string _endpointUrl;

        private Database _database;
        private CosmosClient _cosmosClient;
        private Container _container;

        public string DatabaseId => "FriendsDb";
        public string ContainerId => "Contacts";


        public FriendsDB(string endpointUrl, string primaryKey)
        {
            _endpointUrl = endpointUrl;
            _primaryKey = primaryKey;
            _cosmosClient = new CosmosClient(_endpointUrl, _primaryKey);
        }

        private async Task CreateDatabaseAsync()
        {
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
            Console.WriteLine("Created Database: {0}\n", _database.Id);
        }

        private async Task CreateContainerAsync()
        {
            _container = await _database.CreateContainerIfNotExistsAsync(ContainerId, "/id");
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        }

        private async Task AddItemToContainerAsync(Contact contact)
        {
            try
            {
                ItemResponse<Contact> contanctResponse = await _container.CreateItemAsync<Contact>(contact, new PartitionKey(contact.ContactId));
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", contanctResponse.Resource.ContactId, contanctResponse.RequestCharge);
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("Cosmos DB Error: {0}", ex);
            }
        }

        private async Task QueryItemByEmailAsync(string email)
        {
            string sqlQueryText = $"SELECT * FROM c WHERE c.Email LIKE '%{email}'";
            Console.WriteLine("Running query...");

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Contact> queryResultSetIterator = _container.GetItemQueryIterator<Contact>(queryDefinition);
            List<Contact> contacts = new List<Contact>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Contact> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Contact contact in currentResultSet)
                {
                    contacts.Add(contact);
                    Console.WriteLine("\t{0}\n", contact);
                }
            }
        }

        private async Task DeleteDatabaseAndCleanupAsync()
        {
            await _database.DeleteAsync();
            Console.WriteLine("Deleted Database: {0}\n", DatabaseId);

            _cosmosClient.Dispose();
        }

        public async Task GetStarted()
        {
            await CreateDatabaseAsync();
            await CreateContainerAsync();
            Contact contact = new Contact
            {
                ContactId = "1",
                Name = "John Doe",
                Email = "test@gmail.com",
                Phone = "123-456-7890"
            };
            await AddItemToContainerAsync(contact);
            await QueryItemByEmailAsync("gmail.com");
            await DeleteDatabaseAndCleanupAsync();
        }
    }
}
