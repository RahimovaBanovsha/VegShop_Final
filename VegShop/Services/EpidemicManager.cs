using System.Timers;
using VegShop.Utils;

namespace VegShop.Services;
public class EpidemicManager
{
    private readonly Random _random = new();
    private readonly System.Timers.Timer _timer;
    private readonly IConsoleWriter _writer;

    public bool IsEpidemicActive { get; private set; }
    private int _remainingDays;

    public event EventHandler? EpidemicStarted;
    public event EventHandler? EpidemicEnded;

    public EpidemicManager(IConsoleWriter writer)
    {
        _writer = writer;
        _timer = new System.Timers.Timer(10000);
        _timer.Elapsed += (s, e) => PassOneDay();
        _timer.AutoReset = true;
        _timer.Start();
    }

    public void StartEpidemic(int durationDays)
    {
        if (IsEpidemicActive) return;

        IsEpidemicActive = true;
        _remainingDays = durationDays;

        _writer.WriteLine($"Epidemic has started. No customer for {durationDays} days!");

        EpidemicStarted?.Invoke(this, EventArgs.Empty);
    }

    public void PassOneDay()
    {
        if (IsEpidemicActive)
        {
            _remainingDays--;
            if (_remainingDays <= 0)
            {
                IsEpidemicActive = false;

                _writer.WriteLine("Epidemic has ended. Customers will return!");

                EpidemicEnded?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            TryStartRandomly();
        }
    }

    public void TryStartRandomly()
    {
        double chance = _random.NextDouble();

        if (chance < 0.05)
        {
            int randomDays = _random.Next(5, 20);
            StartEpidemic(randomDays);
        }
    }

    public void StartManually()
    {
        if (!IsEpidemicActive)
        {
            int randomDays = _random.Next(5, 20);
            StartEpidemic(randomDays);
        }
        else
        {
            _writer.WriteLine("Epidemic is already active!");
        }
    }
}