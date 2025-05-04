using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.Pwd
{
    public class PWD : ICommands
    {
        public void Execute(string[] args)
        {
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Current Directory: " + currentDirectory);
            Console.ForegroundColor = ConsoleColor.White;

        }
    }
}
