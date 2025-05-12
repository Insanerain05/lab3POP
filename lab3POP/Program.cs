using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProducerConsumer
{
    class Program
    {
        private static readonly object locker = new object();
        private static Semaphore access;
        private static Semaphore full;
        private static Semaphore empty;
        private static List<string> storage = new List<string>();

        private static int storageCapacity = 5;
        private static int totalItems = 20;
        private static int producerCount = 3;
        private static int consumerCount = 2;

        static void Main(string[] args)
        {
            access = new Semaphore(1, 1);
            full = new Semaphore(storageCapacity, storageCapacity);
            empty = new Semaphore(0, storageCapacity);

        
            int baseProducerItems = totalItems / producerCount;
            int[] producerItems = new int[producerCount];
            for (int i = 0; i < producerCount; i++)
                producerItems[i] = baseProducerItems + (i < totalItems % producerCount ? 1 : 0);

            int baseConsumerItems = totalItems / consumerCount;
            int[] consumerItems = new int[consumerCount];
            for (int i = 0; i < consumerCount; i++)
                consumerItems[i] = baseConsumerItems + (i < totalItems % consumerCount ? 1 : 0);

        
            Thread[] producers = new Thread[producerCount];
            for (int i = 0; i < producerCount; i++)
            {
                int id = i + 1;
                int items = producerItems[i];
                producers[i] = new Thread(() => Producer(id, items));
                producers[i].Start();
            }

            Thread[] consumers = new Thread[consumerCount];
            for (int i = 0; i < consumerCount; i++)
            {
                int id = i + 1;
                int items = consumerItems[i];
                consumers[i] = new Thread(() => Consumer(id, items));
                consumers[i].Start();
            }

            foreach (var t in producers) t.Join();
            foreach (var t in consumers) t.Join();

            Console.WriteLine("\nВсі виробники і споживачі завершили роботу.");
        }

        static void Producer(int id, int count)
        {
            for (int i = 0; i < count; i++)
            {
                full.WaitOne();
                access.WaitOne();

                string item = $"Продукцiя {id}-{i + 1}";
                storage.Add(item);
                Console.WriteLine($"[Виробник {id}] Додав: {item}");

                access.Release();
                empty.Release();

                Thread.Sleep(new Random().Next(200, 500));
            }

            Console.WriteLine($"[Виробник {id}] Завершив виробництво.");
        }

        static void Consumer(int id, int count)
        {
            for (int i = 0; i < count; i++)
            {
                empty.WaitOne();
                access.WaitOne();

                string item = storage.First();
                storage.RemoveAt(0);
                Console.WriteLine($"[Споживач {id}] Взяв: {item}");

                access.Release();
                full.Release();

                Thread.Sleep(new Random().Next(300, 600));
            }

            Console.WriteLine($"[Споживач {id}] Завершив споживання.");
        }
    }
}
