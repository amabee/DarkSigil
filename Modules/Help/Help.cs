using DarkSigil.Interface;
using DarkSigil.Misc;


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
                { "ls", "List the files, directories and subdirectories"},
                { "cat", "Reads and displayes text files"},
                { "ifconfig", "Displays the network configuration"},
                { "cd", "Changes working directory" },
                {"ping", "check the internet connection" },
                { "rm", "removes a file or a directory"},
                {"darksigil update", "updates the program to the latest version"},
                {"grep", "Searches for a pattern in a file"},
                {"rmdir", "Removes a directory"},
                {"mkdir", "Creates a directory"},
                {"mv", "Moves a file or a directory"},
                // {"cp", "Copies a file or a directory"},
                // {"touch", "Creates an empty file"},
                // {"echo", "Displays a message"}
            };

      var sortedCommands = commands.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
      return sortedCommands;
    }
  }

}
