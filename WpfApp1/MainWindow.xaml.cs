using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        sealed class CallerMemberNameAttribute: Attribute { }
        ObservableCollection<WindowSummary> windowSummaries = null;

        // NOTE: variables for setting up windows icon
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _programIndex = 0;
        public int ProgramIndex {
            get { return _programIndex; }
            set
            {
                Console.WriteLine("windowSummaries.Count: {0}", windowSummaries.Count);
                if (windowSummaries.Count < 1)
                {
                    return;
                }

                int upperBound = windowSummaries.Count - 1;

                if (value < 0)
                {
                    value = upperBound;
                }

                if (value > upperBound)
                {
                    value = 0;
                }

                if(_programIndex != value)
                {

                    _programIndex = value;
                    this.NotifyPropertyChanged();
                }
            }
        }


        public MainWindow()
        {

            InitializeComponent();

            // TODO: figure out what this does
            Style = (Style)FindResource(typeof(Window));
            Keyevent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            Console.WriteLine("App.Current.Windows:", App.Current.Windows);
            Console.WriteLine("Hello");
            Process[] processList = Process.GetProcesses();
            // TODO: Refactor this
            List<Process> mainProcessList = WindowSummaryManager.GetRunningPrograms();
            this.windowSummaries = WindowSummaryManager.getWindowSummaryInfo(mainProcessList);
            programList.ItemsSource = this.windowSummaries;
            Window window = Application.Current.MainWindow;
            DataContext = this;

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
        }

        public static void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("Alt Key: {0}", Keyboard.Modifiers == ModifierKeys.Alt);
            Console.WriteLine("Tab Key: {0}", e.SystemKey == Key.Tab);
            Console.WriteLine("F4 Key: {0}", e.SystemKey == Key.F4);

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Tab)
            {
                e.Handled = true;
            }
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

        private int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSSTRUCT lParam)
        {
            if (nCode >= 0)
            {
                switch (wParam)
                {
                    case 256: // WM_KEYDOWN
                    case 257: // WM_KEYUP
                    case 260: // WM_SYSKEYDOWN
                    case 261: // M_SYSKEYUP
                    Console.WriteLine("Pressing Shift?: {0}", (lParam.vkCode == 0xA0 || lParam.vkCode == 0xA1));
                    Console.WriteLine("Pressing Shift Key Code: {0}", lParam.vkCode);
                    Console.WriteLine("Mod", lParam.vkCode);
                    if ((lParam.vkCode == 0x09 && (Keyboard.Modifiers & ModifierKeys.Shift) != 0 && lParam.flags == 32)) {
                        Console.WriteLine("Pressing Alt + Shift + Tab");
                        ProgramIndex--;
                        return 1;
                    } else if ((lParam.vkCode == 0x09 && lParam.flags == 32)) {
                        Console.WriteLine("Pressing Alt + Tab");
                        ProgramIndex++;
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
