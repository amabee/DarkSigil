using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.WhoIs
{
    public class WhoIs : ICommands
    {
        private int WHOIS_PORT = 43;
        private string WHOSIS_HOST = "whois.arin.net";
        private string WHOIS_EDU = "whois.educause.edu";
        private string WHOIS_DEFAULT = "whois.internic.net";

       public async void Execute(string[] args)
        {
          

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: whois <domain>");
                return;
            }

            string domain = args[0];
            string query = domain;

            string whoIsHost;
            string queryString;

            if (query.EndsWith("?") || query.EndsWith("--help") || query.EndsWith("-h"))
            {
                Console.WriteLine("Usage: whois <domain>");
                return;
            }

            if (isIpAddr(domain))
            {
                whoIsHost = this.WHOSIS_HOST;
                queryString = "n " + domain;
            }
            else if (query.EndsWith(".edu"))
            {
                whoIsHost = this.WHOIS_EDU;
                queryString = "domain=" + domain;
            }
            else
            {
               whoIsHost = this.WHOIS_DEFAULT;
                queryString = "domain=" + domain;
            }

            string response = await WhoIsLookUp(whoIsHost, queryString);

            if (response != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(response);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No response from server.");
                Console.ForegroundColor = ConsoleColor.White;

            }
        }

        private bool isIpAddr(string ipaddr)
        {
            System.Net.IPAddress ip;
            return System.Net.IPAddress.TryParse(ipaddr, out ip);
        }

        private async Task<string> WhoIsLookUp(string host, string query)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient(host, WHOIS_PORT))
                using (var stream = client.GetStream())
                using (var writer = new System.IO.StreamWriter(stream))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    await writer.WriteLineAsync(query);
                    await writer.FlushAsync();
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
