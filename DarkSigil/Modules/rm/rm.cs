using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.rm
{
    public class Rm : ICommands
    {
        public void Execute(string[] args)
        {
            bool recursive = false;
            bool force = false;
            var targets = new List<String>();

            foreach (string arg in args) {

                switch (arg)
                {
                    case "-r":
                    case "--recursive":
                        recursive = true;
                        break;

                    case "-f":
                    case "--force":
                        force = true; 
                        break;

                    case "-rf":
                        force |= true;
                        recursive |= true; 
                        break;

                    default:
                        targets.Add(arg);
                        break;
                }
            }

            if (targets.Count == 0) {

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Usage: rm [-r] [-f] [file|dir]");
                Console.ResetColor();

                return;
                
            }

            foreach (var target in targets) {

                try {
                    if (Directory.Exists(target))
                    {

                        if (recursive)
                        {
                            Directory.Delete(target, true);
                            Console.WriteLine($"Removed Directory: {target}");
                        }
                        else
                        {
                            Console.WriteLine($"{target} is a directory. Use -r to remove it.");
                        }

                    }
                    else if (File.Exists(target))
                    {

                        File.Delete(target);
                        Console.WriteLine($"Removed file: {target}");

                    }
                    else {
                        if (!force)
                        {
                            Console.WriteLine($"No such file or directory: {target}");
                        }
                    }


                }
                catch(Exception ex) {

                    if (!force)
                    {
                        Console.ForegroundColor= ConsoleColor.Red;
                        Console.WriteLine($"Error removing {target}: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            
            }
        }
    }
}
