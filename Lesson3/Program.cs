using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;


namespace Lesson3
{

    internal class Program
    {
        static string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

        

        static async Task Main(string[] args)
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionString);

            Exercice1 exercice1 = new Exercice1(serviceClient);
            await exercice1.Run();

            Exercice2 exercice2 = new Exercice2(serviceClient); 
            await exercice2.Run();

            Exercice3 exercice3 = new Exercice3(serviceClient);
            await exercice3.Run();
        }
    }
}
