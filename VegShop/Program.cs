using VegShop.Utils;
using VegShop.UI;
namespace VegShop;
class Program
{
    static void Main()
    {
        var writer = new SafeConsoleWriter();
        var menu = ProgramReferences.CreateMenuService(writer, new Random());
        menu.Run();
    }
}