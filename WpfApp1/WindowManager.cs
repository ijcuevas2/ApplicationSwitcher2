using System;
using System.Runtime.InteropServices;

namespace ApplicationSwitcher
{
    class WindowManager
    {
        public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        // this would iterate through child windows
        public static bool IterateChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam)
        {
            return EnumChildWindows(window, callback, lParam);
        }

        // iterate through child windows
        public static bool OnEnumWindow(IntPtr foundWindow, IntPtr lParam)
        {
            return true;
        }
    }
}
