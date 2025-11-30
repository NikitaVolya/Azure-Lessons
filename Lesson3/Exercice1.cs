using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;


namespace Lesson3
{
    internal class Exercice1
    {
        class Reminder
        {
            public string text { get; set; }
        }

        private QueueServiceClient _serviceClient; 

        public Exercice1(QueueServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        async Task AddReminder(QueueClient queue)
        {
            Console.Write("Enter reminder text: ");
            string text = Console.ReadLine();

            Console.Write("Deliver after minutes: ");
            int minutes = int.Parse(Console.ReadLine());

            var payload = new { text = text };

            string json = JsonSerializer.Serialize(payload);

            await queue.SendMessageAsync(
                json,
                visibilityTimeout: TimeSpan.FromMinutes(minutes),
                timeToLive: TimeSpan.FromDays(1)
            );

            Console.WriteLine("Reminder added!");
        }

        async Task ProcessReminders(QueueClient queue)
        {
            while (true)
            {
                QueueMessage[] msgs = await queue.ReceiveMessagesAsync(maxMessages: 1);

                if (msgs.Length > 0)
                {
                    var msg = msgs[0];
                    var reminder = JsonSerializer.Deserialize<Reminder>(msg.MessageText);

                    Console.WriteLine($"\n[Reminder: {reminder.text}]");

                    await queue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                }

                await Task.Delay(10_000);
            }
        }

        public async Task Run()
        {
            QueueClient queue = await _serviceClient.CreateQueueAsync("reminders");
            await queue.CreateIfNotExistsAsync();

            Console.WriteLine("=== Reminder System (Delayed Notifications) ===");

            Task backgroundWorker = Task.Run(async () => { await ProcessReminders(queue); });

            while (true)
            {
                Console.WriteLine("\n1. Add reminder");
                Console.WriteLine("0. Exit");
                Console.Write("Your choice: ");
                string c = Console.ReadLine();

                if (c == "1")
                {
                    await AddReminder(queue);
                }
                else if (c == "0")
                {
                    break;
                }
            }
        }
    }
}
