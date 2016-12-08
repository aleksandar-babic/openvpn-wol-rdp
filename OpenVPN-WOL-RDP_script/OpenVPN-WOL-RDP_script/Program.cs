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
                    //Console.WriteLine("Valid Mac address!");
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
                        Console.WriteLine("Everything looks good!\n\nMAC:{0}\nSubnet Broadcast IP :{1}\n\n",macAddr,subnetBroadcast);
                        Console.WriteLine("Sending WOL Packet..");
                        string strWOLCall;
                        strWOLCall = "/C mc-wol.exe "+ macAddr +" /a " + subnetBroadcast;
                        System.Diagnostics.Process.Start("CMD.exe", strWOLCall);
                        Console.WriteLine("WOL Packet sent!");
                        isValid = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid IP address for subnet broadcast, try again : ");
                        subnetBroadcast = Console.ReadLine();
                        match = Regex.Match(subnetBroadcast, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                    }
                }
            }
            return false;
        }
        static void Main(string[] args)
        {
            string checkAddr = "88:AE:1D:41:87:58";
            //Console.WriteLine(checkMacFormat(checkAddr));
            sendWOL(checkAddr,"10.10.0.255");

            //
            Console.Read();
        }
    }
}
