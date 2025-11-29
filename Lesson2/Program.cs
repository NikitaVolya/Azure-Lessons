using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lesson2
{
    internal class Program
    {
        static string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        static string localLogPath = "./logs/";



        static async Task Exercise1()
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionString);
            QueueClient queue = await serviceClient.CreateQueueAsync("orders-queue");

            await queue.CreateIfNotExistsAsync();

            while (true)
            {
                Console.WriteLine("\n--- Orders Menu ---");
                Console.WriteLine("1. Add order");
                Console.WriteLine("2. Peek first 10 orders");
                Console.WriteLine("3. Receive & process order");
                Console.WriteLine("4. Delete entire queue");
                Console.WriteLine("0. Exit to main");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                if (choice == "0") break;

                switch (choice)
                {
                    case "1":
                        await AddOrder(queue);
                        break;
                    case "2":
                        await PeekOrders(queue);
                        break;
                    case "3":
                        await ProcessOrder(queue);
                        break;
                    case "4":
                        await queue.DeleteAsync();
                        Console.WriteLine("Queue deleted!");
                        break;
                }
            }
        }

        static async Task AddOrder(QueueClient queue)
        {
            Console.Write("Customer name: ");
            string name = Console.ReadLine();

            Console.Write("Product: ");
            string product = Console.ReadLine();

            var order = new
            {
                orderId = Guid.NewGuid().ToString(),
                customer = name,
                product = product,
                createdAt = DateTime.UtcNow.ToString("s")
            };

            string messageJson = JsonSerializer.Serialize(order);

            await queue.SendMessageAsync(messageJson, timeToLive: TimeSpan.FromMinutes(10));

            Console.WriteLine("Order saved.");
        }

        static async Task PeekOrders(QueueClient queue)
        {
            PeekedMessage[] messages = (await queue.PeekMessagesAsync(10)).Value;

            if (messages.Length == 0)
            {
                Console.WriteLine("Queue is empty.");
                return;
            }

            Console.WriteLine("\n--- First 10 orders (Peek) ---");
            foreach (var msg in messages)
            {
                Console.WriteLine(msg.Body.ToString());
            }
        }

        static async Task ProcessOrder(QueueClient queue)
        {
            QueueMessage msg = (await queue.ReceiveMessagesAsync(1)).Value.FirstOrDefault();

            if (msg == null)
            {
                Console.WriteLine("Queue empty.");
                return;
            }

            Console.WriteLine("Processing...");
            await Task.Delay(3000);

            await queue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
            Console.WriteLine("Order processed & deleted.");
        }
        static async Task Exercise2()
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionString);

            QueueClient logsInfo = await serviceClient.CreateQueueAsync("logs-info");

            QueueClient logsErrors = await serviceClient.CreateQueueAsync("logs-errors");

            await logsInfo.CreateIfNotExistsAsync();
            await logsErrors.CreateIfNotExistsAsync();

            Directory.CreateDirectory(localLogPath);

            while (true)
            {
                Console.WriteLine("\n--- Logs Menu ---");
                Console.WriteLine("1. Add INFO log");
                Console.WriteLine("2. Add ERROR log");
                Console.WriteLine("3. View last 5 logs");
                Console.WriteLine("4. Receive & process logs");
                Console.WriteLine("0. Exit");
                string c = Console.ReadLine();

                if (c == "0") break;

                switch (c)
                {
                    case "1":
                        await AddLog(logsInfo, "info");
                        break;
                    case "2":
                        await AddLog(logsErrors, "error");
                        break;
                    case "3":
                        await PeekLogs(logsInfo, logsErrors);
                        break;
                    case "4":
                        await ProcessLogs(logsInfo, logsErrors);
                        break;
                }
            }
        }

        static async Task AddLog(QueueClient queue, string type)
        {
            Console.Write("Username: ");
            string user = Console.ReadLine();

            Console.Write("Message: ");
            string msg = Console.ReadLine();

            var log = new
            {
                type = type,
                message = msg,
                timestamp = DateTime.UtcNow.ToString("s"),
                username = user
            };

            string json = JsonSerializer.Serialize(log);

            if (type == "error")
                await queue.SendMessageAsync(json, timeToLive: null);
            else
                await queue.SendMessageAsync(json);

            Console.WriteLine("Log saved.");
        }

        static async Task PeekLogs(QueueClient info, QueueClient errors)
        {
            Console.WriteLine("\n--- INFO logs ---");
            foreach (var m in (await info.PeekMessagesAsync(5)).Value)
                Console.WriteLine(m.Body.ToString());

            Console.WriteLine("\n--- ERROR logs ---");
            foreach (var m in (await errors.PeekMessagesAsync(5)).Value)
                Console.WriteLine(m.Body.ToString());
        }

        static async Task ProcessLogs(QueueClient info, QueueClient errors)
        {
            await ProcessLogsFromQueue(info);
            await ProcessLogsFromQueue(errors);
        }

        static async Task ProcessLogsFromQueue(QueueClient queue)
        {
            QueueMessage[] msgs = await queue.ReceiveMessagesAsync(5, TimeSpan.FromSeconds(5));

            foreach (var msg in msgs)
            {
                string file = Path.Combine(localLogPath, "logs.txt");
                File.AppendAllText(file, msg.Body + "\n");

                await queue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);

                Console.WriteLine("Processed: " + msg.Body);
            }
        }

        static async Task Exercise3()
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionString);
            QueueClient queue = await serviceClient.CreateQueueAsync("background-tasks");

            await queue.CreateIfNotExistsAsync();

            while (true)
            {
                Console.WriteLine("\n--- Background Tasks ---");
                Console.WriteLine("1. Add task");
                Console.WriteLine("2. Process task");
                Console.WriteLine("3. Simulate error");
                Console.WriteLine("0. Exit");
                string c = Console.ReadLine();

                if (c == "0") break;

                switch (c)
                {
                    case "1":
                        await AddBackgroundTask(queue);
                        break;
                    case "2":
                        await ProcessBackgroundTask(queue, false);
                        break;
                    case "3":
                        await ProcessBackgroundTask(queue, true);
                        break;
                }
            }
        }

        static async Task AddBackgroundTask(QueueClient queue)
        {
            Console.Write("Enter task text: ");
            string text = Console.ReadLine();

            await queue.SendMessageAsync(text);

            Console.WriteLine("Task added.");
        }

        static async Task ProcessBackgroundTask(QueueClient queue, bool simulateError)
        {
            var msg = (await queue.ReceiveMessagesAsync(1, TimeSpan.FromSeconds(3))).Value.FirstOrDefault();

            if (msg == null)
            {
                Console.WriteLine("No tasks.");
                return;
            }

            Console.WriteLine($"Task received. DequeueCount = {msg.DequeueCount}");
            Console.WriteLine("Processing 10 seconds...");

            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(2000);

                await queue.UpdateMessageAsync(
                    msg.MessageId,
                    msg.PopReceipt,
                    msg.Body,
                    visibilityTimeout: TimeSpan.FromSeconds(3)
                );

                Console.WriteLine("Visibility extended...");
            }

            if (simulateError)
            {
                Console.WriteLine("Simulated error! Message returned to queue.");
                return;
            }

            await queue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
            Console.WriteLine("Task finished and deleted.");
        }

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\n=== Azure Queue Exercises ===");
                Console.WriteLine("1. Exercise 1 (Orders)");
                Console.WriteLine("2. Exercise 2 (Logs)");
                Console.WriteLine("3. Exercise 3 (Background tasks)");
                Console.WriteLine("0. Exit");

                string c = Console.ReadLine();

                if (c == "0") break;

                switch (c)
                {
                    case "1": await Exercise1(); break;
                    case "2": await Exercise2(); break;
                    case "3": await Exercise3(); break;
                }
            }
        }
    }
}
