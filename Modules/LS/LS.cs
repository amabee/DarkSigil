using DarkSigil.Interface;
using System;
using System.IO;

namespace DarkSigil.Modules.LS
{
    public class LS : ICommands
    {
        public void Execute(string[] args)
        {
            try
            {
                
                string currentDirectory = Directory.GetCurrentDirectory();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Contents of directory: {currentDirectory}");
                Console.ResetColor();

                ListCurrentDirectory(currentDirectory);
            }
            catch (Exception ex)
            {
             
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private void ListCurrentDirectory(string directoryPath)
        {
            try
            {
             
                string[] directories = Directory.GetDirectories(directoryPath);
                string[] files = Directory.GetFiles(directoryPath);

            
                string[] allEntries = new string[directories.Length + files.Length];
                directories.CopyTo(allEntries, 0);
                files.CopyTo(allEntries, directories.Length);

                Array.Sort(allEntries);

                foreach (string entry in allEntries)
                {
                    string name = Path.GetFileName(entry);

                    if (Directory.Exists(entry))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(name + " <DIR>");
                        Console.ResetColor();
                    }
                    else
                    {

                        Console.ForegroundColor = ConsoleColor.White; 
                        Console.WriteLine(name);
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error reading directory: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
