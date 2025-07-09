using System.Timers;
using VegShop.Models;
using VegShop.Utils;

namespace VegShop.Services;
public class CustomerGenerator
{
    private readonly CustomerQueueManager _queueManager;
    private readonly Random _random = new();
    private readonly EpidemicManager _epidemicManager;
    private readonly IConsoleWriter _writer;

    private System.Timers.Timer _timer;
    private bool _isPaused;

    public CustomerGenerator(CustomerQueueManager queueManager, EpidemicManager epidemicManager, IConsoleWriter writer)
    {
        _queueManager = queueManager;
        _epidemicManager = epidemicManager;
        _writer = writer;
        _isPaused = false;

        _timer = new System.Timers.Timer(CalculateIntervalBasedOnRating());
        _timer.Elapsed += OnTimerElapsed!;
        _timer.AutoReset = true;

        _epidemicManager.EpidemicStarted += (_, _) => _isPaused = true;
        _epidemicManager.EpidemicEnded += (_, _) => _isPaused = false;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Interval = CalculateIntervalBasedOnRating();
        if (_isPaused) return;

        var customer = CreateRandomCustomer();
        _queueManager.EnqueueCustomer(customer);
    }

    private int CalculateIntervalBasedOnRating()
    {
        var rating = _queueManager.GetCurrentRating();
        if (rating >= 4.5) return 1000;
        if (rating >= 3.0) return 2000;
        if (rating >= 1.5) return 3000;
        return 5000;
    }

    private Customer CreateRandomCustomer()
    {
        var names = new[]
        {
            "Violet","Banovsha","Ilaha","Naila","Sabuhi",
            "Elmar","Jamila","Parvin","Aysel","Emin",
            "Leyla","Farid","Rana","Kamran","Aysu"
        };
        var vegs = new[]
        {
            "Potato","Tomato","Cucumber","Onion","Pepper",
            "Cabbage","Carrot","Broccoli","Spinach","Eggplant",
            "Zucchini","Lettuce","Cauliflower","Mushroom","Kale"
        };

        var name = names[_random.Next(names.Length)];
        var veg = vegs[_random.Next(vegs.Length)];

        return new Customer(name, veg);
    }
}