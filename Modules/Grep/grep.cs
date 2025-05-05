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
      if (args.Length < 2)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: grep [options] <pattern> <file>");
        Console.WriteLine("Options: -i (ignore case), -n (show line numbers), -v (invert match), -c (count matches), -l (list filenames), -h (suppress filenames), -w (whole word match), -x (whole line match), -r (recursive), -e (multiple patterns), -f (patterns from file), -E (extended regex), -o (only matched parts), -A (n lines after), -B (n lines before), -C (n lines before and after)");
        Console.ResetColor();
        return;
      }

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
      string filePath = null;

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
          case "-A":
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int a)) afterLines = a;
            i++;
            break;
          case "-B":
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int b)) beforeLines = b;
            i++;
            break;
          case "-C":
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int c)) contextLines = c;
            i++;
            break;
          case "-e":
            if (i + 1 < args.Length) patterns.Add(args[++i]);
            break;
          case "-f":
            if (i + 1 < args.Length) patterns.AddRange(File.ReadAllLines(args[++i]));
            break;
          default:
            if (!arg.StartsWith("-"))
            {
              if (patterns.Count == 0)
                patterns.Add(arg);
              else
                filePath = arg;
            }
            break;

        }
      }

      if (patterns.Count == 0 || filePath == null)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Missing pattern or file.");
        Console.ResetColor();
        return;
      }

      var joinedPattern = string.Join("|", patterns);
      if (wholeWordMatch) joinedPattern = $"\\b({joinedPattern})\\b";
      if (wholeLineMatch) joinedPattern = $"^({joinedPattern})$";
      var regexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
      if (extendedRegex) regexOptions |= RegexOptions.ECMAScript;
      var regex = new Regex(joinedPattern, regexOptions);

      IEnumerable<string> filesToSearch = recursiveSearch
        ? Directory.EnumerateFiles(filePath, "*", SearchOption.AllDirectories)
        : new[] { filePath };

      int totalMatches = 0;

      foreach (var file in filesToSearch)
      {
        if (!File.Exists(file)) continue;

        var lines = File.ReadAllLines(file);
        List<string> matchedLines = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
          bool isMatch = regex.IsMatch(lines[i]);
          if (invertMatch) isMatch = !isMatch;

          if (isMatch)
          {
            totalMatches++;
            matchedLines.Add(lines[i]);

            if (onlyMatchedParts)
            {
              foreach (Match match in regex.Matches(lines[i]))
              {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(match.Value);
                Console.ResetColor();
              }
            }
            else
            {
              string prefix = "";
              if (!suppressFilenames && filesToSearch.Count() > 1)
              {
                prefix += $"{file}:";
              }
              if (showLineNumbers)
              {
                prefix += $"{i + 1}:";
              }

              Console.Write(prefix);
              int lastPos = 0;
              foreach (Match m in regex.Matches(lines[i]))
              {
                Console.Write(lines[i].Substring(lastPos, m.Index - lastPos));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(m.Value);
                Console.ResetColor();
                lastPos = m.Index + m.Length;
              }
              if (lastPos < lines[i].Length)
              {
                Console.WriteLine(lines[i].Substring(lastPos));
              }
              else
              {
                Console.WriteLine();
              }
            }
          }
        }

        if (countOnly && matchedLines.Count > 0)
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine($"{file}: {matchedLines.Count} match(es)");
          Console.ResetColor();
        }
        else if (listFilenames && matchedLines.Count > 0)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine(file);
          Console.ResetColor();
        }
      }

      if (countOnly && totalMatches > 0)
      {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Total matches: {totalMatches}");
        Console.ResetColor();
      }
    }
  }
}
