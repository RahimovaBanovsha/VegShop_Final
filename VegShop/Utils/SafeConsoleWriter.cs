namespace VegShop.Utils;
public class SafeConsoleWriter : IConsoleWriter
{
    public void WriteLine(string message)
    {
        if (ConsoleControl.IsUserViewing) return; 

        lock (ConsoleControl.LockObj)
        {
            Console.WriteLine(message);
        }
    }

    public void Write(string message)
    {
        if (ConsoleControl.IsUserViewing) return;

        lock (ConsoleControl.LockObj)
        {
            Console.Write(message);
        }
    }

    public string? SafeReadLine()
    {
        if (ConsoleControl.IsUserViewing) return null; 

        lock (ConsoleControl.LockObj)
        {
            return Console.ReadLine();
        }
    }
}