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
        public static string GetBaseUrl(int port = 7103)
        {
            string? ip = Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?
                .ToString();

            if (string.IsNullOrEmpty(ip))
                ip = "localhost"; // fallback if IP can’t be resolved

            return $"https://{ip}:{port}/api";
        }
    }
}
