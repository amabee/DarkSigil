using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.ping
{
    public class PingOptionsModel
    {

       

            public bool IsAudible { get; set; } = false;
            public int Count { get; set; } = -1;
            public int Interval {get; set;} = 1;
            public string InterfaceAddress { get; set; } = null; // or maybe just ""
            public bool IsNumeric { get; set; } = false;
            public bool IsQuiet { get; set; } = false;
            public int PacketSize { get; set; } = 32; // default windows ping packet size, 64 for linux dist
            public int TTL { get; set; } = 128; // default TTL is 128 in most OS's
            public bool IsVerbose { get; set; } = false;
            public int Timeout { get; set; } = 1;
            public string Host { get; set; } = null;
            public int Deadline { get; set; } = -1;
            public bool IsHelp { get; set; } = false;
            
       

    }
}
