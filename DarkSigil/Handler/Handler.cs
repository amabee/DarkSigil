using DarkSigil.Interface;
using DarkSigil.Modules;
using DarkSigil.Modules.Help;
using DarkSigil.Modules.WhoIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DarkSigil.Handler
{
    public class CommandHandler
    {
        private Dictionary<string, ICommands> commandsDictionary;

        public CommandHandler()
        {

            commandsDictionary = new Dictionary<string, ICommands>(StringComparer.OrdinalIgnoreCase)
            {
                { "clear" , new ClearTerminal()},
                { "cls" , new ClearTerminal()},
                { "exit", new Exit()},
                { "help" , new Help()},
                { "about", new About()},
                {"whois", new WhoIs()},
              
               
            };
        }

        public void Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("██████╗  █████╗ ██████╗ ██╗  ██╗███████╗██╗ ██████╗ ██╗██╗     ");
            Console.WriteLine("██╔══██╗██╔══██╗██╔══██╗██║ ██╔╝██╔════╝██║██╔════╝ ██║██║     ");
            Console.WriteLine("██║  ██║███████║██████╔╝█████╔╝ ███████╗██║██║  ███╗██║██║     ");
            Console.WriteLine("██║  ██║██╔══██║██╔══██╗██╔═██╗ ╚════██║██║██║   ██║██║██║     ");
            Console.WriteLine("██████╔╝██║  ██║██║  ██║██║  ██╗███████║██║╚██████╔╝██║███████╗");
            Console.WriteLine("╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝╚══════╝\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("DarkSigil. Type 'help' for the list of commands. \n");

            while (true)
            {
                Console.Write("DarkSigil> ");
                string input = Console.ReadLine()?.Trim();

                //REGEX 
                string[] parts = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                                   .Cast<Match>()
                                   .Select(m => m.Value.Replace("\"", ""))
                                   .ToArray();

                string[] args = parts.Skip(1).ToArray();

                if (string.IsNullOrEmpty(input))
                    continue;

                string command = parts[0];


                if (commandsDictionary.TryGetValue(command, out ICommands commandObjs))
                {
                    commandObjs.Execute(args);
                }
                else
                {
                    Console.WriteLine($"Command `{command}` is not recognized ");
                }
            }

        }
    }
}
