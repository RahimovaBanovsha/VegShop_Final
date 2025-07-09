namespace VegShop.Models;
// 3rd:
public enum Condition
{
    Fresh,
    Normal,
    Rotten,
    Toxic
}
public class Vegetable
{
    public string? Name { get; }
    public int Age { get; private set; } = default;
    public double PricePerKg { get; set; }
    public double QuantityKg { get; private set; }
    public Condition Condition { get; private set; } = Condition.Fresh;
    public DateTime AddedTime { get; } = DateTime.Now;
    public event EventHandler? Aged;
    public bool IsInfected { get; }
    public Vegetable(string? name, double quantity)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        QuantityKg = quantity;
        Random rand = new Random();
        IsInfected = rand.Next(1, 101) <= 0.02;
    }
    // 4th:
    public void AgeOneDay()
    {
        Age++;
        Random rand = new Random();
        int chance = rand.Next(1, 101);
        switch (Condition)
        {
            case Condition.Fresh:
                if (chance <= 40) Condition = Condition.Normal;
                break;
            case Condition.Normal:
                if (chance <= 50) Condition = Condition.Rotten;
                break;
            case Condition.Rotten:
                if (chance <= 20) Condition = Condition.Toxic;
                break;
        }
        Aged?.Invoke(this, EventArgs.Empty);
    }
    public bool TryTake(double AmountKg)
    {
        if (QuantityKg >= AmountKg)
        {
            QuantityKg -= AmountKg;
            return true;
        }
        return false;
    }
    override public string ToString()
    {
        return $"{Name} | ({Condition}) - Age: {Age} Days, Added: {AddedTime.ToShortDateString()}";
    }
}