namespace VegShop.Utils;

public interface IConsoleWriter
{
    void WriteLine(string message);

    string? SafeReadLine();

    void Write(string message);
}

public static class ConsoleControl
{
    public static bool IsUserViewing = false;
    public static readonly object LockObj = new object();
}