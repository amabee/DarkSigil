using DarkSigil.Interface;
using DarkSigil.Modules;
using DarkSigil.Modules.Cal;
using DarkSigil.Modules.Cat;
using DarkSigil.Modules.ChangeDirectory;
using DarkSigil.Modules.Help;
using DarkSigil.Modules.IfConfig;
using DarkSigil.Modules.LS;
using DarkSigil.Modules.Mkdir;
using DarkSigil.Modules.ping;
using DarkSigil.Modules.Pwd;
using DarkSigil.Modules.rm;
using DarkSigil.Modules.WhoAmI;
using DarkSigil.Modules.WhoIs;
using DarkSigil.Utils;
using System.Text.RegularExpressions;



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
                {"whoami" , new WhoAmI()},
                { "pwd" , new PWD()},
                { "cal", new cal() },
                { "cd", new ChangeDirectory()},
                { "ls", new LS()},
                { "cat", new CAT()},
                { "ifconfig", new IfConfig()},
                { "ipconfig", new IfConfig()},
                {"ping", new PING() },
                { "rm", new Rm()},
                {"update", new Updater()},
                {"mkdir", new Mkdir()},

            };
    }

    public void Run()
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████\n\n");
      Console.WriteLine("       @@@@@@@@@@@@@@@@@@");
      Console.WriteLine("     @@@@@@@@@@@@@@@@@@@@@@@               ██████╗  █████╗ ██████╗ ██╗  ██╗███████╗██╗ ██████╗ ██╗██╗     ");
      Console.WriteLine("   @@@@@@@@@@@@@@@@@@@@@@@@@@@             ██╔══██╗██╔══██╗██╔══██╗██║ ██╔╝██╔════╝██║██╔════╝ ██║██║     ");
      Console.WriteLine("  @@@@@@@@@@@@@@@@@@@@@@@@@@@@@            ██║  ██║███████║██████╔╝█████╔╝ ███████╗██║██║  ███╗██║██║     ");
      Console.WriteLine(" @@@@@@@@@@@@@@@/      \\@@@/   @           ██║  ██║██╔══██║██╔══██╗██╔═██╗ ╚════██║██║██║   ██║██║██║     ");
      Console.WriteLine("@@@@@@@@@@@@@@@@\\      @@  @___@           ██████╔╝██║  ██║██║  ██║██║  ██╗███████║██║╚██████╔╝██║███████╗");
      Console.WriteLine("@@@@@@@@@@@@@ @@@@@@@@@@  | \\@@@@@         ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝╚══════╝");
      Console.WriteLine("@@@@@@@@@@@@@ @@@@@@@@@\\__@_/@@@@@");
      Console.WriteLine(" @@@@@@@@@@@@@@@/,/,/./'/_|.\\'\\,\\");
      Console.WriteLine("    @@@@@@@@@@@@@|  | | | | | | | |");
      Console.WriteLine("                 \\_|_|_|_|_|_|_|_| \n\n");
      Console.WriteLine("██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████");

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
