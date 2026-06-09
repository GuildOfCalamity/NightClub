using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NightClub;

/// <summary>
/// Support class for writing to console terminal.
/// </summary>
public static class QueuedTerminal
{
    private static bool _ThreadRunning = true;

    // BlockingCollection represents a collection that allows for thread-safe adding and removal of data. 
    private static BlockingCollection<Message> m_Collection = new BlockingCollection<Message>();

    // Bags are useful for storing objects when ordering doesn't matter, and unlike sets, bags support duplicates. 
    private static ConcurrentBag<Message> m_Bag = new ConcurrentBag<Message>();

    // Represents a thread-safe first in-first out (FIFO) collection.
    private static ConcurrentQueue<Message> m_Queue = new ConcurrentQueue<Message>();

    // Represents a thread-safe last in-first out (LIFO) collection.
    private static ConcurrentStack<Message> m_Stack = new ConcurrentStack<Message>();

    // Represents a thread-safe collection of key/value pairs that can be accessed by multiple threads concurrently.
    private static ConcurrentDictionary<int, Message> m_Dictionary = new ConcurrentDictionary<int, Message>();

    public static string Divider = "▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲▼▲";
    static QueuedTerminal()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Configure our console writing thread delegate...
        Thread thread = new Thread(() => 
        {
            while (_ThreadRunning)
            {
                // NOTE: To use an enumerable with the collection you would call GetConsumingEnumerable()
                // foreach (var item in m_Collection.GetConsumingEnumerable())
                //     Console.WriteLine($"Consuming: {item}");
                if (m_Collection.Count > 0)
                {
                    Message message;
                    if (m_Collection.TryTake(out message))
                    {
                        Console.ForegroundColor = message.color;
                        Console.Write((message.time ? $"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] " : " ") + message.text);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("WARNING: Unable to remove message from BlockingCollection!");
                    }
                }
                else // Always include a delay when there's nothing to do to prevent CPU pinning.
                {
                    Thread.Sleep(5);
                }
            }
            m_Collection.Dispose();
        });

        thread.Name = "QueuedTerminal";
        thread.Priority = ThreadPriority.Lowest;
        thread.IsBackground = true;
        thread.Start();
    }

    #region [Public Methods]
    public static void WriteLine(string value, bool time = true)
    {
        if (!m_Collection.TryAdd(new Message(value + "\r\n", ConsoleColor.Gray, time)))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WARNING: Unable to add message to BlockingCollection!");
        }
        //m_Collection.CompleteAdding(); //marks the collection as not accepting any more additions
    }

    public static void Write(string value, bool time = true)
    {
        if (!m_Collection.TryAdd(new Message(value, ConsoleColor.Gray, time)))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WARNING: Unable to add message to BlockingCollection!");
        }
        //m_Collection.CompleteAdding(); //marks the collection as not accepting any more additions
    }

    public static void WriteLine(string value, ConsoleColor color, bool time = true)
    {
        if (!m_Collection.TryAdd(new Message(value + "\r\n", color, time)))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WARNING: Unable to add message to BlockingCollection!");
        }
        //m_Collection.CompleteAdding(); //marks the collection as not accepting any more additions
    }

    public static void Write(string value, ConsoleColor color, bool time = true)
    {
        if (!m_Collection.TryAdd(new Message(value, color, time)))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("WARNING: Unable to add message to BlockingCollection!");
        }
        //m_Collection.CompleteAdding(); //marks the collection as not accepting any more additions
    }

    public static void ShutDown()
    {
        _ThreadRunning = false;
    }
    #endregion

    #region [Terminal Message Structure]
    private struct Message
    {
        public string text;
        public ConsoleColor color;
        public bool time;

        public Message(string value, ConsoleColor color, bool time)
        {
            this.text = value;
            this.color = color;
            this.time = time;
        }
    }
    #endregion [Terminal Message Structure]
}
