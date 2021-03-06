﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Net;

namespace OpenVPN_WOL_RDP_script
{
    class Program
    {
        public static string MAC;
        public static string IP;
        public static string ROUTER;
        public static string FILE;

        public static string checkMacFormat(string macAddr)
        {

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


        public static string checkIPandCreateBroadcast(string ip)
        {
            Match match = Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            if (match.Success)
            {
                string[] tmpip = ip.Split('.');
                ip = tmpip[0] + '.' + tmpip[1] + '.' + tmpip[2] + ".255";
                return ip;
            }
            else
                return "";


        }


        public static void getConfig()
        {
            using (StreamReader reader = new StreamReader("config.ini"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] tmp = line.Split('=');
                    switch (tmp[0])
                    {
                        case "MAC": MAC = tmp[1]; break;
                        case "IP": IP = tmp[1]; break;
                        case "ROUTER": ROUTER = tmp[1]; break;
                        case "FILE": FILE = tmp[1]; break;
                    }

                }

            }

        }


        public static bool isReachable(string ipAddr)
        {
            Console.WriteLine("Waiting for {0} to become reachable.", ipAddr);
            Ping p = new Ping();
            PingReply r;
            while (true)
            {
                if (ipAddr != null)
                {
                    r = p.Send(ipAddr);
                    if (r.Status == IPStatus.Success)
                    {
                        Console.WriteLine("{0} is reachable!", ipAddr);
                        return true;
                    }
                }
                else break;
            }
            return false;
        }


        public static void startRDP(string ip)
        {
            if (isReachable(ip))
            {
                Console.WriteLine("\n\nGiving machine 20seconds to finish booting after becoming reachable.");
                Thread.Sleep(20000);
                string strConnectIP = ip;
                string strRDPCall;
                strRDPCall = "/v " + strConnectIP;
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "mstsc.exe";
                startInfo.Arguments = strRDPCall;
                process.StartInfo = startInfo;
                process.Start();
                Console.WriteLine("\n\nStarted RDP session for IP : {0}", strConnectIP);
            }
            else
            {
                Console.WriteLine("Can't start RDP, machine({0}) is not reachable", ip);
            }

        }


        public static bool sendWOL(string macAddr, string ip)

        {

            if (((macAddr = checkMacFormat(macAddr)) != null) && (checkIPandCreateBroadcast(ip) != ""))
            {
                string html = string.Empty;
                string url = @"http://" + ROUTER + "/" + FILE + "?mac=" + macAddr + "&ip=" + checkIPandCreateBroadcast(ip);
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        html = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error connecting to WOL server. Full error : {0}\n", ex);
                    return false;
                }


                Console.WriteLine("Response from WOL server : {0}\n", html);
                return true;
            }

            return false;
        }


        public static string checkOpenVPNService()
        {
            ServiceController sc = new ServiceController("OpenVPNService");

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return "Running";
                case ServiceControllerStatus.Stopped:
                    return "Stopped";
                case ServiceControllerStatus.Paused:
                    return "Paused";
                case ServiceControllerStatus.StopPending:
                    return "Stopping";
                case ServiceControllerStatus.StartPending:
                    return "Starting";
                default:
                    return "Status Changing";
            }
        }


        public static bool connectToOpenVPN()
        {
            if (checkOpenVPNService() == "Running")
            {
                Console.WriteLine("OpenVPN service is already running.");
                return true;
            }
            else if (checkOpenVPNService() == "Stopped")
            {
                Console.WriteLine("OpenVPN service is stopped.");
                Console.WriteLine("Trying to start OpenVPN service.");
                ServiceController sc = new ServiceController("OpenVPNService");
                try
                {
                    sc.Start();
                    Thread.Sleep(4000);
                    if (checkOpenVPNService() == "Running")
                    {
                        Console.WriteLine("OpenVPN service is running.");
                        Thread.Sleep(7000);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error starting OpenVPN service. Error : {0}", ex);
                }
            }
            return false;
        }


        static void Main(string[] args)
        {
            getConfig();
            string checkAddr = MAC;
            string rdpIP = IP;

            if (checkAddr != "" && rdpIP != "" && ROUTER != "" && FILE != "")
            {

                if (connectToOpenVPN() != false)
                {
                    if (checkIPandCreateBroadcast(rdpIP) != "" && checkMacFormat(checkAddr) != null)
                    {
                        sendWOL(checkAddr, rdpIP);
                        startRDP(rdpIP);
                        Console.WriteLine("\n\n\nIf your RDP is started you can close this window by pressing any key.");
                        Console.Read();
                    }
                    else
                        Console.WriteLine("IP or MAC invalid.");
                }
                else
                {
                    Console.WriteLine("Could not start OpenVPN. Make sure your OpenVPN GUI is not started and connected.\nPress any key to exit..");
                    Console.Read();
                }
            }
            else
            {
                Console.WriteLine("OpenVPN - WakeOnLan - RDP script v1.0\n\nPopulate config.ini file in format: \nMAC=XX:XX:XX:XX:XX:XX\nIP=x.x.x.x\nROUTER=x.x.x.x\nFILE=filename.php\nPress any key to exit..");
                Console.Read();
                Environment.Exit(-1);
            }

        }
    }
}
