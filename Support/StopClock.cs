using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightClub;

/// <summary>
/// Utility class to assist with performance timing.
/// </summary>
public class StopClock : IDisposable
{
    string m_title;
    bool m_console;
    ConsoleColor m_color;
    bool m_disposed;
    System.Diagnostics.Stopwatch m_watch;

    public StopClock(string title = "Default", ConsoleColor color = ConsoleColor.DarkYellow, bool console = true)
    {
        m_watch = System.Diagnostics.Stopwatch.StartNew();
        m_title = title;
        m_console = console;
        m_color = color;
    }

    public System.Diagnostics.Stopwatch Stop()
    {
        if (m_watch != null)
            m_watch.Stop();

        return m_watch;
    }

    public void Print()
    {
        if (m_watch != null)
        {
            if (m_console)
            {
                Console.ForegroundColor = m_color;
                double result = (double)m_watch.ElapsedMilliseconds / 1000.0;
                if (Console.CursorLeft > 0) { Console.WriteLine(); } // if there's already data on the line then add a CRLF
                Console.WriteLine($"> {m_title}: Execution lasted {m_watch.ElapsedMilliseconds} ms ({result.ToString("0.0")} sec)");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(new string('=', 70));
                double result = (double)m_watch.ElapsedMilliseconds / 1000.0;
                System.Diagnostics.Debug.WriteLine($"> {m_title}: Execution lasted {m_watch.ElapsedMilliseconds} ms ({result.ToString("0.0")} sec)");
                System.Diagnostics.Debug.WriteLine(new string('=', 70));
            }
        }
    }

    /// <summary>
    /// public Dispose follows the recommended pattern and suppresses finalization
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allow derived types to override disposal behavior if needed
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed) return;
        m_disposed = true;

        if (disposing)
        {
            Stop();
            Print();
        }
    }
}
