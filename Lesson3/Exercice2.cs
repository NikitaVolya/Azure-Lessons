using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;


namespace Lesson3
{
    internal class Exercice2
    {
        class TaskPayload
        {
            public string text { get; set; }
        }

        private static Random _random = new Random();

        public QueueServiceClient _serviceClient;

        QueueClient _highQueue;
        QueueClient _mediumQueue;
        QueueClient _lowQueue;

        int _highCount;
        int _mediumCount;
        int _lowCount;


        public Exercice2(QueueServiceClient serviceClient)
        {
            _serviceClient = serviceClient;

            _highQueue = serviceClient.CreateQueue("tasks-high");
            _mediumQueue = serviceClient.CreateQueue("tasks-medium");
            _lowQueue = serviceClient.CreateQueue("tasks-low");

            _highCount = 0;
            _mediumCount = 0;
            _lowCount = 0;
        }

        static async Task<QueueMessage> TryReceive(QueueClient queue)
        {
            var msgs = await queue.ReceiveMessagesAsync(1);
            return msgs.Value.Length > 0 ? msgs.Value[0] : null;
        }

        async Task ProcessTasks()
        {
            while (true)
            {
                QueueMessage msg = null;
                QueueClient selectedQueue = null;

                msg = await TryReceive(_highQueue);
                if (msg != null) selectedQueue = _highQueue;

                if (msg == null)
                {
                    msg = await TryReceive(_mediumQueue);
                    if (msg != null) selectedQueue = _mediumQueue;
                }

                if (msg == null)
                {
                    msg = await TryReceive(_lowQueue);
                    if (msg != null) selectedQueue = _lowQueue;
                }

                if (msg != null)
                {
                    var task = JsonSerializer.Deserialize<TaskPayload>(msg.MessageText);

                    Console.WriteLine($"\nProcessing: {task.text}");

                    await Task.Delay(_random.Next(2000, 3000));

                    await selectedQueue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);

                    if (selectedQueue == _highQueue) _highCount++;
                    if (selectedQueue == _mediumQueue) _mediumCount++;
                    if (selectedQueue == _lowQueue) _lowCount++;

                    Console.WriteLine("Task processed!");
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
        }

        async Task AddTask()
        {
            Console.Write("Enter task text: ");
            string text = Console.ReadLine();

            Console.WriteLine("Choose priority:");
            Console.WriteLine("1. High");
            Console.WriteLine("2. Medium");
            Console.WriteLine("3. Low");
            Console.Write("Your choice: ");

            string pr = Console.ReadLine();

            string json = JsonSerializer.Serialize(new { text = text });

            switch (pr)
            {
                case "1":
                    await _highQueue.SendMessageAsync(json);
                    Console.WriteLine("Task added to HIGH queue");
                    break;
                case "2":
                    await _mediumQueue.SendMessageAsync(json);
                    Console.WriteLine("Task added to MEDIUM queue");
                    break;
                case "3":
                    await _lowQueue.SendMessageAsync(json);
                    Console.WriteLine("Task added to LOW queue");
                    break;
                default:
                    Console.WriteLine("Wrong choice!");
                    break;
            }
        }

        void ShowCounters()
        {
            Console.WriteLine("\n=== Processed tasks count ===");
            Console.WriteLine($"High:   {_highCount}");
            Console.WriteLine($"Medium: {_mediumCount}");
            Console.WriteLine($"Low:    {_lowCount}");
        }


        public async Task Run()
        {
            Console.WriteLine("=== Priority Task Queue System ===");

            _ = Task.Run(ProcessTasks);

            while (true)
            {
                Console.WriteLine("\n1. Add task");
                Console.WriteLine("2. Show counters");
                Console.WriteLine("0. Exit");
                Console.Write("Your choice: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                    await AddTask();
                else if (choice == "2")
                    ShowCounters();
                else if (choice == "0")
                    break;
            }
        }   
    }
}
