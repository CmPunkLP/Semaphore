using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    private static readonly int storageCapacity = 10; 
    private static readonly int maxAccessCount = 3;   

    private static SemaphoreSlim producerSemaphore = new SemaphoreSlim(storageCapacity);
    private static SemaphoreSlim consumerSemaphore = new SemaphoreSlim(0);
    private static SemaphoreSlim accessSemaphore = new SemaphoreSlim(maxAccessCount);

    private static Queue<int> storage = new Queue<int>();
    private static readonly object storageLock = new object();

    static void Main()
    {
        int numberOfProducers = 3; 
        int numberOfConsumers = 3; 

        for (int i = 1; i <= numberOfProducers; i++)
        {
            int producerId = i;
            new Thread(() => Producer(producerId)).Start();
        }

        for (int i = 1; i <= numberOfConsumers; i++)
        {
            int consumerId = i;
            new Thread(() => Consumer(consumerId)).Start();
        }
    }

    static void Producer(int id)
    {
        while (true)
        {
            producerSemaphore.Wait();
            accessSemaphore.Wait();

            lock (storageLock)
            {
                if (storage.Count < storageCapacity)
                {
                    int product = new Random().Next(100); 
                    storage.Enqueue(product);
                    Console.WriteLine($"Виробник {id} додав товар: {product}. Сховище: {storage.Count}/{storageCapacity}");
                }
            }

            accessSemaphore.Release();
            consumerSemaphore.Release();
            Thread.Sleep(new Random().Next(1000)); 
        }
    }

    static void Consumer(int id)
    {
        while (true)
        {
            consumerSemaphore.Wait();
            accessSemaphore.Wait();

            lock (storageLock)
            {
                if (storage.Count > 0)
                {
                    int product = storage.Dequeue();
                    Console.WriteLine($"Споживач {id} взяв товар: {product}. Сховище: {storage.Count}/{storageCapacity}");
                }
            }

            accessSemaphore.Release();
            producerSemaphore.Release();
            Thread.Sleep(new Random().Next(1000)); 
        }
    }
}
