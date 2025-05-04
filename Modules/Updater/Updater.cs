using DarkSigil.Interface;
using DarkSigil.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules
{
  public class Updater : ICommands
  {
    public async void Execute(string[] args)
    {
      bool forceCheck = args.Contains("--force") || args.Contains("-f");

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("Checking for updates...");
      Console.ResetColor();

      if (forceCheck)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Force checking for updates...");
        Console.ResetColor();
      }

      try
      {
        await VersionUpdater.CheckForUpdates();
        
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
      }
    }
  }
}
