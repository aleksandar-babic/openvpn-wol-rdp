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


            if (r.IsMatch(macAddr))
            {
                Console.WriteLine("Valid Mac address");
                return macAddr;
            }
            else
            {
                Console.WriteLine("Invalid Mac");
                return null;
            }

        }
        static void Main(string[] args)
        {
            string checkAddr = "88-AE-1D-41-87-58";
            
        }
    }
}
