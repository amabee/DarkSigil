using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.IfConfig
{
    public class IfConfig : ICommands
    {
        public void Execute(string[] args)
        {
            try { 
                
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces) {

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Interface: {networkInterface.Name}");
                    Console.ResetColor();

                    Console.WriteLine($"  Description : {networkInterface.Description}");
                    Console.WriteLine($"  Status      : {networkInterface.OperationalStatus}");
                    Console.WriteLine($"  Speed       : {networkInterface.Speed / 1_000_000} Mbps");
                    Console.WriteLine($"  MAC Address : {networkInterface.GetPhysicalAddress()}");

                    IPInterfaceProperties ipProps = networkInterface.GetIPProperties();
                    foreach (var gateway  in ipProps.GatewayAddresses)
                    {

                        Console.WriteLine($"  IP Address  : {gateway.Address}");
                    }

                    Console.WriteLine();
                }

            } catch (Exception ex) {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
