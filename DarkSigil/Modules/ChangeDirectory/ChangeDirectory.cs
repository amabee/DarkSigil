using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.ChangeDirectory
{
    public class ChangeDirectory : ICommands
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: cd <directory>");
                return;
            }

            string path = args[0];

            try
            {
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(Directory.GetCurrentDirectory(), path);
                }

                if (Directory.Exists(path))
                {
                    Directory.SetCurrentDirectory(path);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Changed directory to: {path}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Directory does not exist: {path}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error changing directory: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
