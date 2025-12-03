using PracticeWorkCosmosDB.Models;
using PracticeWorkCosmosDB.Cosmo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PracticeWorkCosmosDB
{
    internal class Program
    {
        static string primaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
        static string endpointUrl = Environment.GetEnvironmentVariable("EndpointUrl");

        static async Task Exercice1()
        {
            WeatherDB weatherDB = new WeatherDB(endpointUrl, primaryKey);
            await weatherDB.InitializeAsync();

            CityInfo cityInfo = new CityInfo
            {
                CityId = "1",
                CityName = "New York",
                Weather = new WeatherData
                {
                    Temperature = 25.3,
                    Wind = 5.4,
                    Humidity = 60.2,
                    ObservationTime = DateTime.UtcNow
                }
            };
            var response = await weatherDB.AddItemToContainerAsync(cityInfo);
            Console.WriteLine($"Item added with id: {response.Resource.CityId}");

            response = await weatherDB.AddItemToContainerAsync(new CityInfo
            {
                CityId = "2",
                CityName = "Los Angeles",
                Weather = new WeatherData
                {
                    Temperature = 24.5,
                    Wind = 3.2,
                    Humidity = 50.5,
                    ObservationTime = DateTime.UtcNow
                }
            });
            Console.WriteLine($"Item added with id: {response.Resource.CityId}");

            List<CityInfo> queriedItems = await weatherDB.FindByLowerTemperatureAsync(25.0);

            foreach (var item in queriedItems)
            {
                Console.WriteLine(item.ToString());
            }

            await weatherDB.DeleteDatabaseAndCleanupAsync();
        }

        static async Task Exercice2()
        {
            CafeDB cafeDB = new CafeDB(endpointUrl, primaryKey);
            await cafeDB.InitializeAsync();

            await cafeDB.AddItemToContainerAsync(new MenuItem
            {
                itemId = "1",
                Name = "Cappuccino",
                Category = MenuCategory.Appetizer,
                Price = 3.5
            });
            await cafeDB.AddItemToContainerAsync(new MenuItem
            {
                itemId = "2",
                Name = "Cheesecake",
                Category = MenuCategory.Dessert,
                Price = 4.0
            });


            List<MenuItem> queriedItems = await cafeDB.FindByMinimumPriceAsync(3.0f, MenuCategory.Appetizer);

            foreach (var item in queriedItems)
            {
                Console.WriteLine(item.ToString());
            }


            await cafeDB.DeleteDatabaseAndCleanupAsync();
        }

        static async Task Exercice3()
        {
            GameDB gameDB = new GameDB(endpointUrl, primaryKey);
            await gameDB.InitializeAsync();

            await gameDB.AddItemToContainerAsync(new Game
            {
                GameId = "1",
                Title = "The Legend of Zelda",
                Genre = "RPG",
                ReleaseYear = 1986
            });

            await gameDB.AddItemToContainerAsync(new Game
            {
                GameId = "2",
                Title = "Super Mario Bros.",
                Genre = "Platformer",
                ReleaseYear = 1985
            });

            await gameDB.AddItemToContainerAsync(new Game
            {
                GameId = "3",
                Title = "Final Fantasy VII",
                Genre = "RPG",
                ReleaseYear = 1997
            });

            List<Game> queriedItems = await gameDB.FindByGenreAsync("RPG", 1980);
            queriedItems.ForEach(game => Console.WriteLine(game.ToString()));
            queriedItems.ForEach(async game => await gameDB.DeleteItemAsync(game));


            Console.WriteLine("After deletion:");
            queriedItems = await gameDB.QueryItemAsync("SELECT * FROM c");
            queriedItems.ForEach(game => Console.WriteLine(game.ToString()));

            await gameDB.DeleteDatabaseAndCleanupAsync();
        }

        static async Task Main(string[] args)
        {
            await Exercice1();
            await Exercice2();
            await Exercice3();
        }
    }
}
