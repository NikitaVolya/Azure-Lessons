using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PracticeWorkCosmosDB.Cosmo
{
    public class CosmoDB<T> : IDisposable
    {
        private string _primaryKey;
        private string _endpointUrl;

        private Database _database;
        private CosmosClient _cosmosClient;
        private Container _container;

        protected string _databaseId = null;
        protected string _containerId = null;

        public string DatabaseId => _databaseId;
        public string ContainerId => _containerId;


        public CosmoDB(string endpointUrl, string primaryKey)
        {
            _endpointUrl = endpointUrl;
            _primaryKey = primaryKey;

            _cosmosClient = new CosmosClient(_endpointUrl, _primaryKey);

        }

        public void Dispose()
        {
            if (_database != null)
            {
                DeleteDatabaseAndCleanupAsync().Wait();
            }
        }

        protected virtual string GetPartitionKey(T item)
        {
            return null;
        }

        public async Task InitializeAsync()
        {
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
            _container = await _database.CreateContainerIfNotExistsAsync(_containerId, "/id");
        }

        public async Task<ItemResponse<T>> AddItemToContainerAsync(T item)
        {
            if (_container == null)
            {
                throw new InvalidOperationException("Container is not initialized. Call InitializeAsync() first.");
            }

            ItemResponse<T> response;
            try
            {
                response = await _container.CreateItemAsync<T>(item, new PartitionKey(GetPartitionKey(item)));
            }
            catch (CosmosException ex)
            {
                response = null;
                Console.WriteLine($"Cosmos DB Exception: {ex.StatusCode} - {ex.Message}");
            }
            return response;
        }

        public async Task<List<T>> QueryItemAsync(string query)
        {
            if (_container == null) {
                throw new InvalidOperationException("Container is not initialized. Call InitializeAsync() first.");
            }

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<T> queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);

            List<T> items = new List<T>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (T family in currentResultSet)
                {
                    items.Add(family);
                }
            }

            return items;
        }

        public async Task DeleteItemAsync(T item)
        {
            if (_container == null)
            {
                throw new InvalidOperationException("Container is not initialized. Call InitializeAsync() first.");
            }
            await _container.DeleteItemAsync<T>(GetPartitionKey(item), new PartitionKey(GetPartitionKey(item)));
        }

        public async Task DeleteDatabaseAndCleanupAsync()
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Database is not initialized. Call InitializeAsync() first.");
            }

            await _database.DeleteAsync();
            _cosmosClient.Dispose();

            _cosmosClient = null;
            _database = null;
            _container = null;
        }
    }
}
