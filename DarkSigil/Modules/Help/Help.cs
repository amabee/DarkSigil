using DarkSigil.Interface;
using DarkSigil.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.Help
{
    public class Help : ICommands
    {
        public void Execute(string[] args)
        {

            CommandTable commandTable = new CommandTable();
            Dictionary<string, string> commands = GetCommands();
            commandTable.ShowCommandTable(commands);
        }

        private Dictionary<string, string> GetCommands()
        {
            var commands = new Dictionary<string, string>
            {
                { "clear", "Clears the terminal." },
                { "cls", "Clears the terminal." },
                { "exit", "Exits the application." },
                { "help", "Displays this help message." },
                { "about", "Displays the program information." },
                { "whois", "WhoIs Domain Lookup." },
                { "whoami", "Show current user"},
                {"pwd", "Shows the current working directory" },
                { "cal" , "Shows the calendar"},
            };

            var sortedCommands = commands.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
            return sortedCommands;
        }
    }
   
}
