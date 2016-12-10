using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

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
                if (r.IsMatch(macAddr)) return macAddr;
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
            try
            {
                if (checkMacFormat(macAddr) != null)
                {
                    var match = Regex.Match(subnetBroadcast, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
                    bool isValid = false;
                    while (!isValid)
                    {
                        if (match.Success)
                        {
                            Console.WriteLine("Everything looks good!\nMAC:{0}\nSubnet Broadcast IP :{1}\n\n", macAddr, subnetBroadcast);
                            Console.WriteLine("Sending WOL Packet..");
                            string strWOLCall;
                            strWOLCall = "/C mc-wol.exe " + macAddr + " /a " + subnetBroadcast;
                            System.Diagnostics.Process.Start("CMD.exe", strWOLCall);
                            Thread.Sleep(2000);
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
            }
            catch (Exception ex) {
                Console.WriteLine("Could not send WOL packet. Error : {0}",ex);
            }
            return false;
        }

        public static void pingPartNetwork(string range,string subnetBroadcast) {
            var match = Regex.Match(subnetBroadcast, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
            bool isValid = false;
            try
            {
                while (!isValid)
                {
                    if (match.Success)
                    {
                        string[] strTmpRange = range.Split('-');
                        string[] strTmp = subnetBroadcast.Split('.');
                        string strFinal = strTmp[0] + "." + strTmp[1] + "." + strTmp[2] + ".";
                        for (int i = Convert.ToInt32(strTmpRange[0]); i < Convert.ToInt32(strTmpRange[1]); i++)
                        {
                            string strPingCall;
                            strPingCall = "/C ping " + strFinal + i + " -n 1";
                            Process process = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.FileName = "cmd.exe";
                            startInfo.Arguments = strPingCall;
                            process.StartInfo = startInfo;
                            process.Start();

                        }
                        isValid = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid IP address format for network discovery.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not discover all network devices. Error : {0}", ex);
            }
        }

        public string getMacByIp(string ip)
        {
            var macIpPairs = GetAllMacAddressesAndIppairs();
            int index = macIpPairs.FindIndex(x => x.IpAddress == ip);
            if (index >= 0)
            {
                return macIpPairs[index].MacAddress.ToUpper();
            }
            else
            {
                return null;
            }
        }

        public struct MacIpPair
        {
            public string MacAddress;
            public string IpAddress;
        }

        public static List<MacIpPair> GetAllMacAddressesAndIppairs()
        {
            List<MacIpPair> mip = new List<MacIpPair>();
            Process pProcess = new Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a ";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string cmdOutput = pProcess.StandardOutput.ReadToEnd();
            string pattern = @"(?<ip>([0-9]{1,3}\.?){4})\s*(?<mac>([a-f0-9]{2}-?){6})";

            foreach (Match m in Regex.Matches(cmdOutput, pattern, RegexOptions.IgnoreCase))
            {
                mip.Add(new MacIpPair()
                {
                    MacAddress = m.Groups["mac"].Value,
                    IpAddress = m.Groups["ip"].Value
                });
            }

            return mip;
        }

        public static string getIP(string macAddr) {
            macAddr = macAddr.Replace(" ", "-").Replace(":", "-").ToLower();
            List<MacIpPair> tmp = GetAllMacAddressesAndIppairs();
            foreach (var v in tmp) {
                if (macAddr == v.MacAddress) {
                    Console.WriteLine("Found IP : {0} for requested MAC : {1}", v.IpAddress, macAddr);
                    return v.IpAddress;
                    break;
                }
            }
            return "";
        }

        public static void startRDP(string macAddr) {
            string strConnectIP = getIP(macAddr);
            string strRDPCall;
            strRDPCall = "/v " + strConnectIP;
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "mstsc.exe";
            startInfo.Arguments = strRDPCall;
            process.StartInfo = startInfo;
            process.Start();
            Console.WriteLine("\n\nStarted RDP session for IP : {0}",strConnectIP);

        }

        static void Main(string[] args)
        {
            string checkAddr = "88:AE:1D:41:87:58";
            string broadcastIP = "10.10.0.255";
            Thread worker1 = new Thread(() => pingPartNetwork("0-100", broadcastIP));
            Thread worker2 = new Thread(() => pingPartNetwork("101-200", broadcastIP));
            Thread worker3 = new Thread(() => pingPartNetwork("201-254", broadcastIP));
            Console.WriteLine("Starting network discovery..");
            worker1.Start();
            worker2.Start();
            worker3.Start();
            worker1.Join();
            worker2.Join();
            worker3.Join();
            Console.WriteLine("Network discovery done!\n");
            sendWOL(checkAddr, broadcastIP);
            startRDP(checkAddr);









            Console.Read();
        }
    }
}
