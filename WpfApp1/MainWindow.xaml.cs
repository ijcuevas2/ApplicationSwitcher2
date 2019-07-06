using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
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
        ObservableCollection<WindowSummary> filteredWindowSummaries = null;
        public Boolean isKeyboardShortcut = false;
        public int prevCaretIndex = 0;

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

        private String _text = "";
        public String Text
        {
            get { return _text;  }
            set
            {
                if (_text != value)
                {
                    Boolean addingText = value.Length > _text.Length;
                    _text = value;
                    // textBoxElement.CaretIndex = 2;

                    this.NotifyPropertyChanged();
                    this.updateWindowSummaries();
                    if (addingText)
                    {
                        this.incrementCursorIndex();
                    }

                    else
                    {
                        this.decrementCursorIndex();
                    }
                }
            }
        }
        
        // NOTE: notify property changed is not necessary here
        // used after updating text length
        private int _caretIndex;
        public int CaretIndex
        {
            get { return _caretIndex; }
            set
            {
                if (value < 0)
                {
                    _caretIndex = 0;
                    return;
                }

                int targetIndex = Text.Length + 1;

                if (_caretIndex > targetIndex)
                {
                    _caretIndex = targetIndex;
                }

                else
                {
                    _caretIndex = value;
                }
            }
        }

        public int getCurrCaretIndex()
        {
            return textBoxElement.CaretIndex;
        }

        public void updateWindowSummaries()
        {
            Console.WriteLine("updateWindowSummaries!!");
            // filteredWindowSummaries = new ObservableCollection<WindowSummary>();
            filteredWindowSummaries.Clear();
            for (int i = 0; i < windowSummaries.Count; i++)
            {
                String programName = windowSummaries[i].ProgramName.ToLower();
                String programWindowTitle = windowSummaries[i].ProgramWindowTitle.ToLower();
                String filterText = Text.ToLower();

                // TODO: Change name of variable
                Boolean isRelevantProgram = programName.Contains(Text) ||
                                            programWindowTitle.Contains(Text);
                if (isRelevantProgram)
                {
                    filteredWindowSummaries.Add(windowSummaries[i]);
                }
            }

        }

        public void decrementCursorIndex()
        {
            CaretIndex -= 1;
            Console.WriteLine("CaretIndex (decrement cursor): {0}", CaretIndex);
            textBoxElement.CaretIndex = CaretIndex;
        }

        public void incrementCursorIndex()
        {
            // CaretIndex = getCurrCaratIndex();
            // CaretIndex = 2;
            Console.WriteLine("CaretIndex Before Increment: {0}", prevCaretIndex);
            CaretIndex++;
            Console.WriteLine("CaretIndex After Increment: {0}", prevCaretIndex);
            textBoxElement.CaretIndex = CaretIndex;
            // Console.WriteLine("After CaratIndex: {0}", textBoxElement.CaretIndex);
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

            this.filteredWindowSummaries = new ObservableCollection<WindowSummary>();

            for (int i = 0; i < windowSummaries.Count; i++)
            {
                this.filteredWindowSummaries.Add(this.windowSummaries[i]);
            }


            programList.ItemsSource = filteredWindowSummaries;
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


            Boolean isAlphaNumeric = isAlphaNumericKeyPress(e.Key);

            if (isAlphaNumeric)
            {
                setTextBoxVisible();
            }
        }

        public Boolean isAlphaNumericKeyPress(Key key)
        {
            // int keyValue = (int)key;
            String keyString = key.ToString();
            if (keyString.Length != 1)
            {
                return false;
            }

            char currChar = keyString[0];

            Console.WriteLine("currChar: {0}", currChar);
            Console.WriteLine("key: {0}, key.ToString(): {1}", key, key.ToString());

            // letters, numbers, keypad
            return ((currChar >= 'A' && currChar <= 'Z'))
                || ((currChar >= 'a' && currChar <= 'z'))
                || ((currChar >= '0' && currChar <= '9'));
        }

        public Boolean isSpace(Key key)
        {
            // Console.WriteLine("key.ToString(): {0}", key.ToString());
            if (key == Key.Space)
            {
                return true;
            }

            return false;
        }

        public Boolean isBackSpace(Key key)
        {
            // Console.WriteLine("key.ToString(): {0}", key.ToString());
            if (key == Key.Back)
            {
                return true;
            }

            return false;
        }

        public Boolean isDelete(Key key)
        {
            if (key == Key.Delete)
            {
                return true;
            }

            return false;
        }

        public Boolean isKeyEquals(Key first, Key second)
        {
            return first == second;
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

        public void setCaretIndex(int index)
        {
            CaretIndex = index;
            textBoxElement.CaretIndex = CaretIndex;
        }

        private int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSSTRUCT lParam)
        {
            // Console.WriteLine("LowLevelKeyboardProc");
            CaretIndex = getCurrCaretIndex();
            Console.WriteLine("CaretIndex: {0}", CaretIndex);
            Console.WriteLine("textBoxElement.CaretIndex:{0}", textBoxElement.CaretIndex);

            if (nCode >= 0)
            {
                Boolean isKeyDown = wParam == 256 || wParam == 260;
                Boolean isKeyUp = wParam == 257 || wParam == 261;
                if (isKeyDown)
                {
                    // case 256: // WM_KEYDOWN
                    // case 260: // WM_SYSKEYDOWN
                    // Console.WriteLine("Pressing Shift?: {0}", (lParam.vkCode == 0xA0 || lParam.vkCode == 0xA1));
                    // Console.WriteLine("lParam.vkCode: {0}", lParam.vkCode);
                    Key currentKey = KeyInterop.KeyFromVirtualKey(lParam.vkCode);
                    // Console.WriteLine("isAlphaNumericKeyPress(currentKey): {0}", isAlphaNumericKeyPress(currentKey));
                    // Console.WriteLine("isSpace(currentKey): {0}", isSpace(currentKey));
                    // Console.WriteLine("Text: {0}", Text);
                    Boolean isShiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                    Boolean isAlt = lParam.flags == 32;
                    Boolean isCtrlModifier = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
                    Boolean isAltModifier = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
                    // Console.WriteLine("isAlt: {0}", isAlt);

                    if (isCtrlModifier && isKeyEquals(currentKey, Key.A))
                    {
                        goto NextHook;
                    }

                    if (isKeyEquals(currentKey, Key.Back))
                    {
                        if (Text.Length > 0)
                        {
                            int selectionStart = textBoxElement.SelectionStart;
                            int selectionLength = textBoxElement.SelectionLength;
                            int SelectionEnd = textBoxElement.SelectionStart + textBoxElement.SelectionLength;
                            // CaretIndex = getCurrCaretIndex();
                            Console.WriteLine("CaretIndex (In Backspace): {0}", CaretIndex);

                            if (selectionLength > 0)
                            {
                                setCaretIndex(selectionStart + 1);
                                Text = Text.Substring(0, selectionStart) + Text.Substring(SelectionEnd);
                            }

                            else if (selectionStart != 0)
                            {
                                Text = Text.Substring(0, selectionStart - 1) + Text.Substring(SelectionEnd);
                            }

                            else if (SelectionEnd == Text.Length)
                            {
                                Text = "";
                                setCaretIndex(0);
                            }

                            else 
                            {
                                Text = Text.Substring(SelectionEnd);
                            }
                        }

                        goto NextHook;
                    }

                    // TODO: make a dependency property
                    if (isKeyEquals(currentKey, Key.Home))
                    {
                        CaretIndex = 0;
                    }

                    if (isKeyEquals(currentKey, Key.Delete))
                    {
                        Boolean lastIndex = Text.Length == CaretIndex;
                        if (Text.Length > 0 && !lastIndex)
                        {
                            int SelectionStart = textBoxElement.SelectionStart;
                            int SelectionEnd = textBoxElement.SelectionStart + textBoxElement.SelectionLength;

                            Text = Text.Substring(0, SelectionStart) + Text.Substring(SelectionEnd + 1);
                            incrementCursorIndex();
                        }
                    }

                    // if (isSpace(currentKey))
                    // {
                    //     char currChar = ' ';
                    //     Text += currChar;
                    //     // textBoxElement.CaretIndex = Text.Length - 1;

                    //     if (isAlt)
                    //     {
                    //         return 1;
                    //     }

                    //     goto NextHook;
                    // }

                    if (isKeyEquals(currentKey, Key.RightShift))
                    {
                        // MainWindow_Hide();
                        Console.WriteLine("textBoxElement.SelectionStart: {0}", textBoxElement.SelectionStart);
                        Console.WriteLine("textBoxElement.SelectionLength: {0}", textBoxElement.SelectionLength);
                    }

                    // TODO: check for unnecessary decrementing or incrementing
                    if (isAltModifier)
                    {
                        if (isKeyEquals(currentKey, Key.Left))
                        {
                            decrementCursorIndex();
                        }

                        if (isKeyEquals(currentKey, Key.Right))
                        {
                            incrementCursorIndex();
                        }
                    }

                    if (isAlphaNumericKeyPress(currentKey))
                    {
                        setTextBoxVisible();

                        char currChar = currentKey.ToString()[0];

                        if (!isShiftKey)
                        {
                            currChar = Char.ToLower(currChar);
                        }

                        Text += currChar;
                        // textBoxElement.Car;
                        // if (isAlt)
                        // {
                        //     textBoxElement.Text += currChar;
                        // }

                        // NOTE: this is the fix
                        goto NextHook;
                    }

                    // Console.WriteLine("Mod", lParam.vkCode);
                    Boolean isAltTab = lParam.vkCode == 0x09 && lParam.flags == 32;
                    if (isAltTab)
                    {
                        this.ShowMainWindow();
                        isKeyboardShortcut = true;
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
                    // NOTE: Be Careful
                    if (isAlt)
                    {
                        // MainWindow_Hide();
                        // return goTo;
                    }
                }
            }

            NextHook:
            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        public void Main_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine("TextChanged!: {0}", textBoxElement.Text);
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
            Text = "";
            // this.Hide();
        }

        // TODO: Figure out that this gets called properly
        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
        }
    }
}
