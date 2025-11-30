using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lesson3
{
    internal class Exercice3
    {
        public class TaskMessage
        {
            public string task { get; set; }
            public int attempt { get; set; }
        }

        private QueueServiceClient _serviceClient;

        private QueueClient _mainQueue;
        private QueueClient _deadQueue;

        public Exercice3(QueueServiceClient serviceClient)
        {
            _serviceClient = serviceClient;

            _mainQueue = serviceClient.CreateQueue("main-queue");
            _deadQueue = serviceClient.CreateQueue("dead-letter-queue");
        }

        public async Task TestInput()
        {

            var msg = new TaskMessage
            {
                task = "Generate PDF report",
                attempt = 0
            };

            string json = JsonSerializer.Serialize(msg);

            await _mainQueue.SendMessageAsync(json);
        }

        public async Task Run()
        {
            await TestInput();

            Console.WriteLine("Processor started...");

            while (true)
            {
                QueueMessage message = (await _mainQueue.ReceiveMessagesAsync(1)).Value.FirstOrDefault();

                if (message == null)
                {
                    await Task.Delay(1000);
                    continue;
                }

                Console.WriteLine($"\nReceived message: {message.MessageText}");

                var data = JsonSerializer.Deserialize<TaskMessage>(message.MessageText);
                data.attempt++;

                bool isError = new Random().NextDouble() < 0.5;

                if (isError)
                {
                    Console.WriteLine($"Error occurred! Attempt {data.attempt}");

                    if (data.attempt < 3)
                    {
                        string updatedJson = JsonSerializer.Serialize(data);

                        await _mainQueue.UpdateMessageAsync(
                            message.MessageId,
                            message.PopReceipt,
                            updatedJson,
                            visibilityTimeout: TimeSpan.FromSeconds(2)
                        );

                        Console.WriteLine("Message returned for retry");
                    }
                    else
                    {
                        string deadJson = JsonSerializer.Serialize(data);

                        await _deadQueue.SendMessageAsync(deadJson);

                        await _mainQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                        Console.WriteLine("Moved to DEAD-LETTER queue");
                    }
                }
                else
                {
                    Console.WriteLine($"Successfully processed after attempt {data.attempt}");

                    await _mainQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }
            }
        }
    }
}
