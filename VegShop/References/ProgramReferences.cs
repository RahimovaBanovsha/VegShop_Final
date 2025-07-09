using System;
using System.Collections.Generic;
using System.Timers;
using VegShop.Models;
using VegShop.Services;
using VegShop.Utils;
using VegShop.UI;

namespace VegShop;
public static class ProgramReferences
{
    public static MenuService CreateMenuService(IConsoleWriter writer, Random rand)
    {

        var epidemic = new EpidemicManager(writer);
        var stand = new VegetableStandManager(writer);
        var queue = new CustomerQueueManager(stand, epidemic, writer);
        var generator = new CustomerGenerator(queue, epidemic, writer);
        var processor = new CustomerProcessor(queue, writer);

        // Starting background timers:
        stand.StartAgingTimer();
        generator.Start();
        processor.Start();

        // Auto-restock:
        SetupAutoRestock(stand, rand, writer);

        // Initial stock:
        SetupInitialStock(stand);

        return new MenuService(stand, queue, epidemic, generator, processor);
    }

    private static void SetupAutoRestock(VegetableStandManager stand, Random rand, IConsoleWriter writer)
    {
        var restock = new System.Timers.Timer(30000);

        restock.Elapsed += (_, __) =>
        {
            var vegs = new[] { "Potato","Tomato","Cucumber","Onion","Pepper",
                "Cabbage","Carrot","Broccoli","Spinach","Eggplant",
                "Zucchini","Lettuce","Cauliflower","Mushroom","Kale" };

            var vn = vegs[rand.Next(vegs.Length)];
            var qty = Math.Round(rand.NextDouble() * 10 + 5, 1);
            stand.AddNewDelivery(vn, new List<Vegetable> { new Vegetable(vn, qty) });
            
            if (!ConsoleControl.IsUserViewing)
                writer.WriteLine($"Auto-restocked {qty}kg of {vn}.");
        };
        restock.AutoReset = true;
        restock.Start();
    }

    private static void SetupInitialStock(VegetableStandManager stand)
    {
        var initial = new[]
        {
            "Potato","Tomato","Cucumber","Onion","Pepper",
            "Cabbage","Carrot","Broccoli","Spinach","Eggplant",
            "Zucchini","Lettuce","Cauliflower","Mushroom","Kale"
        };
        foreach (var name in initial)
            stand.AddNewDelivery(name, new List<Vegetable> { new Vegetable(name, 20.0) });
    }
}