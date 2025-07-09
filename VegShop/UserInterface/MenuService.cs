using System.Threading;
using VegShop.Models;
using VegShop.Services;
using VegShop.Utils;
namespace VegShop.UI;
public class MenuService
{
    private readonly VegetableStandManager _stand;
    private readonly CustomerQueueManager _queue;
    private readonly EpidemicManager _epidemic;
    private readonly CustomerGenerator _gen;
    private readonly CustomerProcessor _proc;
    private readonly string[] _options = {
        "View Shop Status",
        "Run Customer Simulation",
        "Export Report",
        "Review Inventory (Stacks)",
        "Exit"
    };

    public MenuService(
        VegetableStandManager standManager,
        CustomerQueueManager queueManager,
        EpidemicManager epidemicManager,
        CustomerGenerator generator,
        CustomerProcessor processor)
    {
        _stand = standManager;
        _queue = queueManager;
        _epidemic = epidemicManager;
        _gen = generator;
        _proc = processor;
    }

    public void Run()
    {
        while (true)
        {
            // pausing background work!
            _gen.Stop();
            _proc.Stop();

            int choice = PromptWithArrows(_options);

            switch (choice)
            {
                case 0: // View Shop Status
                    Console.Clear();
                    ConsoleControl.IsUserViewing = false;
                    _stand.ShowShopStatus(_queue, _epidemic);
                    Console.ReadLine();
                    break;

                case 1: // Run Customer Simulation
                    Console.Clear();
                    Console.WriteLine("========= CUSTOMER SIMULATION =========");
                    Console.WriteLine("Press ENTER to stop...");
                    ConsoleControl.IsUserViewing = false;
                    _gen.Start();
                    _proc.Start();
                    Console.ReadLine();
                    break;

                case 2: // Export Report
                    var fn = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    _queue.ExportStatisticsToFile(fn);
                    Console.WriteLine($"Report saved as {fn}");
                    Console.ReadLine();
                    break;

                case 3: // Review Inventory
                    Console.Clear();
                    ConsoleControl.IsUserViewing = false;
                    _stand.ShowStandStatus();
                    Console.ReadLine();
                    break;

                case 4: // Exit
                    return;
            }
        }
    }

    private int PromptWithArrows(string[] options)
    {
        int selected = 0;
        ConsoleKey key;

        do
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========= VEGETABLE SHOP SIMULATION =========");
            Console.ResetColor();
            Console.WriteLine();
            for (int i = 0; i < options.Length; i++)
            {
                string prefix = (i == selected) ? "-> " : "   ";
                if (i == selected)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine(prefix + options[i]);
                Console.ResetColor();
            }

            var keyInfo = Console.ReadKey(true);
            key = keyInfo.Key;

            if (key == ConsoleKey.UpArrow)
                selected = (selected - 1 + options.Length) % options.Length;
            else if (key == ConsoleKey.DownArrow)
                selected = (selected + 1) % options.Length;

        } while (key != ConsoleKey.Enter);

        return selected;
    }
}