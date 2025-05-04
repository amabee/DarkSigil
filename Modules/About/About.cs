using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules
{
    public class About : ICommands
    {
        public void Execute(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("┌─────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│   ██████╗  █████╗ ██████╗ ██╗  ██╗███████╗██╗ ██████╗ ██╗██╗        │");
            Console.WriteLine("│   ██╔══██╗██╔══██╗██╔══██╗██║ ██╔╝██╔════╝██║██╔════╝ ██║██║        │");
            Console.WriteLine("│   ██║  ██║███████║██████╔╝█████╔╝ ███████╗██║██║  ███╗██║██║        │");
            Console.WriteLine("│   ██║  ██║██╔══██║██╔══██╗██╔═██╗ ╚════██║██║██║   ██║██║██║        │");
            Console.WriteLine("│   ██████╔╝██║  ██║██║  ██║██║  ██╗███████║██║╚██████╔╝██║███████╗   │");
            Console.WriteLine("│   ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝╚══════╝   │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│                Coded in the shadows with C# ConsoleApp + .Net       │");
            Console.WriteLine("│                Unauthorized access is... encouraged :)              │");
            Console.WriteLine("│                                                                     │");
            Console.WriteLine("│                Features:                                            │");
            Console.WriteLine("│               - Custom command parsing                              │");
            Console.WriteLine("│               - WhoIs Domain LookUp                                 │");
            Console.WriteLine("│               - History & FS emulation (cumming soon)               │");
            Console.WriteLine("│                                                                     │");
            Console.WriteLine("│                 Version : 1.0.0                                     │");
            Console.WriteLine("│                 Creator : @amabee                                   │");
            Console.WriteLine("│                 Repo    : github.com/amabee/DarkSigil               │");
            Console.WriteLine("└─────────────────────────────────────────────────────────────────────┘");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
