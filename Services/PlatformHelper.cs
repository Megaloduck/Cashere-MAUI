using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace Cashere.Services
{
    public static class PlatformHelper
    {
        public static bool IsWindows => DeviceInfo.Platform == DevicePlatform.WinUI;
        public static bool IsAndroid => DeviceInfo.Platform == DevicePlatform.Android;
    }
}
