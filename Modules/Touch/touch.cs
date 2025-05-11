using System;
using DarkSigil.Interface;

namespace DarkSigil.Modules.Touch
{
  public class Touch : ICommands
  {
    public void Execute(string[] args)
    {

      if (args.Length == 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: touch [options] <filename>");
        Console.WriteLine("Options: -c (no create), -a (change access time), -m (change modification time), -t (set time), -d (date)");
        Console.ResetColor();
        return;
      }

      bool noCreate = false;
      bool updateAccess = false;
      bool updateMod = false;
      DateTime? customTime = null;

      var files = new List<string>();

      for (int i = 0; i < args.Length; i++)
      {
        var arg = args[i];

        switch (arg)
        {
          case "-c":
            noCreate = true;
            break;
          case "-a":
            updateAccess = true;
            break;
          case "-m":
            updateMod = true;
            break;
          case "-t":
            if (i < args.Length - 1)
            {
              i++;
              string ts = args[i];
              string format = ts.Contains(".") ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss";
              if (DateTime.TryParseExact(ts, format, null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
              {
                customTime = parsedTime;
              }
              else
              {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Invalid time format '{ts}'.");
                Console.ResetColor();
                return;
              }
            }
            break;
          default:
            files.Add(arg);
            break;
        }
      }

      if (!updateAccess && !updateMod)
      {
        updateAccess = true;
        updateMod = true;
      }

      DateTime now = customTime ?? DateTime.Now;

      foreach (var file in files)
      {
        if (File.Exists(file))
        {
          if (updateAccess)
          {
            File.SetLastAccessTime(file, now);
          }
          if (updateMod)
          {
            File.SetLastWriteTime(file, now);
          }
        }
        else if (!noCreate)
        {
          try
          {
            File.WriteAllText(file, string.Empty);
            if (customTime.HasValue)
            {
              if (updateAccess)
              {
                File.SetLastAccessTime(file, now);
              }
              if (updateMod)
              {
                File.SetLastWriteTime(file, now);
              }
            }
          }
          catch (Exception ex)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Could not create file {file}\n Exception Message: {ex.Message}");
            Console.ResetColor();
          }
        }
      }
    }
  }
}
