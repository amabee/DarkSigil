using DarkSigil.Interface;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace DarkSigil.Modules.ping
{
    public class PING : ICommands
    {
        public void Execute(string[] args)
        {
            bool isCancelled = false;

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nPing is cancelled by user.");
                Console.ResetColor();

                isCancelled = true;
                e.Cancel = true;

            };

           var options = PingArgumentParser.Parse(args);

            if(options.IsHelp || string.IsNullOrEmpty(options.Host))
            {
                ShowHelp();
                return;
            }

            var pingSender = new Ping();
            var pingOptions = new PingOptions { Ttl = options.TTL};
            byte[] buffer = new byte[options.PacketSize];
            int sent = 0;

            var deadline = options.Deadline > 0 ? DateTime.Now.AddSeconds(options.Deadline) : DateTime.MaxValue;

            Console.WriteLine($"Pinging {options.Host} with {options.PacketSize} bytes of data:\n");

            while (!isCancelled && (options.Count < 0 || sent < options.Count) && DateTime.Now < deadline) {
                try
                { 
                    PingReply reply = pingSender.Send(options.Host, options.Timeout * 1000, buffer, pingOptions);
                   
                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        var output = options.IsNumeric ? reply.Address.ToString() : options.Host;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{(options.IsVerbose ? "[VERBOSE]" : "")} Reply from {output} [{reply.Address}]: bytes={options.PacketSize} time={reply.RoundtripTime}ms TTL={reply.Options.Ttl}");

                        if (options.IsAudible)
                        {
                            Console.Beep();
                        }   
                    }
                    else if (!options.IsQuiet)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Request to {options.Host} failed: {reply.Status}");
                    }

                    Console.ResetColor();
                    Thread.Sleep(options.Interval * 1000);
                    sent++;

                }
                catch (Exception ex) {
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ping failed: {ex.Message}");
                    Console.ResetColor();
                    break;
                
                }
            
            
            }
        }

        private void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Usage: ping [options] host");
            Console.WriteLine("   -a              Beep when host is reachable");
            Console.WriteLine("   -c <count>      Number of packets to send");
            Console.WriteLine("   -i <interval>          Interval between requests (seconds)");
            Console.WriteLine("   -I <interface>           Source IP/Interface for(IPv6 link-local)");
            Console.WriteLine("   -n           Numeric output (IP Only :p)");
            Console.WriteLine("   -q           Suppress all output except summary");
            Console.WriteLine("   -s <size>           Packet size in bytes");
            Console.WriteLine("   -t <ttl>           Time To Live");
            Console.WriteLine("   -v           Make verbose output");
            Console.WriteLine("   -w <seconds>           Deadline for total operation");
            Console.WriteLine("   -W <timeout>           Timeout per request in seconds");
            Console.WriteLine("   -h, --help           Show this help message");

            Console.WriteLine("== Sample commands ==");
            Console.WriteLine("ping example.com");
            Console.WriteLine("ping -a example.com");
            Console.WriteLine("ping -c 4 i 2 example.com");
            Console.WriteLine("ping -n -v example.com");
            Console.WriteLine("ping -s 128 -t 64 -W 2 example.com");
            Console.WriteLine("ping -a -w 10 8.8.8.8 ");
            Console.WriteLine("ping -q -c 10 -i 1 --help");
            Console.ResetColor();
        }
    }
}
