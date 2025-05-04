using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Interface
{
    public interface ICommands
    {
        void Execute(string[] args);
    }
}
