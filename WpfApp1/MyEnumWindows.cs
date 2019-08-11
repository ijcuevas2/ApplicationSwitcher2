using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace WpfApp1
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
            // NOTE: In the case of chrome, it was created by the same process
            string title = MyEnumWindows.GetWindowTitle(testWindowHandle);
            uint lpdwProcessId = 0;
            GetWindowThreadProcessId(testWindowHandle, out lpdwProcessId);
            //if (hasChildProgramWindow(testWindowHandle) && title != "")
            if (hasChildProgramWindow(testWindowHandle))
            {
                ChildWindowSummary childWindowSummary = new ChildWindowSummary(title, lpdwProcessId, testWindowHandle);
                childWindowSummaries.Add(childWindowSummary);
                Console.WriteLine("Title: {0}, Process Id; {1}", title, lpdwProcessId);
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

            // NOTE: Not necessary I guess?
            Dictionary<string, long> extendedWindowStyles = new Dictionary<string, long>();
            extendedWindowStyles.Add("WS_EX_TOPMOST", 0x00000008L);

            Boolean styleMatch = iterateThroughDict(windowStyles, info, false);
            Boolean exStyleMatch = iterateThroughDict(extendedWindowStyles, info, true);
            Boolean viableChildWindow = styleMatch && exStyleMatch;
            return viableChildWindow;
        }

        public static Boolean iterateThroughDict(Dictionary<string, long> dict, WINDOWINFO info, Boolean extended)
        {
            Console.WriteLine("iterateThroughDict Size: {0}", dict.Count);
            foreach(KeyValuePair<string, long> entry in dict)
            {
                long value = entry.Value;

                Boolean match = false;
                if (!extended)
                {
                    match = (info.dwStyle & value) != 0;
                    // Console.WriteLine("Hex: {0:X}", info.dwStyle);
                    //if ((info.dwStyle & dict["WS_MINIMIZE"]) == 0)
                    //{
                    //    continue;
                    //}
                }

                else
                {
                    // Console.WriteLine("Hex Extended: {1:X}", info.dwExStyle);
                    match = (info.dwExStyle & value) != 0;
                    //if ((info.dwExStyle & dict["WS_EX_TOPMOST"]) == 0)
                    //{
                    //    continue;
                    //}
                }

                // Console.WriteLine("Match: {0}", match);

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
