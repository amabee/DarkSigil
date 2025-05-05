using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DarkSigil.Interface;

namespace DarkSigil.Modules.Grep
{
  public class Grep : ICommands
  {
    public void Execute(string[] args)
    {
      // Validate argument count
      if (args.Length < 2)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: grep [options] <pattern> <file>");
        Console.WriteLine("Options: -i (ignore case), -n (show line numbers), -v (invert match), -c (count matches), -l (list filenames), -h (suppress filenames), -w (whole word match), -x (whole line match), -r (recursive), -e (multiple patterns), -f (patterns from file), -E (extended regex), -o (only matched parts), -A (n lines after), -B (n lines before), -C (n lines before and after)");
        Console.ResetColor();
        return;
      }

      // Flags and settings
      bool ignoreCase = false;
      bool showLineNumbers = false;
      bool invertMatch = false;
      bool countOnly = false;
      bool listFilenames = false;
      bool suppressFilenames = false;
      bool wholeWordMatch = false;
      bool wholeLineMatch = false;
      bool recursiveSearch = false;
      bool extendedRegex = false;
      bool onlyMatchedParts = false;
      int afterLines = 0;
      int beforeLines = 0;
      int contextLines = 0;

      List<string> patterns = new List<string>();
      List<string> filePaths = new List<string>();

      // Parse arguments and flags
      for (int i = 0; i < args.Length; i++)
      {
        var arg = args[i];
        switch (arg)
        {
          case "-i": ignoreCase = true; break;
          case "-n": showLineNumbers = true; break;
          case "-v": invertMatch = true; break;
          case "-c": countOnly = true; break;
          case "-l": listFilenames = true; break;
          case "-h": suppressFilenames = true; break;
          case "-w": wholeWordMatch = true; break;
          case "-x": wholeLineMatch = true; break;
          case "-r": case "-R": recursiveSearch = true; break;
          case "-E": extendedRegex = true; break;
          case "-o": onlyMatchedParts = true; break;

          // Context line flags
          case "-A": if (i + 1 < args.Length && int.TryParse(args[++i], out int a)) afterLines = a; break;
          case "-B": if (i + 1 < args.Length && int.TryParse(args[++i], out int b)) beforeLines = b; break;
          case "-C": if (i + 1 < args.Length && int.TryParse(args[++i], out int c)) contextLines = c; break;

          // Add pattern manually
          case "-e": if (i + 1 < args.Length) patterns.Add(args[++i]); break;

          // Load patterns from file
          case "-f":
            if (i + 1 < args.Length && File.Exists(args[i + 1]))
              patterns.AddRange(File.ReadAllLines(args[++i]));
            break;

          default:
            // Determine if it's a file/directory or pattern
            if (File.Exists(arg) || Directory.Exists(arg))
              filePaths.Add(arg);
            else
              patterns.Add(arg);
            break;
        }
      }

      // Validate input
      if (patterns.Count == 0 || filePaths.Count == 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Missing pattern or file.");
        Console.ResetColor();
        return;
      }

      // Build the regex pattern
      var joinedPattern = string.Join("|", extendedRegex ? patterns : patterns.Select(Regex.Escape));
      if (wholeWordMatch) joinedPattern = $"\\b({joinedPattern})\\b";
      if (wholeLineMatch) joinedPattern = $"^({joinedPattern})$";

      // Compile regex with options
      var regexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
      if (extendedRegex) regexOptions |= RegexOptions.ECMAScript;
      var regex = new Regex(joinedPattern, regexOptions);

      // Collect all files from paths (including recursive directory scan)
      List<string> allFiles = new();
      foreach (var path in filePaths)
      {
        if (Directory.Exists(path) && recursiveSearch)
        {
          allFiles.AddRange(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories));
        }
        else if (File.Exists(path))
        {
          allFiles.Add(path);
        }
      }

      int totalMatches = 0;

      // Process each file
      foreach (var file in allFiles)
      {
        string[] lines;
        try { lines = File.ReadAllLines(file); } catch { continue; }

        List<int> matchedIndices = new();

        // Check each line for matches
        for (int i = 0; i < lines.Length; i++)
        {
          bool isMatch = regex.IsMatch(lines[i]);
          if (invertMatch) isMatch = !isMatch;
          if (isMatch) matchedIndices.Add(i);
        }

        if (matchedIndices.Count == 0) continue;
        totalMatches += matchedIndices.Count;

        if (listFilenames)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine(file);
          Console.ResetColor();
          continue;
        }

        if (countOnly)
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine($"{file}: {matchedIndices.Count} match(es)");
          Console.ResetColor();
          continue;
        }

        // Gather context lines to include
        HashSet<int> contextLinesSet = new(matchedIndices);
        foreach (var idx in matchedIndices)
        {
          for (int j = 1; j <= Math.Max(beforeLines, contextLines); j++)
            if (idx - j >= 0) contextLinesSet.Add(idx - j);
          for (int j = 1; j <= Math.Max(afterLines, contextLines); j++)
            if (idx + j < lines.Length) contextLinesSet.Add(idx + j);
        }

        // Output matched lines with formatting
        foreach (int i in contextLinesSet.OrderBy(i => i))
        {
          // Build prefix string
          string prefix = (!suppressFilenames && allFiles.Count > 1 ? $"{file}:" : "") + (showLineNumbers ? $"{i + 1}:" : "");
          Console.Write(prefix);

          // Output only matched parts
          if (onlyMatchedParts && regex.IsMatch(lines[i]))
          {
            foreach (Match match in regex.Matches(lines[i]))
            {
              Console.ForegroundColor = ConsoleColor.Yellow;
              Console.WriteLine(match.Value);
              Console.ResetColor();
            }
            continue;
          }

          // Output with highlighted matches
          int last = 0;
          foreach (Match m in regex.Matches(lines[i]))
          {
            Console.Write(lines[i].Substring(last, m.Index - last));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(m.Value);
            Console.ResetColor();
            last = m.Index + m.Length;
          }

          // Print remaining part of the line if any
          if (last < lines[i].Length) Console.WriteLine(lines[i].Substring(last));
          else Console.WriteLine();
        }
      }

      // Show total matches across files if in count mode
      if (countOnly && totalMatches > 0)
      {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Total matches: {totalMatches}");
        Console.ResetColor();
      }
    }
  }
}
