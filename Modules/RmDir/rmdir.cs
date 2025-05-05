using System;
using DarkSigil.Interface;

namespace DarkSigil.Modules.RmDir

{
  public class RmDir : ICommands
  {
    public void Execute(string[] args)
    {
      if (args.Length < 1)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: rmdir [options] <directory>");
        Console.WriteLine("Options: -r (remove recursively), -v (verbose)");
        Console.ResetColor();
        return;
      }

      bool recursive = false;
      bool verbose = false;

      // PARSE OPTIONS

      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == "r")
        {
          recursive = true;
        }
        else if (args[i] == "v")
        {
          verbose = true;
        }
        else
        {
          string dirPath = args[i];
          try
          {
            if (recursive)
            {
              RemoveDirectoryRecursive(dirPath, verbose);
            }
            else
            {
              RemoveDirectory(dirPath, verbose);
            }

            if (verbose)
            {
              Console.WriteLine($"Removed directory: {dirPath}");
            }
          }
          catch (Exception ex)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error removing directory: {ex.Message}");
            Console.ResetColor();
          }
        }
      }
    }

    private void RemoveDirectory(string dirPath, bool verbose)
    {
      if (Directory.Exists(dirPath))
      {
        try
        {
          Directory.Delete(dirPath, false);
          if (verbose)
          {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Removed empty directory: {dirPath}");
            Console.ResetColor();
          }
        }
        catch (IOException ex)
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"Directory not empty: {ex.Message}");
          Console.ResetColor();
        }
        catch (UnauthorizedAccessException ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Error Removing directory: {ex.Message}");
          Console.ResetColor();
        }
        catch (Exception)
        {
          throw new Exception("An error occurred while removing the directory.");
        }
      }
    }

    private void RemoveDirectoryRecursive(string dirPath, bool verbose)
    {
      if (Directory.Exists(dirPath))
      {
        try
        {
          // First, delete all files in the directory
          var files = Directory.GetFiles(dirPath);
          foreach (var file in files)
          {
            try
            {
              File.Delete(file);
              if (verbose)
              {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Removed file: {file}");
                Console.ResetColor();
              }
            }
            catch (Exception ex)
            {
              Console.ForegroundColor = ConsoleColor.Red;
              Console.WriteLine($"Error removing file {file}: {ex.Message}");
              Console.ResetColor();
            }
          }

          // Then, delete all subdirectories
          var subDirs = Directory.GetDirectories(dirPath);
          foreach (var subDir in subDirs)
          {
            RemoveDirectoryRecursive(subDir, verbose);
          }

          // Finally, remove the main directory
          Directory.Delete(dirPath);
          if (verbose)
          {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Removed directory: {dirPath}");
            Console.ResetColor();
          }
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Error: {ex.Message}");
          Console.ResetColor();
        }
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Directory not found: {dirPath}");
        Console.ResetColor();
      }
    }
  }
}
