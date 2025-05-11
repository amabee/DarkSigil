using System;
using DarkSigil.Interface;

namespace DarkSigil.Modules.Echo
{
  public class Echo : ICommands
  {
    public void Execute(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine();
        return;
      }

      bool noNewLine = false;
      bool interpretEscapes = false;
      bool rawLiteral = false;
      int startIndex = 0;

      while (startIndex < args.Length && args[startIndex].StartsWith("-"))
      {
        var option = args[startIndex];
        switch (option)
        {
          case "-n":
            noNewLine = true;
            break;
          case "-e":
            interpretEscapes = true;
            break;
          case "-E":
            rawLiteral = true;
            break;
          default:
            break;
        }
        startIndex++;
      }
      {
        var message = String.Join(" ", args, startIndex, args.Length - startIndex);
        if (interpretEscapes && !rawLiteral)
        {
          message = message.Replace("\\n", "\n")
                           .Replace("\\t", "\t")
                           .Replace("\\r", "\r")
                           .Replace("\\b", "\b")
                           .Replace("\\a", "\a")
                           .Replace("\\f", "\f")
                           .Replace("\\\"", "\"")
                           .Replace("\\\\", "\\");
        }

        if (noNewLine)
        {
          Console.Write(message);
        }
        else
        {
          Console.WriteLine(message);
        }
      }
    }
  }
}
