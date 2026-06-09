using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// NOTE: In a dotnetcore app the assembly attributes are located in the csproj <PropertyGroup> tag.
[assembly: System.Reflection.AssemblyKeyFileAttribute("club.snk")]
namespace NightClub;

/// <summary>
/// Multi-threaded demo of the throttled semaphore model.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        ShowVersions(new AssemblyAttributes());
        ShowLogo();

        #region [Init]
        CancellationTokenSource _cts = null;
        _cts = new CancellationTokenSource(60000); // We'll let the night club stay open for 1 minute max.
        int taskCount = 10;                        // We want this many total threads to run,
        int semaphoreCount = 3;                    // but only this many can run at the same time.
        var semaphore = new SemaphoreSlim(semaphoreCount, semaphoreCount);
        var semTasks = new Task[taskCount];
        DateTime start = DateTime.UtcNow;
        QueuedTerminal.WriteLine($"{QueuedTerminal.Divider}", ConsoleColor.DarkGray, false);
        QueuedTerminal.WriteLine($"{taskCount} people trying to enter the night club ({semaphoreCount} at a time may enter)…", ConsoleColor.Cyan, false);
        Thread.Sleep(2000);
        for (int tsk = 0; tsk < taskCount; tsk++)
        {
            semTasks[tsk] = Task.Run(() => SemaTaskNoWaitTimeout(semaphore, _cts.Token));
            //semTasks[tsk] = Task.Run(() => SemaTaskWaitTimeout(semaphore, _cts.Token));
            //semTasks[tsk] = Task.Run(async () => await SemaTaskWaitTimeoutAsync(semaphore, _cts.Token));
        }
        #endregion

        #region [Results]
        await Task.WhenAll(semTasks).ContinueWith(t =>
        {
            QueuedTerminal.WriteLine($"{QueuedTerminal.Divider}", ConsoleColor.DarkGray, false);
            if (t.Status == TaskStatus.Canceled)
                QueuedTerminal.WriteLine($"The night club was canceled, some patrons may not have been served.", ConsoleColor.DarkRed, false);
            else if (t.Status == TaskStatus.RanToCompletion)
                QueuedTerminal.WriteLine($"The night club was a success!", ConsoleColor.Green, false);
            else
                QueuedTerminal.WriteLine($"Yo, this night club got some issues.", ConsoleColor.White, false);

            DateTime end = DateTime.UtcNow;
            TimeSpan? timeDiff = end - start;
            QueuedTerminal.WriteLine($"Total time: {timeDiff.ToTimeString()} ", ConsoleColor.Yellow, false);
        });
        #endregion

        QueuedTerminal.WriteLine($"[press any key to exit]", ConsoleColor.DarkGray, false);
        var key = Console.ReadKey().Key;
    }

    /// <summary>
    /// performs an implicit cast to an object type
    /// </summary>
    public static object BoxValue(int number) { return number; }

    /// <summary>
    /// performs an explicit cast to an integer type
    /// </summary>
    public static int UnboxValue(object value) { return (int)value; }

    #region [Semaphores]
    static void SemaTaskWaitTimeout(SemaphoreSlim semaphore, CancellationToken cts, int waitMS = 2000)
    {
        bool isCompleted = false;
        while (!isCompleted)
        {
            try
            {
                if (semaphore.Wait(waitMS, cts)) // Will be true if the current thread was allowed to enter the semaphore.
                {
                    int? currId = Task.CurrentId;
                    try
                    {
                        int stay = new Random((int)DateTime.Now.Ticks).Next(750, 6001);
                        QueuedTerminal.WriteLine($"Allowed #{currId} into the night club for {stay} milliseconds.", ConsoleColor.DarkCyan, false);
                        //Task.Delay(stay).Wait();
                        Thread.Sleep(stay);
                    }
                    finally
                    {
                        semaphore.Release();
                        QueuedTerminal.WriteLine($"#{currId} left the night club, spots available: {semaphore.CurrentCount}", ConsoleColor.DarkGreen, false);
                        isCompleted = true;
                    }
                }
                else // Current thread was not allowed to enter the semaphore, so keep waiting.
                {
                    QueuedTerminal.WriteLine($"Still waiting for #{Task.CurrentId} to leave the club.", ConsoleColor.DarkYellow, false);
                }

                // Extra precaution, but the Wait should properly observe the cts...
                if (cts.IsCancellationRequested)
                {
                    QueuedTerminal.WriteLine($">> Night club is shutting down! <<", ConsoleColor.Red, false);
                    isCompleted = true;
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                QueuedTerminal.WriteLine($">> Night club canceled! <<", ConsoleColor.Yellow, false);
            }
        }
    }

    static void SemaTaskNoWaitTimeout(SemaphoreSlim semaphore, CancellationToken cts)
    {
        bool isCompleted = false;
        while (!isCompleted)
        {
            try
            {
                semaphore.Wait(cts);
                int stay = new Random((int)DateTime.Now.Ticks).Next(750, 6001);
                QueuedTerminal.WriteLine($"Allowed #{Task.CurrentId} into the night club for {stay.ToTimeString()}.", ConsoleColor.DarkCyan, false);
                //Task.Delay(stay).Wait();
                Thread.Sleep(stay);
            }
            catch (OperationCanceledException)
            {
                QueuedTerminal.WriteLine($">> Night club canceled! <<", ConsoleColor.Yellow, false);
                break;
            }
            finally
            {
                semaphore.Release();
                QueuedTerminal.WriteLine($"#{Task.CurrentId} left the night club, spots available: {semaphore.CurrentCount}", ConsoleColor.DarkGreen, false);
                isCompleted = true;
            }

            // Extra precaution, but the Wait should properly observe the cts...
            if (cts.IsCancellationRequested)
            {
                QueuedTerminal.WriteLine($">> Night club is shutting down! <<", ConsoleColor.Red, false);
                break;
            }
        }
    }

    static async Task SemaTaskWaitTimeoutAsync(SemaphoreSlim semaphore, CancellationToken cts)
    {
        bool isCompleted = false;
        while (!isCompleted)
        {
            try
            {
                if (semaphore.Wait(2000, cts)) // Will be true if the current thread was allowed to enter the semaphore.
                {
                    int? currId = Task.CurrentId;
                    try
                    {
                        int stay = new Random((int)DateTime.Now.Ticks).Next(750, 6001);
                        QueuedTerminal.WriteLine($"Allowed #{currId} into the night club for {stay} milliseconds.", ConsoleColor.DarkCyan, false);
                        await Task.Delay(stay);
                    }
                    finally
                    {
                        semaphore.Release();
                        QueuedTerminal.WriteLine($"#{currId} left the night club, spots available: {semaphore.CurrentCount}", ConsoleColor.DarkGreen, false);
                        isCompleted = true;
                    }
                }
                else // Current thread was not allowed to enter the semaphore, so keep waiting.
                {
                    QueuedTerminal.WriteLine($"Still waiting for #{Task.CurrentId} to leave the club.", ConsoleColor.DarkYellow, false);
                }

                // Extra precaution, but the Wait should properly observe the cts...
                if (cts.IsCancellationRequested)
                {
                    QueuedTerminal.WriteLine($">> Night club is shutting down! <<", ConsoleColor.Red, false);
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                QueuedTerminal.WriteLine($">> Night club canceled! <<", ConsoleColor.Yellow, false);
                throw new OperationCanceledException();
            }
        }
    }
    #endregion

    #region [Misc]
    static void ShowVersions(AssemblyAttributes asa)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var iva = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        Console.WriteLine($" [Version: {asa.AssemblyVersion}]   [FileVersion: {asa.AssemblyFileVersion}]   [InformationalVersion: {iva}] ");
    }

    static void ShowLogo()
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(@"  __   __     __     ______     __  __     ______       ______     __         __  __     ______   ");
        Console.WriteLine(@" /\ '-.\ \   /\ \   /\  ___\   /\ \_\ \   /\__  _\     /\  ___\   /\ \       /\ \/\ \   /\  == \  ");
        Console.WriteLine(@" \ \ \-.  \  \ \ \  \ \ \__ \  \ \  __ \  \/_/\ \/     \ \ \____  \ \ \____  \ \ \_\ \  \ \  __<  ");
        Console.WriteLine(@"  \ \_\\'\_\  \ \_\  \ \_____\  \ \_\ \_\    \ \_\      \ \_____\  \ \_____\  \ \_____\  \ \_____\");
        Console.WriteLine(@"   \/_/ \/_/   \/_/   \/_____/   \/_/\/_/     \/_/       \/_____/   \/_____/   \/_____/   \/_____/");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    static void IQueryableVsIEnumerableTest()
    {
        #region [IQueryable vs IEnumerable]
        // For the test we assume that our data set lives across a network path.
        ORM db = new ORM();

        /** This is the correct performant way: **/
        using (StopClock sc = new StopClock("IQueryable Time"))
        {
            // This will NOT cause all records to be fetched from across the network...
            IQueryable<Customer> lowBandwidth = db.GetCustomersAsQueryable();
            // The collection has not been fetched yet, so doing a query at this point will not cause the fetch to occur...
            IQueryable<Customer> highRevCust = lowBandwidth.Where(c => c.Revenue > db.Max / 1.2);
            // This will cause only the filtered records to be fetched from across the network...
            //var finalCollection = highRevCust.ToList(); // You could also use .ToArray() "var finalArray = highRevCust.ToArray();"

            QueuedTerminal.WriteLine($"IQueryable<Customer> total: {highRevCust.Count()}", ConsoleColor.DarkMagenta, false);
        }

        /** This is the not ideal way: **/
        using (StopClock sc = new StopClock("IEnumerable Time"))
        {
            // This will cause all records to be fetched from across the network...
            IEnumerable<Customer> highBandwidth = db.GetCustomersAsEnumerable();
            // The collection has already been fetched so doing a query at this point will not add to the traffic...
            IEnumerable<Customer> highRevCust = highBandwidth.Where(c => c.Revenue > db.Max / 1.2);

            QueuedTerminal.WriteLine($"IEnumerable<Customer> total: {highRevCust.Count()}", ConsoleColor.DarkMagenta, false);
        }

        #endregion
    }

    static void BoxingVsUnboxingTest()
    {
        #region [Boxing vs Unboxing]
        int someNumber = 420;
        // Boxing is 20 times slower than assignment.
        object boxed = someNumber;
        // Unboxing is 4 times slower than assignment.
        int unboxed = (int)someNumber;

        var arrayOfInts = Enumerable.Range(1, 100).ToArray();
        // The individual storage type of each element in an arrayList is an object.
        // So each of the 100 integers will be boxed as an object.
        var al = new ArrayList(arrayOfInts);
        // This is the correct way. All 100 integers will be stored as individual integer types.
        var il = new List<int>(arrayOfInts);
        #endregion
    }
    #endregion
}
