namespace VegShop.Models;
public class Customer
{
    public string Name { get; }
    public string DesiredVegetable { get; }
    public Customer(string name, string desiredVegetable)
    {
        Name = name;
        DesiredVegetable = desiredVegetable;
    }
    override public string ToString()
    {
        return $"Customer Name: {Name} | Wants: {DesiredVegetable}";
    }
}
