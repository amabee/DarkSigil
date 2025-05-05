using DarkSigil.Interface;
using DarkSigil.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarkSigil.Modules
{
  public class Updater : ICommands
  {
    public async void Execute(string[] args)
    {
      bool forceCheck = args.Contains("--force") || args.Contains("-f");
      bool installUpdate = args.Contains("--install") || args.Contains("-i");
      bool rollback = args.Contains("--rollback") || args.Contains("-r");

      if (rollback)
      {
        HandleRollback();
        return;
      }

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
        bool updateAvailable = await VersionUpdater.CheckForUpdates(installUpdate);

        if (updateAvailable && !installUpdate)
        {
          Console.WriteLine();
          Console.WriteLine("To install the update, use: update --install");
        }
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
      }
    }

    private void HandleRollback()
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("Rolling back to previous version...");
      Console.ResetColor();

      try
      {
        bool rollbackSuccess = VersionUpdater.RollbackUpdate();

        if (!rollbackSuccess)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("Rollback failed. Please try again or reinstall the application.");
          Console.ResetColor();
        }
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error during rollback: {ex.Message}");
        Console.ResetColor();
      }
    }
  }
}
