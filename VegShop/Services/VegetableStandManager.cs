using VegShop.Models;
using VegShop.Utils;
using System.Timers;

namespace VegShop.Services;

public class VegetableStandManager
{
    private readonly IConsoleWriter _writer;

    public VegetableStandManager(IConsoleWriter writer)
    {
        _writer = writer;
    }

    public double TotalMoneyEarned;

    private Dictionary<string, Stack<Vegetable>> _stands = new();
    private System.Timers.Timer? _agingTimer;
    private bool _expandedOnce = false;
    private double _lastThresholdChecked = 0;
    private const double ThresholdKg = 10.0;

    public void AddVegetable(Vegetable vegetable)
    {
        if (!_stands.ContainsKey(vegetable.Name!))
        {
            _stands[vegetable.Name!] = new Stack<Vegetable>();
        }
        _stands[vegetable.Name!].Push(vegetable);
    }

    public void ExpandAssortmentIfPossible()
    {
        if (TotalMoneyEarned >= 100 && !_expandedOnce)
        {
            _expandedOnce = true;

            _writer.WriteLine("\nSales milestone reached! New vegetables unlocked!\n");

            var onionDelivery = new List<Vegetable>
            {
                new Vegetable("Onion", 3.0),
                new Vegetable("Onion", 2.5)
            };
            var pepperDelivery = new List<Vegetable>
            {
                new Vegetable("Pepper", 2.0),
                new Vegetable("Pepper", 2.5)
            };
            var cabbageDelivery = new List<Vegetable>
            {
                new Vegetable("Cabbage", 4.0)
            };

            AddNewDelivery("Onion", onionDelivery);
            AddNewDelivery("Pepper", pepperDelivery);
            AddNewDelivery("Cabbage", cabbageDelivery);

            _writer.WriteLine("New vegetables added: Onion, Pepper, and Cabbage!");
            _writer.WriteLine("Customers will start requesting them from now on.");
        }
    }

    public void AddEarnings(double amount)
    {
        TotalMoneyEarned += amount;
    }

    public void CheckAndAddBonusVegetables(double currentTotalSales)
    {
        if (currentTotalSales - _lastThresholdChecked >= ThresholdKg)
        {
            _writer.WriteLine("High sales achieved! Bonus delivery triggered.");

            var bonusDelivery = new List<Vegetable>
            {
                new Vegetable("Eggplant", 2.5),
                new Vegetable("Zucchini", 2.5)
            };

            AddNewDelivery("Eggplant", new List<Vegetable> { bonusDelivery[0] });
            AddNewDelivery("Zucchini", new List<Vegetable> { bonusDelivery[1] });

            _lastThresholdChecked += ThresholdKg;
        }
    }

    public void AutoRestockIfRatingLow(double currentRating)
    {
        if (currentRating < 1.0)
        {
            _writer.WriteLine(" Rating is too low! Bringing new vegetables to recover...");

            var autoDelivery = new List<Vegetable>
            {
                new Vegetable("Carrot", 3.0),
                new Vegetable("Spinach", 2.0),
                new Vegetable("Broccoli", 2.5)
            };

            AddNewDelivery("Carrot", new List<Vegetable> { autoDelivery[0] });
            AddNewDelivery("Spinach", new List<Vegetable> { autoDelivery[1] });
            AddNewDelivery("Broccoli", new List<Vegetable> { autoDelivery[2] });

            _writer.WriteLine("New vegetables delivered! Rating may improve as more customers arrive.");
        }
    }

    public void ShowShopStatus(CustomerQueueManager queueManager, EpidemicManager epidemicManager)
    {
        Console.Clear();
        _writer.WriteLine("========= SHOP STATUS PANEL =========");

        _writer.WriteLine($"Snapshot Time        : {DateTime.Now}");
        _writer.WriteLine($"Current Rating       : {queueManager.GetCurrentRating():F1}");
        _writer.WriteLine($"Customers in Queue   : {queueManager.GetQueueLength()}");
        _writer.WriteLine($"Total Money Earned   : {TotalMoneyEarned:F2} AZN");
        _writer.WriteLine($"Epidemic Active?     : {(epidemicManager.IsEpidemicActive ? "Yes" : "No")}");

        int fresh = 0, normal = 0, rotten = 0, toxic = 0;
        double totalKg = 0;

        foreach (var stack in _stands.Values)
        {
            foreach (var veg in stack)
            {
                totalKg += veg.QuantityKg;
                switch (veg.Condition)
                {
                    case Condition.Fresh: 
                        fresh++; 
                        break;
                    case Condition.Normal: 
                        normal++; 
                        break;
                    case Condition.Rotten: 
                        rotten++; 
                        break;
                    case Condition.Toxic: 
                        toxic++; 
                        break;
                }
            }
        }

        _writer.WriteLine($"Total Vegetables     : {totalKg:F2} kg");
        _writer.WriteLine($"Fresh: {fresh}, Normal: {normal}, Rotten: {rotten}, Toxic: {toxic}");
        _writer.WriteLine("======================================");

        _writer.WriteLine("\nPress ENTER to return to main menu...");
        Console.ReadLine();
    }

    public void ShowStandStatus()
    {
        foreach (var stand in _stands)
        {
            _writer.WriteLine($"Stand: {stand.Key}");
            foreach (var veg in stand.Value)
            {
                _writer.WriteLine($" {veg}");
            }
        }
    }

    public Stack<Vegetable>? GetStand(string name)
    {
        return _stands.TryGetValue(name, out var stack) ? stack : null;
    }

    public bool ContainsVegetable(string name)
    {
        return _stands.ContainsKey(name) && _stands[name].Count > 0;
    }

    public void AddNewDelivery(string VegName, List<Vegetable> NewDelivery)
    {
        double DiscardedKg = 0;
        double DiscardedInfectedKg = 0;
        List<Vegetable> HealthyNewDelivery = new();

        foreach (var veg in NewDelivery)
        {
            if (veg.IsInfected)
            {
                DiscardedInfectedKg += veg.QuantityKg;
            }
            else
            {
                HealthyNewDelivery.Add(veg);
            }
        }

        List<Vegetable> FreshOld = new();
        List<Vegetable> NormalOld = new();

        if (_stands.TryGetValue(VegName, out var OldStack))
        {
            while (OldStack.Count > 0)
            {
                var veg = OldStack.Pop();
                switch (veg.Condition)
                {
                    case Condition.Rotten:
                    case Condition.Toxic:
                        DiscardedKg += veg.QuantityKg;
                        break;
                    case Condition.Normal:
                        NormalOld.Add(veg);
                        break;
                    case Condition.Fresh:
                        FreshOld.Add(veg);
                        break;
                }
            }
        }

        Stack<Vegetable> UpdatedStack = new();
        for (int i = HealthyNewDelivery.Count - 1; i >= 0; i--)
        {
            UpdatedStack.Push(HealthyNewDelivery[i]);
        }
        foreach (var veg in FreshOld)
        {
            UpdatedStack.Push(veg);
        }
        foreach (var veg in NormalOld)
        {
            UpdatedStack.Push(veg);
        }
        _stands[VegName] = UpdatedStack;

        _writer.WriteLine($"New delivery: {HealthyNewDelivery.Count} of {VegName} added.");
        _writer.WriteLine($"{DiscardedInfectedKg}kg of {VegName} discarded due to infection.");
        _writer.WriteLine($"{DiscardedKg}kg of {VegName} discarded due to spoilage.");
    }

    public void AgeAllVegetablesOneDay()
    {
        foreach (var stand in _stands.Values)
        {
            foreach (var veg in stand)
            {
                veg.AgeOneDay();
            }
        }
    }

    public void StartAgingTimer()
    {
        _agingTimer = new System.Timers.Timer(20000);
        _agingTimer.Elapsed += (sender, e) => AgeAllVegetablesOneDay();
        _agingTimer.AutoReset = true;
        _agingTimer.Enabled = true;
    }

    public double GetTotalMoneyEarned() => TotalMoneyEarned;
}