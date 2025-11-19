using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Lessons
{
    internal class Program
    {
        private static string localPath = "./data/";

        static async Task Exercice1(BlobContainerClient containerClient)
        {
            string fileName = "test.txt";
            string localFilePath = Path.Combine(localPath, fileName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            FileStream uplodaFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uplodaFileStream, overwrite: true);
            uplodaFileStream.Close();
        }

        static async Task Exercice2(BlobContainerClient containerClient)
        {
            string secondFileName = "second.txt";
            string localFilePath = Path.Combine(localPath, secondFileName);
            string blockName;

            BlobClient blobClient = containerClient.GetBlobClient(secondFileName);
            FileStream uplodaFileStream = File.OpenRead(localFilePath);

            await blobClient.UploadAsync(uplodaFileStream, overwrite: true);
            uplodaFileStream.Close();

            Console.WriteLine("Chose one of files:");
            foreach (BlobItem block in containerClient.GetBlobs())
            {
                Console.Write("{0}, ", block.Name);
            }
            Console.WriteLine();

            blockName = Console.ReadLine();

            BlobClient clien = containerClient.GetBlobClient(blockName);
            BlobDownloadInfo download = await clien.DownloadAsync();

            localFilePath = localFilePath.Replace(".", "DOWNLAOD.");
            FileStream downloadFileStream = File.OpenWrite(localFilePath);
            await download.Content.CopyToAsync(downloadFileStream);
            downloadFileStream.Close();
        }

        static async Task Exercice3(BlobContainerClient containerClient)
        {
            string localFilePath, virtualFilePath;

            string virtualPath = Path.Combine(
                    DateTime.Now.Year.ToString(),
                    DateTime.Now.Month.ToString(),
                    DateTime.Now.DayOfWeek.ToString());
            string[] pictures = { "picture1.jpg", "picture2.jpg", "picture3.jpg", "picture4.jpg" };

            foreach (string fileName in pictures)
            {
                localFilePath = Path.Combine(localPath, fileName);
                virtualFilePath = Path.Combine(virtualPath, fileName);

                BlobClient blobClient = containerClient.GetBlobClient(virtualFilePath);

                FileStream uplodaFileStream = File.OpenRead(localFilePath);
                await blobClient.UploadAsync(uplodaFileStream, overwrite: true);
                uplodaFileStream.Close();

                BlobSasBuilder sas = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    BlobName = virtualFilePath,
                    Resource = "b",
                    ExpiresOn = DateTime.UtcNow.AddHours(1)
                };
                sas.SetPermissions(BlobSasPermissions.Read);

                Uri sasUri =containerClient.GetBlobClient(virtualFilePath).GenerateSasUri(sas);

                Console.WriteLine(sasUri);
            }
        }

        static async Task Main(string[] args)
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            string containerName = "testcontainer";

            BlobServiceClient blobService = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

            await Exercice1(containerClient);
            await Exercice2(containerClient);


            BlobContainerClient picturesContainerClient = new BlobContainerClient(connectionString, "pictures");
            await picturesContainerClient.CreateIfNotExistsAsync();

            await Exercice3(picturesContainerClient);
        }
    }
}
