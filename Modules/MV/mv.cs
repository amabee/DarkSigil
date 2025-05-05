using System;
using System.IO;
using DarkSigil.Interface;

namespace DarkSigil.Modules.Mv
{
  public class Mv : ICommands
  {
    public void Execute(string[] args)
    {
      if (args.Length < 2)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: mv [options] <source> <destination>");
        Console.WriteLine("Options: -f (force overwrite), -v (verbose)");
        Console.ResetColor();
        return;
      }

      bool forceOverwrite = false;
      bool verbose = false;

      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == "-f")
        {
          forceOverwrite = true;
        }
        else if (args[i] == "-v")
        {
          verbose = true;
        }
        else
        {
          string source = args[i];
          string destination = args[++i];

          MoveItem(source, destination, forceOverwrite, verbose);
        }
      }
    }


    private void MoveItem(string source, string destination, bool forceOverwrite, bool verbose)
    {
      if (File.Exists(source))
      {
        MoveFile(source, destination, forceOverwrite, verbose);
      }
      else if (Directory.Exists(source))
      {
        MoveDirectory(source, destination, forceOverwrite, verbose);
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: Source not found: {source}");
        Console.ResetColor();
      }
    }

    private void MoveFile(string source, string destination, bool forceOverwrite, bool verbose)
    {
      try
      {
        if (File.Exists(destination) && !forceOverwrite)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Error: Destination file already exists: {destination}");
          Console.ResetColor();
          return;
        }

        FileInfo sourceFileInfo = new FileInfo(source);
        if (sourceFileInfo.IsReadOnly)
        {
          sourceFileInfo.IsReadOnly = false;
        }

        File.Move(source, destination);

        if (verbose)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"File moved: {source} -> {destination}");
          Console.ResetColor();
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error moving file: Unauthorized access. {ex.Message}");
        Console.ResetColor();
      }
      catch (IOException ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error moving file: {ex.Message}");
        Console.ResetColor();
      }
    }

    private void MoveDirectory(string source, string destination, bool forceOverwrite, bool verbose)
    {
      try
      {
        if (Directory.Exists(destination) && !forceOverwrite)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Error: Destination directory already exists: {destination}");
          Console.ResetColor();
          return;
        }

        if (destination.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
          destination = destination.TrimEnd(Path.DirectorySeparatorChar);
        }

        DirectoryInfo sourceDirInfo = new DirectoryInfo(source);
        if (sourceDirInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
        {
          sourceDirInfo.Attributes &= ~FileAttributes.ReadOnly;
        }


        Directory.Move(source, destination);

        if (verbose)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"Directory moved: {source} -> {destination}");
          Console.ResetColor();
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error moving directory: Unauthorized access. {ex.Message}");
        Console.ResetColor();
      }
      catch (IOException ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error moving directory: {ex.Message}");
        Console.ResetColor();
      }
    }
  }
}
