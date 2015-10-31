// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Tera.DamageMeter
{
    public class TeraWindow
    {
        private const int WM_CHAR = 0x0102;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        public static bool SendString(string s)
        {
            var teraWindow = FindTeraWindow();
            if (teraWindow == IntPtr.Zero)
                return false;

            SendString(teraWindow, s);

            return true;
        }

        private static void SendString(IntPtr hWnd, string s)
        {
            foreach (var character in s)
            {
                if (!PostMessage(hWnd, WM_CHAR, character, 0))
                    throw new Win32Exception();
                Thread.Sleep(1);
            }
        }

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

        public static bool IsTeraRunning()
        {
            return FindTeraWindow() != IntPtr.Zero;
        }
    }
}
