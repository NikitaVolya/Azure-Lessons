

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComputerVision
{
    internal class Program
    {
        static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_KEY");
        static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");



        static async Task Main(string[] args)
        {
            string imageUrl = "https://worldofgeek.fr/wp-content/uploads/2025/11/five-night-at-freddys-2-1.jpg";

            ComputerVisionClient client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(subscriptionKey))
                {
                    Endpoint = endpoint
                };

            Exercice1 exercice1 = new Exercice1(client);
            await exercice1.Run();

            Exercice2 exercice2 = new Exercice2(client);
            await exercice2.Run();
        }
    }
}
