using DarkSigil.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil
{
    public class Program
    {
        static void Main(string[] args)
        {
            CommandHandler handler = new CommandHandler();
            handler.Run(); 
        }
    }
}
