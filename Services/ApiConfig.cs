using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Cashere.Services
{
    public static class ApiConfig
    {
        public static string GetBaseUrl()
        {
            // Check for saved configuration first
            if (Preferences.ContainsKey("ServerHostIP"))
            {
                string savedIp = Preferences.Get("ServerHostIP", "localhost");
                int port = Preferences.Get("ServerPort", 7102);
                return $"http://{savedIp}:{port}/api";
            }

            // Auto-detect for Windows
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                string ip = GetLocalIPAddress();
                int port = 7102;
                return $"http://{ip}:{port}/api";
            }

            // Fallback for Android (use a common local IP pattern)
            // This will be updated when server starts on Windows
            return "http://192.168.1.6:7102/api";
        }

        public static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }

            return "localhost";
        }

        public static void SaveServerConfig(string ip, int port)
        {
            Preferences.Set("ServerHostIP", ip);
            Preferences.Set("ServerPort", port);
        }

        public static bool IsConfigured()
        {
            return Preferences.ContainsKey("ServerHostIP");
        }
    }
}
