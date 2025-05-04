using System;
using System.Collections.Generic;
using System.IO;
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
        Console.WriteLine("Options: -i (ignore case), -n (show line numbers), -v (invert match), -c (count matches), -l (list filenames), -h (suppress filenames), -w (whole word match), -x (whole line match), -r (recursive), -e <exp> (multiple patterns), -f <file> (patterns from file), -E (extended regex), -o (only matched parts), -A <n> (n lines after), -B <n> (n lines before), -C <n> (n lines before and after)");
        Console.ResetColor();
        return;
      }

      // Flag variables
      bool ignoreCase = false, showLineNumbers = false, invertMatch = false,
           countOnly = false, listFilenames = false, suppressFilenames = false,
           wholeWordMatch = false, wholeLineMatch = false, recursiveSearch = false,
           extendedRegex = false, onlyMatchedParts = false;

      int afterLines = 0, beforeLines = 0, contextLines = 0;
      List<string> patterns = new List<string>();
      List<string> files = new List<string>();

      for (int i = 0; i < args.Length; i++)
      {
        switch (args[i])
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
          case "-A": if (++i < args.Length && int.TryParse(args[i], out int a)) afterLines = a; break;
          case "-B": if (++i < args.Length && int.TryParse(args[i], out int b)) beforeLines = b; break;
          case "-C": if (++i < args.Length && int.TryParse(args[i], out int c)) contextLines = c; break;
          case "-e": if (++i < args.Length) patterns.Add(args[i]); break;
          case "-f": if (++i < args.Length) patterns.AddRange(File.ReadAllLines(args[i])); break;
          default:
            if (!args[i].StartsWith("-"))
            {
              if (patterns.Count == 0) patterns.Add(args[i]);
              else files.Add(args[i]);
            }
            break;
        }
      }

      if (patterns.Count == 0 || files.Count == 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Missing pattern or file.");
        Console.ResetColor();
        return;
      }

      string patternJoined = string.Join("|", patterns);
      if (wholeWordMatch) patternJoined = $@"\\b({patternJoined})\\b";
      if (wholeLineMatch) patternJoined = $"^{patternJoined}$";

      var regexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
      if (extendedRegex) regexOptions |= RegexOptions.ECMAScript;
      var regex = new Regex(patternJoined, regexOptions);

      int totalMatches = 0;

      foreach (var file in files)
      {
        IEnumerable<string> filePaths = recursiveSearch
            ? Directory.EnumerateFiles(file, "*", SearchOption.AllDirectories)
            : new[] { file };

        foreach (var path in filePaths)
        {
          if (!File.Exists(path)) continue;

          var lines = File.ReadAllLines(path);
          int matchCount = 0;

          for (int i = 0; i < lines.Length; i++)
          {
            bool isMatch = regex.IsMatch(lines[i]);
            if (invertMatch) isMatch = !isMatch;

            if (isMatch)
            {
              matchCount++;
              totalMatches++;

              if (countOnly) continue;

              if (listFilenames) break;

              if (onlyMatchedParts)
              {
                foreach (Match m in regex.Matches(lines[i]))
                  Console.WriteLine(m.Value);
              }
              else if (contextLines > 0 || beforeLines > 0 || afterLines > 0)
              {
                int before = Math.Max(beforeLines, contextLines);
                int after = Math.Max(afterLines, contextLines);
                for (int j = i - before; j <= i + after; j++)
                {
                  if (j >= 0 && j < lines.Length)
                  {
                    string linePrefix = showLineNumbers ? $"{j + 1}:" : "";
                    Console.WriteLine($"{linePrefix}{lines[j]}");
                  }
                }
              }
              else
              {
                string linePrefix = showLineNumbers ? $"{i + 1}:" : "";
                Console.WriteLine($"{linePrefix}{lines[i]}");
              }
            }
          }

          if (countOnly)
          {
            Console.WriteLine($"{path}: {matchCount}");
          }
          else if (listFilenames && matchCount > 0)
          {
            Console.WriteLine(path);
          }
        }
      }

      if (countOnly && files.Count > 1)
      {
        Console.WriteLine($"Total matches: {totalMatches}");
      }
    }
  }
}
