using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace ApplicationSwitcher
{
    public class ChildWindowSummary
    {
        public string title;
        public uint lpdwProcessId;
        public IntPtr windowHandle; 

        public ChildWindowSummary(string title, uint lpdwProcessId, IntPtr windowHandle)
        {
            this.title = title;
            this.lpdwProcessId = lpdwProcessId;
            this.windowHandle = windowHandle;
        }
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

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static List<string> windowTitles = new List<string>();
        public static List<ChildWindowSummary> childWindowSummaries = new List<ChildWindowSummary>();

        public static List<string> GetWindowTitles(bool includeChildren)
        {
            EnumWindows(MyEnumWindows.EnumWindowsCallback, includeChildren ? (IntPtr)1 : IntPtr.Zero);
            return MyEnumWindows.windowTitles;
        }

        public static bool EnumWindowsCallback(IntPtr testWindowHandle, IntPtr includeChildren)
        {
            string title = MyEnumWindows.GetWindowTitle(testWindowHandle);
            uint lpdwProcessId = 0;
            GetWindowThreadProcessId(testWindowHandle, out lpdwProcessId);
            if (hasChildProgramWindow(testWindowHandle) && title != "")
            {
                ChildWindowSummary childWindowSummary = new ChildWindowSummary(title, lpdwProcessId, testWindowHandle);
                childWindowSummaries.Add(childWindowSummary);
            }


            MyEnumWindows.windowTitles.Add(title);

            if (includeChildren.Equals(IntPtr.Zero) == false)
            {
                MyEnumWindows.EnumChildWindows(testWindowHandle, MyEnumWindows.EnumWindowsCallback, IntPtr.Zero);
            }

            return true;
        }

        public static bool hasChildProgramWindow(IntPtr windowHandle)
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (UInt32)Marshal.SizeOf(info);
            MyEnumWindows.GetWindowInfo(windowHandle, ref info);
            Boolean match = processWindowInfo(info);
            return match;
        }

        private static Boolean processWindowInfo(WINDOWINFO info)
        {
            Dictionary<string, long> windowStyles = new Dictionary<string, long>();

            //windowStyles.Add("WS_MAXIMIZE", 0x01000000L);
            windowStyles.Add("WS_MINIMIZE", 0x20000000L);

            Dictionary<string, long> extendedWindowStyles = new Dictionary<string, long>();
            extendedWindowStyles.Add("WS_EX_TOPMOST", 0x00000008L);

            Boolean styleMatch = iterateThroughDict(windowStyles, info, false);
            Boolean exStyleMatch = iterateThroughDict(extendedWindowStyles, info, true);
            Boolean viableChildWindow = styleMatch && exStyleMatch;
            return viableChildWindow;
        }

        public static Boolean iterateThroughDict(Dictionary<string, long> dict, WINDOWINFO info, Boolean extended)
        {
            foreach(KeyValuePair<string, long> entry in dict)
            {
                long value = entry.Value;

                Boolean match = false;
                if (!extended)
                {
                    match = (info.dwStyle & value) != 0;
                    // System.Diagnostics.Debug.WriteLine("Hex: {0:X}", info.dwStyle);
                    //if ((info.dwStyle & dict["WS_MINIMIZE"]) == 0)
                    //{
                    //    continue;
                    //}
                }

                else
                {
                    // System.Diagnostics.Debug.WriteLine("Hex Extended: {1:X}", info.dwExStyle);
                    match = (info.dwExStyle & value) != 0;
                    //if ((info.dwExStyle & dict["WS_EX_TOPMOST"]) == 0)
                    //{
                    //    continue;
                    //}
                }

                if (match)
                {
                    if (entry.Key == "WS_EX_TOPMOST" || entry.Key == "WS_MINIMIZE")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TitleMatches(string title, string key)
        {
            bool match = title.Contains(key);
            return match;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

        private static string GetWindowTitle(IntPtr windowHandle)
        {
            object o = new object();
            int capacity = GetWindowTextLength(new HandleRef(o, windowHandle)) * 2;
            StringBuilder stringBuilder = new StringBuilder(capacity);
            GetWindowText(new HandleRef(o, windowHandle), stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
        }
    }
}
