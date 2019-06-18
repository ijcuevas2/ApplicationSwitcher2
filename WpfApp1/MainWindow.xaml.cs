using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
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
        public Boolean isKeyboardShortcut = false;

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

        private String _textBoxVisibility = "Collapsed";
        public String TextBoxVisibility
        {
            get { return _textBoxVisibility; }
            set
            {
                if (_textBoxVisibility != value)
                {
                    _textBoxVisibility = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public void setTextBoxVisible()
        {
            TextBoxVisibility = "Visible";
            textBoxElement.Focus();
        }

        public void setTextBoxCollapsed()
        {
            TextBoxVisibility = "Collapsed";
        }

        public MainWindow()
        {

            InitializeComponent();

            // TODO: figure out what this does
            Style = (Style)FindResource(typeof(Window));
            Keyevent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            // Console.WriteLine("App.Current.Windows:", App.Current.Windows);
            // Console.WriteLine("Hello");
            Process[] processList = Process.GetProcesses();
            // TODO: Refactor this
            List<Process> mainProcessList = WindowSummaryManager.GetRunningPrograms();
            this.windowSummaries = WindowSummaryManager.getWindowSummaryInfo(mainProcessList);
            programList.ItemsSource = this.windowSummaries;
            // programList.MouseEnter += new MouseEventHandler(Mouse_Enter);
            Window window = Application.Current.MainWindow;
            DataContext = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            // MainWindow_Hide();
        }

        // public static void Mouse_Enter(object sender, MouseEventArgs e)
        // {
        //     programList.SelectedItem = e.
        // }

        public void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Tab)
            {
                e.Handled = true;
            }

            if (e.Key == Key.Escape)
            {
                MainWindow_Hide();
            }

            Boolean isAlphaNumeric = isAlphaNumericKeyPress(e.Key);
            // Console.WriteLine("e.Key: {0}", (int)e.Key);
            // Console.WriteLine("isAlphaNumeric: {0}", isAlphaNumeric);

            if (isAlphaNumeric)
            {
                setTextBoxVisible();
            }
        }

        public Boolean isAlphaNumericKeyPress(Key key)
        {
            int keyValue = (int)key;

            // letters, numbers, keypad
            return ((keyValue >= 0x30 && keyValue <= 0x39))
                || ((keyValue >= 0x41 && keyValue <= 0x5A))
                || ((keyValue >= 0x60 && keyValue <= 0x69));
        }

        private void PreviewMouse_Move(object sender, MouseEventArgs e)
        {
            if (isKeyboardShortcut)
            {
                return;
            }

            FrameworkElement ctrl = (e.OriginalSource as FrameworkElement);
            if (ctrl != null)
            {
                WindowSummary windowSummary = ctrl.DataContext as WindowSummary;
                if (windowSummary != null)
                {
                    programList.SelectedItem = windowSummary;
                }
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
            // Console.WriteLine("LowLevelKeyboardProc");
            if (nCode >= 0)
            {
                // Console.WriteLine("wParam: {0}", wParam);
                Boolean isKeyDown = wParam == 256 || wParam == 260;
                Boolean isKeyUp = wParam == 257 || wParam == 261;
                if (isKeyDown)
                {
                    // case 256: // WM_KEYDOWN
                    // case 260: // WM_SYSKEYDOWN
                    // Console.WriteLine("Pressing Shift?: {0}", (lParam.vkCode == 0xA0 || lParam.vkCode == 0xA1));
                    // Console.WriteLine("Pressing Shift Key Code: {0}", lParam.vkCode);
                    // Console.WriteLine("Mod", lParam.vkCode);
                    Boolean isAltTab = lParam.vkCode == 0x09 && lParam.flags == 32;
                    if (isAltTab)
                    {
                        this.ShowMainWindow();
                        isKeyboardShortcut = true;
                        Boolean isShiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                        Console.WriteLine("isShiftKey: {0}", isShiftKey);
                        if (isShiftKey) {
                            Console.WriteLine("Pressing Alt + Shift + Tab");
                            ProgramIndex--;
                        } else {
                            Console.WriteLine("Pressing Alt + Tab");
                            ProgramIndex++;
                        }

                        isKeyboardShortcut = false;
                        return 1;
                    }
                }

                // case 257: // WM_KEYUP
                // case 261: // WM_SYSKEYUP
                // break;
                if (isKeyUp)
                {
                    // Boolean isAlt = lParam.flags == 32;
                    Boolean isAlt = lParam.vkCode == 0xA4 || lParam.vkCode == 0xA5;
                    // Console.WriteLine("isAlt: {0}", isAlt);
                    // Console.WriteLine("lParam.vkCode: {0}", lParam.vkCode);
                    if (isAlt)
                    {
                        MainWindow_Hide();
                        return 1;
                    }

                }
            }

            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        public void ShowMainWindow()
        {
            // Console.WriteLine("MainWindow_Show()");
            // Console.WriteLine("this.IsVisible: {0}", this.IsVisible);
            // if (this.IsVisible)
            // {
            //     if (this.WindowState == WindowState.Minimized)
            //     {
            //         this.WindowState = WindowState.Normal;
            //     }
            //     else
            //     {
            //         this.Show();
            //     }
            // }

            this.Show();
        }

        public void MainWindow_Hide()
        {
            Console.WriteLine("MainWindow_Hide()");
            setTextBoxCollapsed();
            this.Hide();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
        }
    }
}
