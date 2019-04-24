using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Keyevent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            Console.WriteLine("App.Current.Windows:", App.Current.Windows);
            Console.WriteLine("Hello");
            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
            {
                Console.WriteLine("Process: {0} Id: {1}", process.ProcessName, process.Id);
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(" ");
            }

            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    Console.WriteLine("Process: {0} Id: {1} Window title: {2} Process File Path: {3}", process.ProcessName, process.Id, process.MainWindowTitle, process.MainModule.FileName);
                    if (process.ProcessName == "chrome")
                    {
                        AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                        Icon programIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                        Bitmap bmp = programIcon.ToBitmap();
                        ImageCodecInfo programImageCodeInfo = GetEncoderInfo("image/jpeg");
                        Encoder qualityEncoder = Encoder.Quality;
                        EncoderParameter programEncoderParameter = new EncoderParameter(qualityEncoder, 25L);
                        EncoderParameters encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = programEncoderParameter;
                        bmp.Save(@"C:\Users\Ismael Cuevas\Documents\example.jpg", programImageCodeInfo, encoderParameters);
                 
                        // if (element != null)
                        // {
                        //     element.SetFocus();
                        // }
                    }
                }
            }

        }

        // NOTE: Code for checking if things work
        // if (process.ProcessName == "chrome")
        // {
        //     AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
        //     Icon programIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);
        //     Bitmap bmp = programIcon.ToBitmap();
        //     ImageCodecInfo programImageCodeInfo = GetEncoderInfo("image/jpeg");
        //     Encoder qualityEncoder = Encoder.Quality;
        //     EncoderParameter programEncoderParameter = new EncoderParameter(qualityEncoder, 25L);
        //     EncoderParameters encoderParameters = new EncoderParameters(1);
        //     encoderParameters.Param[0] = programEncoderParameter;
        //     bmp.Save(@"C:\Users\Ismael Cuevas\Documents\example.jpg", programImageCodeInfo, encoderParameters);
     
        //     // if (element != null)
        //     // {
        //     //     element.SetFocus();
        //     // }
        // }

        // private void Window_Loaded(object sender, RoutedEventArgs e)
        // {
        //     this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        // }

        public static void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("Alt Key: {0}", Keyboard.Modifiers == ModifierKeys.Alt);
            Console.WriteLine("Tab Key: {0}", e.SystemKey == Key.Tab);
            Console.WriteLine("F4 Key: {0}", e.SystemKey == Key.F4);

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                Console.WriteLine("Pressing Alt!!");
                // Console.WriteLine("Pressing Alt + Tab!!");
            } else if (e.Key == Key.Tab) {
                Console.WriteLine("Pressing Tab!!");
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageDecoders();
            for (int i = 0; i < encoders.Length; i++)
            {
                if (encoders[i].MimeType == mimeType)
                {
                    return encoders[i];
                }
            }
            return null;
        }

        /* Code to disable WinKey, Alt + Tab, Ctrl + Esc */
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSSTRUCT IParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate Ipfn, IntPtr hMod, int dwThreadId);
        
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSSTRUCT IParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(IntPtr path);

        private IntPtr hHook;
        LowLevelKeyboardProcDelegate hookProc; // prevent gc?
        const int WH_KEYBOARD_LL = 13;

        public void Keyevent()
        {
            IntPtr hModule = GetModuleHandle(IntPtr.Zero);
            hookProc = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hModule, 0);
            if (hHook == IntPtr.Zero)
            {
                MessageBox.Show("Failed to set hook, error = " + Marshal.GetLastWin32Error());
            }
        }

        private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSSTRUCT lParam)
        {
            if (nCode >= 0)
            {
                switch (wParam)
                {
                    case 256: // WM_KEYDOWN
                    case 257: // WM_KEYUP
                    case 260: // WM_SYSKEYDOWN
                    case 261: // M_SYSKEYUP
                    if ((lParam.vkCode == 0x09 && lParam.flags == 32)) {
                        Console.WriteLine("Pressing Alt + Tab");
                        return 1;
                    }
                    break;
                }

            }

            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
        }
    }
}
