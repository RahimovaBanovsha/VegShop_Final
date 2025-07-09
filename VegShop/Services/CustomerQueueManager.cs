using VegShop.Models;
using VegShop.Utils;
namespace VegShop.Services;
public class CustomerQueueManager
{
    private readonly IConsoleWriter _writer;
    private readonly Queue<Customer> _queue = new();
    private readonly EpidemicManager _epidemicManager;
    private readonly Random _random = new();    
    
    private int _customerCount;
    private int _toxicFledCount;
    private int _rottenDiscarded;
    private int _successfulSales;
    private double _totalSales = 0;
    private double _rating = 5.0;
    private readonly VegetableStandManager? _standManager;

    public CustomerQueueManager(VegetableStandManager? standManager, EpidemicManager epidemicManager, IConsoleWriter writer)
    {
        _standManager = standManager;
        _epidemicManager = epidemicManager;
        _writer = writer;
    }

    public void EnqueueCustomer(Customer customer)
    {
        _queue.Enqueue(customer);
    }

    public void ProcessNextCustomer()
    {
        if (_queue.Count == 0) return;

        var customer = _queue.Dequeue();

        if (_epidemicManager.IsEpidemicActive)
        {
            if (!ConsoleControl.IsUserViewing)
                _writer.WriteLine($"Customer {customer.Name} wanted {customer.DesiredVegetable}, but service is unavailable due to an epidemic.");
            return;
        }

        var stand = _standManager.GetStand(customer.DesiredVegetable);

        if (!ConsoleControl.IsUserViewing)
            _writer.WriteLine($"Processing Customer: {customer.Name} | Wants: {customer.DesiredVegetable}");

        if (!_standManager.ContainsVegetable(customer.DesiredVegetable) || stand == null || stand.Count == 0)
        {
            if (!ConsoleControl.IsUserViewing)
                _writer.WriteLine($"{customer.DesiredVegetable} is not available! Rating -0.1");
            _rating = Math.Max(0, _rating - 0.1);
            return;
        }

        var topVegetable = stand.Peek();

        switch (topVegetable.Condition)
        {
            case Condition.Toxic:
                stand.Pop();
                if (!ConsoleControl.IsUserViewing)
                    _writer.WriteLine($"{customer.Name} picked a toxic {topVegetable.Name}. Fled without paying. Rating -0.5.");
                _toxicFledCount++;
                _customerCount++;
                _rating = Math.Max(0, _rating - 0.5);
                break;

            case Condition.Rotten:
                stand.Pop();
                if (!ConsoleControl.IsUserViewing)
                    _writer.WriteLine($"{customer.Name} picked a rotten {topVegetable.Name}. Threw it away.");
                _rottenDiscarded++;
                _customerCount++;
                break;

            case Condition.Fresh:
            case Condition.Normal:
                double desiredKg = Math.Round(_random.NextDouble() * 4 + 1, 2);

                if (topVegetable.TryTake(desiredKg))
                {
                    if (!ConsoleControl.IsUserViewing)
                        _writer.WriteLine($"{customer.Name} bought {desiredKg}kg of {topVegetable.Name}.");
                    
                    _totalSales += desiredKg;
                    _successfulSales++;
                    _customerCount++;
                    double cost = desiredKg * topVegetable.PricePerKg;
                    _standManager.AddEarnings(cost);
                }
                else
                {
                    double remaining = topVegetable.QuantityKg;
                    string response = _random.Next(2) == 0 ? "Y" : "N";

                    if (!ConsoleControl.IsUserViewing)
                    {
                        _writer.WriteLine($"Only {remaining}kg of {topVegetable.Name} is available.");
                        _writer.WriteLine($"Customer {customer.Name} decision: {(response == "Y" ? "Yes" : "No")}");
                    }

                    if (response == "Y")
                    {
                        topVegetable.TryTake(remaining);
                        stand.Pop();
                        if (!ConsoleControl.IsUserViewing)
                            _writer.WriteLine($"{customer.Name} accepted and bought {remaining}kg of {topVegetable.Name}.");
                        _totalSales += remaining;
                        _successfulSales++;
                        _customerCount++;

                        double cost = remaining * topVegetable.PricePerKg;
                        _standManager.AddEarnings(cost);
                    }
                    else
                    {
                        if (!ConsoleControl.IsUserViewing)
                            _writer.WriteLine($"{customer.Name} refused the partial quantity. No sale made.");
                    }
                }
                break;
        }
    }

    public void ExportStatisticsToFile(string path)
    {
        var lines = new[] {
            "--- Shop Statistics Report ---",
            $"Total Customers       : {_customerCount}",
            $"Successful Sales      : {_successfulSales}",
            $"Rotten Discards       : {_rottenDiscarded}",
            $"Toxic Escapes         : {_toxicFledCount}",
            $"Total Sales (kg)      : {_totalSales:F2}",
            $"Current Rating        : {_rating:F2}",
            $"Report Date           : {DateTime.Now}"
        };
        File.WriteAllLines(path, lines);
    }

    public double GetCurrentRating() => _rating;
    public int GetQueueLength() => _queue.Count;
    public int GetCustomerCount() => _customerCount;
    public double GetTotalSalesKg() => _totalSales;
}