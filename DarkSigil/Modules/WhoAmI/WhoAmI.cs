using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.WhoAmI
{
    public class WhoAmI : ICommands
    {
      public void Execute(string[] args)
        {
            string username = Environment.UserName;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Current user: " + username);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
