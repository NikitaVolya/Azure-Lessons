

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ComputerVision
{

    internal class Exercice1
    {
        private ComputerVisionClient _client;
        private static readonly List<VisualFeatureTypes?> _features = new List<VisualFeatureTypes?>()
        {
            VisualFeatureTypes.Objects,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Color,
            VisualFeatureTypes.Brands,
            VisualFeatureTypes.Adult,
            VisualFeatureTypes.Description,
        };

        public Exercice1(ComputerVisionClient client)
        {
            _client = client;
        }

        public async Task AnalyzeImageByUrl(string imageUrl)
        {
            ImageAnalysis analysis = await _client.AnalyzeImageAsync(imageUrl, _features);
            PrintAnalysis(analysis);
        }

        public async Task AnalyzeImageByStream(string imagePath)
        {
            using (var imageStream = System.IO.File.OpenRead(imagePath))
            {
                var analysisByStream = await _client.AnalyzeImageInStreamAsync(imageStream, _features);
                PrintAnalysis(analysisByStream);
            }
        }   

        public void PrintAnalysis(ImageAnalysis analysis)
        {
            Console.WriteLine("Object | Probability | Coordinates | Type");
            Console.WriteLine("--------------------------------------------------");

            if (analysis.Objects != null)
            {
                foreach (var obj in analysis.Objects)
                {
                    Console.WriteLine(
                        $"{obj.ObjectProperty} | {obj.Confidence} | " +
                        $"X:{obj.Rectangle.X},Y:{obj.Rectangle.Y},W:{obj.Rectangle.W},H:{obj.Rectangle.H} | Object"
                    );
                }
            }

            if (analysis.Brands != null)
            {
                foreach (var brand in analysis.Brands)
                {
                    Console.WriteLine(
                        $"{brand.Name} | {brand.Confidence} | " +
                        $"X:{brand.Rectangle.X},Y:{brand.Rectangle.Y},W:{brand.Rectangle.W},H:{brand.Rectangle.H} | Brand"
                    );
                }
            }

            if (analysis.Faces != null)
            {
                foreach (var face in analysis.Faces)
                {
                    Console.WriteLine(
                        $"Face ({face.Gender}, {face.Age}) | 1.0 | " +
                        $"X:{face.FaceRectangle.Left},Y:{face.FaceRectangle.Top}," +
                        $"W:{face.FaceRectangle.Width},H:{face.FaceRectangle.Height} | Face"
                    );
                }
            }

            if (analysis.Description?.Tags != null)
            {
                foreach (var tag in analysis.Description.Tags)
                {
                    Console.WriteLine(
                        $"{tag} | - | - | Tag"
                    );
                }
            }

            if (analysis.Adult != null)
            {
                Console.WriteLine(
                    $"Adult content | {analysis.Adult.AdultScore} | - | Adult"
                );

                Console.WriteLine(
                    $"Racy content | {analysis.Adult.RacyScore} | - | Adult"
                );
            }
        }

        public async Task Run()
        {
            int choice;

            Console.Write("Exercice 1:\n1. analysis by href\n2. analysis by path\n>> ");
            choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Enter image URL: ");
                    string imageUrl = Console.ReadLine();

                    await AnalyzeImageByUrl(imageUrl);

                    break;
                case 2:
                    Console.WriteLine("Enter image path: ");
                    string imagePath = Console.ReadLine();
                    await AnalyzeImageByStream(imagePath);

                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }
}
