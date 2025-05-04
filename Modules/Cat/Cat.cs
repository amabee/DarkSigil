using DarkSigil.Interface;
using System;
using System.IO;

namespace DarkSigil.Modules.Cat
{
    public class CAT : ICommands
    {
        public void Execute(string[] args)
        {
            if (args.Length == 1)
            {
                string filePath = args[0];

                try
                {
                    if (File.Exists(filePath))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        foreach (var line in File.ReadLines(filePath))
                        {
                            Console.WriteLine(line);
                        }
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: The file '{filePath}' does not exist.");
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
                Console.WriteLine("Usage: cat <file-path>");
                Console.ResetColor();
            }
        }
    }
}
