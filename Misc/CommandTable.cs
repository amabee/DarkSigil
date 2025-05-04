using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Misc
{
    public class CommandTable
    {
        public void ShowCommandTable(Dictionary<string, string> commands)
        {
            int consoleWidth = Console.WindowWidth;
            int nameColumnWidth = Math.Min(consoleWidth / 3, 20);
            int descriptionColumnWidth = Math.Min(consoleWidth / 2, 40);

            string border = new string('-', consoleWidth);

            Console.WriteLine(border);
            Console.WriteLine($"| {PadString("Command", nameColumnWidth)} | {PadString("Description", descriptionColumnWidth)} |");
            Console.WriteLine(border);

            foreach (var command in commands)
            {
                Console.WriteLine($"| {PadString(command.Key, nameColumnWidth)} | {PadString(command.Value, descriptionColumnWidth)} |");
            }

            Console.WriteLine(border);
        }

        private string PadString(string str, int width)
        {
            return str.Length > width ? str.Substring(0, width) : str.PadRight(width);
        }
    }
}
