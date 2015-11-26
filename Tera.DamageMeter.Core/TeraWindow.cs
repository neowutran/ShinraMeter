// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Tera.DamageMeter
{
    public static class TeraWindow
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private static IntPtr FindTeraWindow()
        {
            var error0 = Marshal.GetLastWin32Error();
            var result = FindWindow("LaunchUnrealUWindowsClient", "TERA");
            var error = Marshal.GetLastWin32Error();
            //if (result == IntPtr.Zero && (error != 0))
            //    throw new Win32Exception();
            return result;
        }

        public static bool IsTeraActive()
        {
            var teraWindow = FindTeraWindow();
            var activeWindow = GetForegroundWindow();
            return (teraWindow != IntPtr.Zero) && (teraWindow == activeWindow);
        }
    }
}
