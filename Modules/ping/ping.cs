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
      ConsoleCancelEventHandler cancelHandler = (sender, e) =>
      {
        e.Cancel = true;
        isCancelled = true;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nPing is cancelled by user.");
        Console.ResetColor();
      };
      Console.CancelKeyPress += cancelHandler;

      var options = PingArgumentParser.Parse(args);
      int sent = 0;
      int received = 0;
      long totalTime = 0;
      long minTime = long.MaxValue;
      long maxTime = long.MinValue;

      try
      {
        options = PingArgumentParser.Parse(args);

        if (options.IsHelp || string.IsNullOrEmpty(options.Host))
        {
          ShowHelp();
          return;
        }

        var pingSender = new Ping();
        var pingOptions = new PingOptions { Ttl = options.TTL };
        byte[] buffer = new byte[options.PacketSize];
        var deadline = options.Deadline > 0 ? DateTime.Now.AddSeconds(options.Deadline) : DateTime.MaxValue;
        Console.WriteLine($"Pinging {options.Host} with {options.PacketSize} bytes of data:\n");

        while (!isCancelled && (options.Count < 0 || sent < options.Count) && DateTime.Now < deadline)
        {
          try
          {
            PingReply reply = pingSender.Send(options.Host, options.Timeout * 1000, buffer, pingOptions);
            sent++;

            if (reply != null && reply.Status == IPStatus.Success)
            {
              received++;
              long roundtrip = reply.RoundtripTime;
              totalTime += roundtrip;
              minTime = Math.Min(minTime, roundtrip);
              maxTime = Math.Max(maxTime, roundtrip);

              if (!options.IsQuiet)
              {
                var output = options.IsNumeric ? reply.Address.ToString() : options.Host;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{(options.IsVerbose ? "[VERBOSE] " : "")}Reply from {output} [{reply.Address}]: bytes={options.PacketSize} time={roundtrip}ms TTL={reply.Options.Ttl}");
                Console.ResetColor();
              }

              if (options.IsAudible)
              {
                Console.Beep();
              }
            }
            else if (!options.IsQuiet)
            {
              Console.ForegroundColor = ConsoleColor.Yellow;
              Console.WriteLine($"Request to {options.Host} failed: {reply?.Status}");
              Console.ResetColor();
            }

            if (isCancelled) break;
            Thread.Sleep(options.Interval * 1000);
          }
          catch (Exception ex)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ping failed: {ex.Message}");
            Console.ResetColor();
            break;
          }
        }
      }
      finally
      {
        if (options != null)
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine($"\n--- {options.Host} ping statistics ---");
          Console.WriteLine($"{sent} packets transmitted, {received} received, {((sent - received) * 100 / sent)}% packet loss");

          if (received > 0)
          {
            long avgTime = totalTime / received;
            Console.WriteLine($"rtt min/avg/max = {minTime}ms/{avgTime}ms/{maxTime}ms");
          }
          Console.ResetColor();
        }

        Console.CancelKeyPress -= cancelHandler;
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
