using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DarkSigil.Interface;

namespace DarkSigil.Modules
{
  public class Cp : ICommands
  {
    public void Execute(string[] args)
    {
      if (args.Length < 2)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: cp [options] <source> <destination>");
        Console.WriteLine("Options: -r (recursive), -f (force overwrite), -i (interactive), -n (no overwrite), -v (verbose), -p (preserve timestamps)");
        Console.ResetColor();
        return;
      }

      // Options
      bool recursive = false;
      bool force = false;
      bool interactive = false;
      bool noClobber = false;
      bool verbose = false;
      bool preserve = false;

      var files = args.Where(arg => !arg.StartsWith("-")).ToList();
      var options = args.Except(files).ToList();

      // Parse flags
      foreach (var opt in options)
      {
        if (opt.Contains("r")) recursive = true;
        if (opt.Contains("f")) force = true;
        if (opt.Contains("i")) interactive = true;
        if (opt.Contains("n")) noClobber = true;
        if (opt.Contains("v")) verbose = true;
        if (opt.Contains("p")) preserve = true;
      }

      if (files.Count < 2)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Please specify a source and destination.");
        Console.ResetColor();
        return;
      }

      string source = files[0];
      string destination = files[1];

      try
      {
        // Handle wildcards (basic globbing)
        if (source.Contains("*") || source.Contains("?"))
        {
          string? dir = Path.GetDirectoryName(source);
          string pattern = Path.GetFileName(source);
          dir = string.IsNullOrEmpty(dir) ? Directory.GetCurrentDirectory() : dir;
          var matches = Directory.GetFiles(dir, pattern);
          foreach (var match in matches)
          {
            string destPath = Directory.Exists(destination)
              ? Path.Combine(destination, Path.GetFileName(match))
              : destination;
            CopyFile(match, destPath, force, interactive, noClobber, verbose, preserve);
          }
        }
        else if (Directory.Exists(source))
        {
          if (!recursive)
          {
            Console.WriteLine("Omitting directory. Use -r to copy recursively.");
            return;
          }

          CopyDirectory(source, destination, force, interactive, noClobber, verbose, preserve);
        }
        else if (File.Exists(source))
        {
          CopyFile(source, destination, force, interactive, noClobber, verbose, preserve);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Source '{source}' not found.");
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

    void CopyFile(string source, string destination, bool force, bool interactive, bool noClobber, bool verbose, bool preserve)
    {
      if (File.Exists(destination))
      {
        if (noClobber)
        {
          if (verbose) Console.WriteLine($"Skipping {destination} (already exists)");
          return;
        }

        if (interactive)
        {
          Console.Write($"overwrite {destination}? (y/n) ");
          var key = Console.ReadKey();
          Console.WriteLine();
          if (key.KeyChar != 'y' && key.KeyChar != 'Y') return;
        }

        if (!force)
        {
          File.Delete(destination);
        }
      }

      File.Copy(source, destination, true);

      if (preserve)
      {
        var srcInfo = new FileInfo(source);
        File.SetLastWriteTime(destination, srcInfo.LastWriteTime);
        File.SetCreationTime(destination, srcInfo.CreationTime);
        File.SetLastAccessTime(destination, srcInfo.LastAccessTime);
      }

      if (verbose)
      {
        Console.WriteLine($"{source} -> {destination}");
      }
    }

    void CopyDirectory(string sourceDir, string destinationDir, bool force, bool interactive, bool noClobber, bool verbose, bool preserve)
    {
      Directory.CreateDirectory(destinationDir);

      foreach (var file in Directory.GetFiles(sourceDir))
      {
        string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
        CopyFile(file, destFile, force, interactive, noClobber, verbose, preserve);
      }

      foreach (var subDir in Directory.GetDirectories(sourceDir))
      {
        string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
        CopyDirectory(subDir, destSubDir, force, interactive, noClobber, verbose, preserve);
      }

      if (preserve)
      {
        var srcInfo = new DirectoryInfo(sourceDir);
        Directory.SetCreationTime(destinationDir, srcInfo.CreationTime);
        Directory.SetLastWriteTime(destinationDir, srcInfo.LastWriteTime);
        Directory.SetLastAccessTime(destinationDir, srcInfo.LastAccessTime);
      }
    }
  }
}
