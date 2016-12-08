using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenVPN_WOL_RDP_script
{
    class Program
    {
        public static string checkMacFormat(string macAddr) {

            macAddr = macAddr.Replace(" ", ":").Replace("-", ":");

            Regex r = new Regex("^(?:[0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}|(?:[0-9a-fA-F]{2}-){5}[0-9a-fA-F]{2}|(?:[0-9a-fA-F]{2}){5}[0-9a-fA-F]{2}$");
            bool isValid = false;

            while (!isValid)
            {
                if (r.IsMatch(macAddr))
                {
                    Console.WriteLine("Valid Mac address!");
                    return macAddr;
                }
                else
                {
                    Console.Write("Invalid Mac, try again. Mac Address : ");
                    macAddr = Console.ReadLine();
                    macAddr = macAddr.Replace(" ", ":").Replace("-", ":");
                }
            }
            return null;
        }

        public static Boolean sendWOL(string macAddr, string subnetBroadcast) {
            if (checkMacFormat(macAddr) != null)
            {
                var match = Regex.Match(subnetBroadcast, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                bool isValid = false;
                while (!isValid)
                {
                    if (match.Success)
                    {
                        Console.WriteLine("Good IP!");
                        isValid = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid IP address for subnet broadcast, try again : ");
                        subnetBroadcast = Console.ReadLine();
                        match = Regex.Match(subnetBroadcast, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                    }
                }
                Console.WriteLine(macAddr + "   " + subnetBroadcast);
            }
            return false;
        }
        static void Main(string[] args)
        {
            string checkAddr = "88-AE-1D-41D-87-58";
            //Console.WriteLine(checkMacFormat(checkAddr));
            sendWOL(checkAddr,"192.168.1.2");

        }
    }
}
