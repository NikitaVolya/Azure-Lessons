using System;
using System.Threading.Tasks;

namespace CosmoDB
{
    internal class Program
    {
        static string primaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
        static string endpointUrl = Environment.GetEnvironmentVariable("EndpointUrl");


        static async Task Main(string[] args)
        {
            Console.WriteLine("Primary Key: {0} EndpointURL: {1}", primaryKey, endpointUrl);

            Example example = new Example(endpointUrl, primaryKey);
            await example.GetStarted();

            FriendsDB friendsDB = new FriendsDB(endpointUrl, primaryKey);
            await friendsDB.GetStarted();

            CinemaDB cinemaDB = new CinemaDB(endpointUrl, primaryKey);
            await cinemaDB.GetStarted();
        }
    }
}
