using System;
using DarkSigil.Interface;

namespace DarkSigil.Modules.Mkdir
{
  public class Mkdir : ICommands
  {
    public void Execute(string[] args)
    {
      if (args.Length == 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: mkdir [-p] <directory_name>");
        Console.ResetColor();
        return;
      }

      bool createParentDirectories = false;
      var dirNames = new List<string>();

      foreach (var arg in args)
      {
        if (arg == "-p" || arg == "--parents")
        {
          createParentDirectories = true;
        }
        else if (arg.StartsWith("-"))
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Unknown option: {arg}");
          Console.ResetColor();
          return;
        }
        else if (Directory.Exists(arg))
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Directory already exists: {arg}");
          Console.ResetColor();
        }
        else
        {
          dirNames.Add(arg);
        }
      }

      foreach (var dir in dirNames)
      {
        try
        {
          if (createParentDirectories)
          {
            Directory.CreateDirectory(dir);
          }
          else
          {
            if (Directory.Exists(dir))
            {
              Console.ForegroundColor = ConsoleColor.Red;
              Console.WriteLine($"Directory already exists: {dir}");
              Console.ResetColor();
              return;
            }
            else
            {
              var parentDir = Path.GetDirectoryName(dir);
              if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
              {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"mkdir: cannot create directory '{dir}': No such file or directory");
                Console.ResetColor();
                return;
              }
              Directory.CreateDirectory(dir);
            }
          }
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"Directory created: {dir}");
          Console.ResetColor();
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Error creating directory '{dir}': {ex.Message}");
          Console.ResetColor();
        }
      }
    }
  }
}


