using CosmoDB.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CosmoDB
{
    internal class CinemaDB
    {
        private string _primaryKey;
        private string _endpointUrl;

        private Database _database;
        private CosmosClient _cosmosClient;
        private Container _container;

        public string DatabaseId => "CinemaDB";
        public string ContainerId => "Movies";


        public CinemaDB(string endpointUrl, string primaryKey)
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

        private async Task AddItemToContainerAsync(Movie movie)
        {
            try
            {
                ItemResponse<Movie> contanctResponse = await _container.CreateItemAsync<Movie>(movie, new PartitionKey(movie.MovieId));
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", contanctResponse.Resource.MovieId, contanctResponse.RequestCharge);
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("Cosmos DB Error: {0}", ex);
            }
        }

        private async Task QueryItemByGenreAsync(MovieGenre genre)
        {
            string sqlQueryText = $"SELECT * FROM c WHERE c.Genre = {genre.GetHashCode()}";
            Console.WriteLine("Running query...");

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Movie> queryResultSetIterator = _container.GetItemQueryIterator<Movie>(queryDefinition);
            List<Movie> contacts = new List<Movie>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Movie> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Movie contact in currentResultSet)
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

            Movie movie = new Movie()
            {
                MovieId = "1",
                Title = "Inception",
                Description = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.",
                Year = 2010,
                Genre = MovieGenre.Comedy
            };

            await AddItemToContainerAsync(movie);
            await QueryItemByGenreAsync(MovieGenre.Comedy);
            await DeleteDatabaseAndCleanupAsync();
        }
    }
}
