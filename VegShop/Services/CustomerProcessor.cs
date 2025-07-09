using VegShop.Utils;

namespace VegShop.Services;
public class CustomerProcessor
{
    private readonly CustomerQueueManager _queueManager;
    private readonly IConsoleWriter _writer;
    private readonly System.Timers.Timer _processTimer;

    public CustomerProcessor(CustomerQueueManager queueManager, IConsoleWriter writer)
    {
        _queueManager = queueManager;
        _writer = writer;

        _processTimer = new System.Timers.Timer(3000); 
        _processTimer.Elapsed += (s, e) => _queueManager.ProcessNextCustomer();
        _processTimer.AutoReset = true;
    }

    public void Start()
    {
        _writer.WriteLine("Customer processing started.");
        _processTimer.Start();
    }

    public void Stop()
    {
        _writer.WriteLine("Customer processing stopped.");
        _processTimer.Stop();
    }
}