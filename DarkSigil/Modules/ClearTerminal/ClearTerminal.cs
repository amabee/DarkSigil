﻿using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules
{
    public class ClearTerminal : ICommands
    {
        public void Execute(string[] args)
        {
            Console.Clear();
        }
    }
}
