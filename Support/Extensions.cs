#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NightClub;

public static class Extensions
{
    public static async Task Throttle(this IEnumerable<Func<Task>> toRun, int throttleTo)
    {
        var running = new List<Task>(throttleTo);
        foreach (var taskToRun in toRun)
        {
            running.Add(taskToRun());
            if (running.Count == throttleTo)
            {
                var comTask = await Task.WhenAny(running);
                running.Remove(comTask);
            }
        }
    }

    public static async Task<IEnumerable<T>> Throttle<T>(IEnumerable<Func<Task<T>>> toRun, int throttleTo)
    {
        var running = new List<Task<T>>(throttleTo);
        var completed = new List<Task<T>>(toRun.Count());
        foreach (var taskToRun in toRun)
        {
            running.Add(taskToRun());
            if (running.Count == throttleTo)
            {
                var comTask = await Task.WhenAny(running);
                running.Remove(comTask);
                completed.Add(comTask);
            }
        }
        return completed.Select(t => t.Result);
    }
    
    #region [Taken From ImageResizer]
    public static int Clamp(int value, int min, int max)
    { 
        return Math.Min(Math.Max(value, min), max);
    }

    public static T GetCustomAttribute<T>(this Assembly assembly) where T : Attribute
    { 
        #pragma warning disable CS8600, CS8603
        return (T)assembly.GetCustomAttributes(typeof(T), inherit: false).SingleOrDefault();
        #pragma warning restore CS8600, CS8603
    }

    public static TimeSpan Multiply(this TimeSpan timeSpan, double scalar)
    { 
        return new TimeSpan((long)(timeSpan.Ticks * scalar));
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }
    #endregion

    /// <summary>
    /// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.
    /// e.g. 420 milliseconds, 3.1 seconds, 2 minutes, 4.231 hours, etc.
    /// </summary>
    /// <param name="span">millisecond amount to be converted to <see cref="TimeSpan"/></param>
    /// <param name="significantDigits">number of right side digits in output (precision)</param>
    /// <returns></returns>
    public static string ToTimeString(this int span, int significantDigits = 3)
    {
        var ts = new TimeSpan(0, 0, 0, 0, span);
        var format = $"G{significantDigits}";
        return ts.TotalMilliseconds < 1000 ? ts.TotalMilliseconds.ToString(format) + " milliseconds"
                : (ts.TotalSeconds < 60 ? ts.TotalSeconds.ToString(format) + " seconds"
                : (ts.TotalMinutes < 60 ? ts.TotalMinutes.ToString(format) + " minutes"
                : (ts.TotalHours < 24 ? ts.TotalHours.ToString(format) + " hours"
                : ts.TotalDays.ToString(format) + " days")));
    }

    /// <summary>
    /// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.
    /// e.g. 420 milliseconds, 3.1 seconds, 2 minutes, 4.231 hours, etc.
    /// </summary>
    /// <param name="span"><see cref="TimeSpan"/></param>
    /// <param name="significantDigits">number of right side digits in output (precision)</param>
    /// <returns></returns>
    public static string ToTimeString(this TimeSpan span, int significantDigits = 3)
    {
        var format = $"G{significantDigits}";
        return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format) + " milliseconds"
                : (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format) + " seconds"
                : (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format) + " minutes"
                : (span.TotalHours < 24 ? span.TotalHours.ToString(format) + " hours"
                : span.TotalDays.ToString(format) + " days")));
    }

    /// <summary>
    /// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.
    /// e.g. 420 milliseconds, 3.1 seconds, 2 minutes, 4.231 hours, etc.
    /// </summary>
    /// <param name="span"><see cref="TimeSpan"/></param>
    /// <param name="significantDigits">number of right side digits in output (precision)</param>
    /// <returns></returns>
    public static string ToTimeString(this TimeSpan? span, int significantDigits = 3)
    {
        var format = $"G{significantDigits}";
        return span?.TotalMilliseconds < 1000 ? span?.TotalMilliseconds.ToString(format) + " milliseconds"
                : (span?.TotalSeconds < 60 ? span?.TotalSeconds.ToString(format) + " seconds"
                : (span?.TotalMinutes < 60 ? span?.TotalMinutes.ToString(format) + " minutes"
                : (span?.TotalHours < 24 ? span?.TotalHours.ToString(format) + " hours"
                : span?.TotalDays.ToString(format) + " days")));
    }

    /// <summary>
    /// IEnumerable file reader helper.
    /// </summary>
    /// <param name="file">the full path to the file</param>
    /// <returns><see cref="IEnumerable{string}"/></returns>
    public static IEnumerable<string> ReadFileLines(this string file)
    {
        if (!File.Exists(file))
            yield break;

        using FileStream stream = File.OpenRead(file);
        using StreamReader reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            if (line == null)
                yield return string.Empty;
            else
                yield return line;
        }
    }

    #region [IEnumerable Helpers]
    public static IEnumerable<T> JoinLists<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
    {
        var joined = new[] { list1, list2 }.Where(x => x != null).SelectMany(x => x);
        return joined ?? Enumerable.Empty<T>();
    }
    public static IEnumerable<T> JoinLists<T>(this IEnumerable<T> list1, IEnumerable<T> list2, IEnumerable<T> list3)
    {
        var joined = new[] { list1, list2, list3 }.Where(x => x != null).SelectMany(x => x);
        return joined ?? Enumerable.Empty<T>();
    }
    public static IEnumerable<T> JoinMany<T>(params IEnumerable<T>[] array)
    {
        var final = array.Where(x => x != null).SelectMany(x => x);
        return final ?? Enumerable.Empty<T>();
    }
    #endregion

    #region [Config helpers]
    /// <summary>
    /// To be used with a config setting <see cref="List{string}"/>.
    /// </summary>
    /// <example>
    /// var result = settings.Update("[lastrun]", $"{DateTime.Now}");
    /// </example>
    public static bool Update(this List<string> source, string? tag, string? newValue, bool addIfNotFound = true)
    {
        if (source == null)
            return false;

        tag ??= "[empty]";
        newValue ??= "";

        int index = source.FindIndex(s => s.Contains(tag));

        if (index != -1)
            source[index] = $"{tag}{newValue}";
        else if (addIfNotFound)
            source.Add($"{tag}{newValue}");
        else
            return false;

        return true;
    }

    /// <summary>
    /// To be used with a config setting <see cref="List{string}"/>.
    /// </summary>
    /// <example>
    /// var result = settings.Fetch("[lastrun]");
    /// </example>
    public static string Fetch(this List<string> source, string? tag)
    {
        if (source == null)
            return string.Empty;

        tag ??= "[empty]";

        int index = source.FindIndex(s => s.Contains(tag));

        if (index != -1)
            return source[index].Replace(tag, "");
        else
            return string.Empty;
    }
    #endregion

    #region [IList Helpers]
    public static void RemoveDuplicates<T>(this List<T> list)
    {
        list.Sort();
        var index = 0;
        while (index < list.Count - 1)
        {
            if (Equals(list[index], list[index + 1]))
                list.RemoveAt(index);
            else
                index++;
        }
    }

    public static int Replace<T>(this IList<T> source, T oldValue, T newValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        var index = source.IndexOf(oldValue);
        if (index != -1)
            source[index] = newValue;

        return index;
    }

    public static void ReplaceAll<T>(this IList<T> source, T oldValue, T newValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        int index = -1;
        do
        {
            index = source.IndexOf(oldValue);
            if (index != -1)
                source[index] = newValue;
        } while (index != -1);
    }


    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T oldValue, T newValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(x => EqualityComparer<T>.Default.Equals(x, oldValue) ? newValue : x);
    }
    #endregion

    #region [LINQ Extenders]
    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
    {
        foreach (var i in ie)
            action(i);
    }

    /// <summary>Determines whether any element of a sequence satisfies a condition.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> whose elements to apply the predicate to.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns><c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.</returns>
    public static bool AnyExt<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (TSource element in source)
            if (predicate(element)) return true;
        return false;
    }

    /// <summary>Casts the elements of an <see cref="IEnumerable"/> to the specified type.</summary>
    /// <typeparam name="TResult">The type to cast the elements of source to.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> that contains the elements to be cast to type <typeparamref name="TResult"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains each element of the source sequence cast to the specified type.</returns>
    public static IEnumerable<TResult> CastExt<TResult>(this IEnumerable source)
    {
        foreach (var i in source)
            yield return (TResult)i;
    }

    /// <summary>Determines whether a sequence contains a specified element by using the default equality comparer.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">A sequence in which to locate a value.</param>
    /// <param name="value">The value to locate in the sequence.</param>
    /// <returns><c>true</c> if the source sequence contains an element that has the specified value; otherwise, <c>false</c>.</returns>
    public static bool ContainsExt<TSource>(this IEnumerable<TSource> source, TSource value)
    {
        foreach (var i in source)
            if (i != null && i.Equals(value)) return true;
        return false;
    }

    /// <summary>Returns the number of elements in a sequence.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">A sequence that contains elements to be counted.</param>
    /// <returns>The number of elements in the input sequence.</returns>
    public static int CountExt<TSource>(this IEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is ICollection<TSource> c) return c.Count;
        if (source is ICollection ngc) return ngc.Count;
        var i = 0;
        foreach (var e in source) i++;
        return i;
    }

    /// <summary>Returns distinct elements from a sequence by using the default equality comparer to compare values.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The sequence to remove duplicate elements from.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
    public static IEnumerable<TSource> DistinctExt<TSource>(this IEnumerable<TSource> source)
    {
        var set = new Hashtable();
        foreach (var element in source)
            if (element != null && !set.ContainsKey(element))
            {
                set.Add(element, null);
                yield return element;
            }
    }

    /// <summary>Returns the first element of a sequence.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return the first element of.</param>
    /// <returns>The first element in the specified sequence.</returns>
    public static TSource FirstExt<TSource>(this IEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is IList<TSource> list)
        {
            if (list.Count > 0) return list[0];
        }
        else
        {
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext()) return e.Current;
            }
        }
        throw new InvalidOperationException(@"No elements");
    }

    /// <summary>Returns the first element of a sequence that satisfies a specified condition.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return the first element of.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
    public static TSource FirstExt<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var element in source)
            if (predicate(element)) return element;

        throw new InvalidOperationException("No match");
    }

    /// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return the first element of.</param>
    /// <returns><c>default( <typeparamref name="TSource"/>)</c> if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.</returns>
    public static TSource? FirstOrDefaultExt<TSource>(this IEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is IList<TSource> list)
        {
            if (list.Count > 0) return list[0];
        }
        else
        {
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext()) return e.Current;
            }
        }
        return default(TSource);
    }

    /// <summary>Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.</summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to return an element from.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns><c>default(<typeparamref name="TSource"/>)</c> if <paramref name="source"/> is empty or if no element passes the test specified by <paramref name="predicate"/>; otherwise, the first element in <paramref name="source"/> that passes the test specified by <paramref name="predicate"/>.</returns>
    public static TSource? FirstOrDefaultExt<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var element in source)
            if (predicate(element)) return element;
        return default(TSource);
    }

    /// <summary>Returns the minimum value in a generic sequence.</summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to determine the minimum value of.</param>
    /// <returns>The minimum value in the sequence.</returns>
    public static TSource? MinExt<TSource>(this IEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var comparer = Comparer<TSource>.Default;
        var value = default(TSource);
        if (value == null)
        {
            foreach (var x in source)
            {
                if (x != null && (value == null || comparer.Compare(x, value) < 0))
                    value = x;
            }
            return value;
        }

        var hasValue = false;
        foreach (var x in source)
        {
            if (hasValue)
            {
                if (comparer.Compare(x, value) < 0)
                    value = x;
            }
            else
            {
                value = x;
                hasValue = true;
            }
        }
        
        if (hasValue)
            return value;

        throw new InvalidOperationException("No elements");
    }

    /// <summary>Returns the maximum value in a generic sequence.</summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to determine the maximum value of.</param>
    /// <returns>The maximum value in the sequence.</returns>
    public static TSource? MaxExt<TSource>(this IEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var comparer = Comparer<TSource>.Default;
        var value = default(TSource);
        if (value == null)
        {
            foreach (var x in source)
            {
                if (x != null && (value == null || comparer.Compare(x, value) > 0))
                    value = x;
            }
            return value;
        }

        var hasValue = false;
        foreach (var x in source)
        {
            if (hasValue)
            {
                if (comparer.Compare(x, value) > 0)
                    value = x;
            }
            else
            {
                value = x;
                hasValue = true;
            }
        }
        if (hasValue) 
            return value;

        throw new InvalidOperationException("No elements");
    }

    /// <summary>Sorts the elements of a sequence in ascending order according to a key.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <param name="source">A sequence of values to order.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> whose elements are sorted according to a key.</returns>
    public static IEnumerable<TSource> OrderByExt<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var d = new SortedDictionary<TKey, TSource>();
        foreach (var item in source)
            d.Add(keySelector(item), item);
        return d.Values;
    }

    /// <summary>Projects each element of a sequence into a new form.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
    /// <param name="source">A sequence of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> whose elements are the result of invoking the transform function on each element of <paramref name="source"/>.</returns>
    public static IEnumerable<TResult> SelectExt<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        foreach (var i in source)
            yield return selector(i);
    }

    /// <summary>Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to return a single element from.</param>
    /// <param name="predicate">A function to test an element for a condition.</param>
    /// <returns>The single element of the input sequence that satisfies a condition.</returns>
    public static TSource SingleExt<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var result = default(TSource);
        long count = 0;
        foreach (var element in source)
        {
            if (!predicate(element)) continue;
            result = element;
            checked { count++; }
        }
        if (count == 0) throw new InvalidOperationException("No matches");
        if (count != 1) throw new InvalidOperationException("More than one match.");
        return result;
    }

    /// <summary>Computes the sum of a sequence of nullable <see cref="Int32"/> values.</summary>
    /// <param name="source">A sequence of nullable <see cref="Int32"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the sequence.</returns>
    public static int SumExt(this IEnumerable<int> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        int sum = 0;
        checked
        {
            foreach (int v in source) sum += v;
        }
        return sum;
    }

    /// <summary>
    /// Computes the sum of the sequence of nullable <see cref="Int32"/> values that are obtained by invoking a transform function on each element of the input sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">A sequence of values that are used to calculate a sum.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>The sum of the projected values.</returns>
    public static int SumExt<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => SumExt(SelectExt(source, selector));

    /// <summary>Creates an array from a <see cref="IEnumerable"/>.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to create an array from.</param>
    /// <returns>An array that contains the elements from the input sequence.</returns>
    public static TSource[] ToArrayExt<TSource>(this IEnumerable<TSource> source) => ToListExt(source).ToArray();

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/> according to a specified key selector function, a comparer, and
    /// an element selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey,TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys.</param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains values of type TElement selected from the input sequence.</returns>
    public static Dictionary<TKey, TElement> ToDictionaryExt<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);

        var d = new Dictionary<TKey, TElement>(comparer);
        foreach (var element in source) d.Add(keySelector(element), elementSelector(element));
        return d;
    }

    /// <summary>Creates a <see cref="List{T}"/> from an <see cref="IEnumerable{T}"/>.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="List{T}"/> from.</param>
    /// <returns>A <see cref="List{T}"/> that contains elements from the input sequence.</returns>
    public static List<TSource> ToListExt<TSource>(this IEnumerable<TSource> source)
    {
        var l = new List<TSource>();
        foreach (var i in source)
            l.Add(i);
        return l;
    }

    /// <summary>Filters a sequence of values based on a predicate.</summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to filter.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from the input sequence that satisfy the condition.</returns>
    public static IEnumerable<TSource> WhereExt<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var i in source)
            if (predicate(i)) yield return i;
    }
    #endregion

    #region [Task Helpers]
    /// <summary>
    /// Task.Factory.StartNew (() => { throw null; }).IgnoreExceptions();
    /// </summary>
    public static void IgnoreExceptions(this Task task, bool logEx = false)
    {
        task.ContinueWith(t =>
        {
            var ignore = t.Exception;
            ignore?.Flatten().Handle(ex =>
            {
                if (logEx)
                    Console.WriteLine("Exception type: {0}\r\nException Message: {1}", ex.GetType(), ex.Message);
                return true; // don't re-throw
            });

        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Not sure if you would ever want this?
    /// We can't return a result if the task failed, so this will throw 
    /// another exception if that happens during the "return t.Result;".
    /// </summary>
    public static Task<TResult> IgnoreExceptions<TResult>(this Task<TResult> task, bool logEx = false)
    {
        var tmp = task.ContinueWith(t =>
        {
            var ignore = t.Exception;
            ignore?.Flatten().Handle(ex =>
            {
                if (logEx)
                    Console.WriteLine("Exception type: {0}\r\nException Message: {1}", ex.GetType(), ex.Message);
                return true; // don't re-throw
            });

            if (t.Status == TaskStatus.Faulted)
                Console.WriteLine("Next line will cause exception!");
            return t.Result; // if we faulted then this will be an issue since there may not be a result to give

        }, TaskContinuationOptions.OnlyOnFaulted);

        return tmp;
    }

    /// <summary>
    /// Chainable task helper.
    /// var result = await SomeLongAsyncFunction().WithTimeout(TimeSpan.FromSeconds(2));
    /// </summary>
    /// <typeparam name="TResult">the type of task result</typeparam>
    /// <returns><see cref="Task"/>TResult</returns>
    public async static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        Task winner = await (Task.WhenAny(task, Task.Delay(timeout)));

        if (winner != task)
            throw new TimeoutException();

        return await task;   // Unwrap result/re-throw
    }

    /// <summary>
    /// Task extension to add a timeout.
    /// </summary>
    /// <returns>The task with timeout.</returns>
    /// <param name="task">Task.</param>
    /// <param name="timeoutInMilliseconds">Timeout duration in Milliseconds.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public async static Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds)
    {
        var retTask = await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds))
            .ConfigureAwait(false);

#pragma warning disable CS8603 // Possible null reference return.
        return retTask is Task<T> ? task.Result : default;
#pragma warning restore CS8603 // Possible null reference return.
    }

    /// <summary>
    /// Chainable task helper.
    /// var result = await SomeLongAsyncFunction().WithCancellation(cts.Token);
    /// </summary>
    /// <typeparam name="TResult">the type of task result</typeparam>
    /// <returns><see cref="Task"/>TResult</returns>
    public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancelToken)
    {
        TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
        CancellationTokenRegistration reg = cancelToken.Register(() => tcs.TrySetCanceled());
        task?.ContinueWith(ant =>
        {
            reg.Dispose(); // NOTE: it's important to dispose of CancellationTokenRegistrations or they will hang around in memory until the application closes
            if (ant.IsCanceled)
                tcs.TrySetCanceled();
            else if (ant.IsFaulted)
                tcs.TrySetException(ant.Exception?.InnerException ?? new AggregateException("Task status faulted."));
            else
                tcs.TrySetResult(ant.Result);
        });
        return tcs.Task;  // Return the TaskCompletionSource result
    }

    public static Task<T> WithAllExceptions<T>(this Task<T> task)
    {
        TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

        task.ContinueWith(ignored =>
        {
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    Console.WriteLine($"[TaskStatus.Canceled]");
                    tcs.SetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    tcs.SetResult(task.Result);
                    //Console.WriteLine($"[TaskStatus.RanToCompletion({task.Result})]");
                    break;
                case TaskStatus.Faulted:
                    // SetException will automatically wrap the original AggregateException
                    // in another one. The new wrapper will be removed in TaskAwaiter, leaving
                    // the original intact.
                    Console.WriteLine($"[TaskStatus.Faulted]: {task.Exception?.Message}");
                    tcs.SetException(task.Exception ?? new AggregateException("Task status faulted."));
                    break;
                default:
                    Console.WriteLine($"[TaskStatus: Continuation called illegally.]");
                    tcs.SetException(new InvalidOperationException("Continuation called illegally."));
                    break;
            }
        });

        return tcs.Task;
    }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
    /// <summary>
    /// Attempts to await on the task and catches exception
    /// </summary>
    /// <param name="task">Task to execute</param>
    /// <param name="onException">What to do when method has an exception</param>
    /// <param name="continueOnCapturedContext">If the context should be captured.</param>
    public static async void SafeFireAndForget(this Task task, Action<Exception>? onException = null, bool continueOnCapturedContext = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex) when (onException != null)
        {
            onException.Invoke(ex);
        }
        catch (Exception ex) when (onException == null)
        {
            Console.WriteLine($"SafeFireAndForget: {ex.Message}");
        }
    }
    #endregion

}
