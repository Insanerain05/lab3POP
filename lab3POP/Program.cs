using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace lab3POP
{
    class Program
    {
        private Semaphore Access;
        private Semaphore Full;
        private Semaphore Empty;

        static int consumersAmount = 2;
        static int producersAmount = 3;
        static int storageSize = 3;
        static int totalItems = 10;

        private Thread[] consumers = new Thread[consumersAmount];
        private Thread[] producers = new Thread[producersAmount];
        private readonly List<string> storage = new List<string>();
        private int itemCounter = 0;

        static void Main(string[] args)
        {
            new Program().Start(storageSize, totalItems);
        }

        private void Start(int storageSize, int itemCount)
        {
            Access = new Semaphore(1, 1);
            Full = new Semaphore(storageSize, storageSize);
            Empty = new Semaphore(0, storageSize);

            int[] producerTasks = DistributeItems(itemCount, producersAmount);
            for (int i = 0; i < producersAmount; i++)
            {
                producers[i] = new Thread(Producer);
                producers[i].Start(producerTasks[i]);
            }

            int[] consumerTasks = DistributeItems(itemCount, consumersAmount);
            for (int i = 0; i < consumersAmount; i++)
            {
                consumers[i] = new Thread(Consumer);
                consumers[i].Start(consumerTasks[i]);
            }

            foreach (var producer in producers) producer.Join();
            foreach (var consumer in consumers) consumer.Join();

            Console.WriteLine("Вся продукція оброблена. Програма завершила роботу.");
        }

        private int[] DistributeItems(int total, int participants)
        {
            int[] result = new int[participants];
            int baseAmount = total / participants;
            int extra = total % participants;

            for (int i = 0; i < participants; i++)
                result[i] = baseAmount + (i < extra ? 1 : 0);

            return result;
        }

        private void Producer(object obj)
        {
            int itemsToProduce = (int)obj;
            for (int i = 0; i < itemsToProduce; i++)
            {
                Full.WaitOne();       
                Access.WaitOne();     

                string item = $"Item {itemCounter++}";
                storage.Add(item);
                Console.WriteLine($"Виробник додав: {item}");

                Access.Release();     
                Empty.Release();     
            }
        }

        private void Consumer(object obj)
        {
            int itemsToConsume = (int)obj;
            for (int i = 0; i < itemsToConsume; i++)
            {
                Empty.WaitOne();      
                Thread.Sleep(500);   
                Access.WaitOne();    

                string item = storage.First();
                storage.RemoveAt(0);
                Console.WriteLine($"Споживач взяв: {item}");

                Access.Release();     
                Full.Release();       
            }
        }
    }
}
