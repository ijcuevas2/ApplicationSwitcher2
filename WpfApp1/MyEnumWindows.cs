﻿using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace WpfApp1
{
    public class ChildWindowSummary
    {
        string title;
        uint lpdwProcessId;
        IntPtr windowHandle; 
    }

    class MyEnumWindows
    {
        private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        [DllImport("user32")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32")]
        private static extern IntPtr GetWindow(IntPtr hwnd, int uCmd);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndStart, EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static List<string> windowTitles = new List<string>();


        public static List<string> GetWindowTitles(bool includeChildren)
        {
            EnumWindows(MyEnumWindows.EnumWindowsCallback, includeChildren ? (IntPtr)1 : IntPtr.Zero);
            return MyEnumWindows.windowTitles;
        }

        public static bool EnumWindowsCallback(IntPtr testWindowHandle, IntPtr includeChildren)
        {
            // NOTE: In the case of chrome, it was created by the same process
            string title = MyEnumWindows.GetWindowTitle(testWindowHandle);
            uint lpdwProcessId = 0;
            GetWindowThreadProcessId(testWindowHandle, out lpdwProcessId);

            // Console.WriteLine("Title: {0}, Process Id; {1}", title, lpdwProcessId);

            MyEnumWindows.windowTitles.Add(title);

            if (includeChildren.Equals(IntPtr.Zero) == false)
            {
                MyEnumWindows.EnumChildWindows(testWindowHandle, MyEnumWindows.EnumWindowsCallback, IntPtr.Zero);
            }

            return true;
        }

        private static bool TitleMatches(string title, string key)
        {
            bool match = title.Contains(key);
            return match;
        }

        private static string GetWindowTitle(IntPtr windowHandle)
        {
            uint SMTO_ABORTIFHUNG = 0x0002;
            uint WM_GETTEXT = 0xD;
            int MAX_STRING_SIZE = 32768;
            IntPtr result;
            string title = string.Empty;
            IntPtr memoryHandle = Marshal.AllocCoTaskMem(MAX_STRING_SIZE);
            Marshal.Copy(title.ToCharArray(), 0, memoryHandle, title.Length);
            MyEnumWindows.SendMessageTimeout(windowHandle, WM_GETTEXT, (IntPtr)MAX_STRING_SIZE, memoryHandle, SMTO_ABORTIFHUNG, (uint)1000, out result);
            title = Marshal.PtrToStringAuto(memoryHandle);
            Marshal.FreeCoTaskMem(memoryHandle);
            return title;
        }
    }
}