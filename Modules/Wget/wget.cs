using System;
using DarkSigil.Interface;
namespace DarkSigil.Modules.Wget
{
  public class Wget : ICommands
  {
    public void Execute(string[] args)
    {
      if (args.Length == 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: wget [options] <url>");
        Console.WriteLine("Options: -O (output file), -P (directory), -r (recursive), -nc (no clobber), -q (quiet), -c (continue), -b (background), -U (user-agent), --header (custom header), --referer (referer), --no-check-certificate");
        Console.ResetColor();
        return;
      }

      bool isDownloading = false;
      bool isRecursive = false;
      bool noClobber = false;
      bool quiet = false;
      bool continueDownload = false;
    }
  }
}
