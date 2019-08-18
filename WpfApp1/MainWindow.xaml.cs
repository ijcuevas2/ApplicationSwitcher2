using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Screen = System.Windows.Forms.Screen;


namespace ApplicationSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        sealed class CallerMemberNameAttribute : Attribute { }
        ObservableCollection<WindowSummary> windowSummaries = null;
        ObservableCollection<WindowSummary> filteredWindowSummaries = null;
        public Boolean isKeyboardShortcut = false;
        public int prevCaretIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            // TODO: figure out what this does
            Style = (Style)FindResource(typeof(Window));
            Keyevent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            initializeWindowSummaryList();
            programList.ItemsSource = filteredWindowSummaries;
            Window window = Application.Current.MainWindow;
            DataContext = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            MainWindow_Hide();
        }

        private void initializeWindowSummaryList()
        {
            Process[] processList = Process.GetProcesses();
            // TODO: Refactor this
            List<Process> mainProcessList = WindowSummaryManager.GetRunningPrograms();
            this.windowSummaries = WindowSummaryManager.getWindowSummaryInfo(mainProcessList);

            this.filteredWindowSummaries = new ObservableCollection<WindowSummary>();

            for (int i = 0; i < windowSummaries.Count; i++)
            {
                this.filteredWindowSummaries.Add(this.windowSummaries[i]);
            }
        }

        private void clearWindowSummaries()
        {
            // TODO: Refactor this section
            this.windowSummaries.Clear();
            MyEnumWindows.childWindowSummaries.Clear();
            this.filteredWindowSummaries.Clear();
        }

        private void reloadWindowSummaryList()
        {
            Process[] processList = Process.GetProcesses();

            // TODO: Refactor this
            clearWindowSummaries();
            List<Process> mainProcessList = WindowSummaryManager.GetRunningPrograms();

            this.windowSummaries = WindowSummaryManager.getWindowSummaryInfo(mainProcessList);


            for (int i = 0; i < windowSummaries.Count; i++)
            {
                this.filteredWindowSummaries.Add(this.windowSummaries[i]);
            }
        }


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private int _programIndex = 0;
        public int ProgramIndex {
            get { return _programIndex; }
            set
            {
                if (filteredWindowSummaries.Count < 1)
                {
                    return;
                }

                int upperBound = filteredWindowSummaries.Count - 1;

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
        
        // NOTE: NotifyPropertyChanged is not called here
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
            filteredWindowSummaries.Clear();
            for (int i = 0; i < windowSummaries.Count; i++)
            {
                String programName = windowSummaries[i].ProgramName.ToLower();
                String programWindowTitle = windowSummaries[i].ProgramWindowTitle.ToLower();
                String filterText = Text.ToLower();

                // TODO: Change name of variable
                Boolean isRelevantProgram = programName.Contains(filterText) ||
                                            programWindowTitle.Contains(filterText);
                if (isRelevantProgram)
                {
                    filteredWindowSummaries.Add(windowSummaries[i]);
                }
            }
        }

        public void decrementCursorIndex()
        {
            CaretIndex -= 1;
            System.Diagnostics.Debug.WriteLine("CaretIndex (decrement cursor): {0}", CaretIndex);
            textBoxElement.CaretIndex = CaretIndex;
        }

        public void incrementCursorIndex()
        {
            System.Diagnostics.Debug.WriteLine("CaretIndex Before Increment: {0}", prevCaretIndex);
            CaretIndex++;
            System.Diagnostics.Debug.WriteLine("CaretIndex After Increment: {0}", prevCaretIndex);
            textBoxElement.CaretIndex = CaretIndex;
        }

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
            String keyString = key.ToString();
            if (keyString.Length != 1)
            {
                return false;
            }

            char currChar = keyString[0];

            // letters, numbers, keypad
            return ((currChar >= 'A' && currChar <= 'Z'))
                || ((currChar >= 'a' && currChar <= 'z'))
                || ((currChar >= '0' && currChar <= '9'));
        }

        public Boolean isSpace(Key key)
        {
            if (key == Key.Space)
            {
                return true;
            }

            return false;
        }

        public Boolean isBackSpace(Key key)
        {
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("e.SystemKey: {0}", e.SystemKey);
            System.Diagnostics.Debug.WriteLine("KeyModifiers: {0}", Keyboard.Modifiers);
            System.Diagnostics.Debug.WriteLine("e.Key: {0}", e.Key);

            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }

            else
            {
                base.OnKeyDown(e);
            }
        }

        public void setCaretIndex(int index)
        {
            CaretIndex = index;
            textBoxElement.CaretIndex = CaretIndex;
        }

        private int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSSTRUCT lParam)
        {
            CaretIndex = getCurrCaretIndex();
            System.Diagnostics.Debug.WriteLine("CaretIndex: {0}", CaretIndex);
            System.Diagnostics.Debug.WriteLine("textBoxElement.CaretIndex:{0}", textBoxElement.CaretIndex);

            if (nCode >= 0)
            {
                Boolean isKeyDown = wParam == 256 || wParam == 260;
                Boolean isKeyUp = wParam == 257 || wParam == 261;
                if (isKeyDown)
                {
                    Key currentKey = KeyInterop.KeyFromVirtualKey(lParam.vkCode);
                    Boolean isShiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                    Boolean isAlt = lParam.flags == 32;
                    Boolean isCtrlModifier = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
                    Boolean isAltModifier = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;

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
                            System.Diagnostics.Debug.WriteLine("CaretIndex (In Backspace): {0}", CaretIndex);

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

                    if (isSpace(currentKey))
                    {
                        char currChar = ' ';
                        Text += currChar;
                    }

                    if (isKeyEquals(currentKey, Key.RightShift))
                    {
                        // MainWindow_Hide();
                        System.Diagnostics.Debug.WriteLine("textBoxElement.SelectionStart: {0}", textBoxElement.SelectionStart);
                        System.Diagnostics.Debug.WriteLine("textBoxElement.SelectionLength: {0}", textBoxElement.SelectionLength);
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

                        // NOTE: this is the fix
                        goto NextHook;
                    }

                    Boolean isAltTab = lParam.vkCode == 0x09 && lParam.flags == 32;
                    if (isAltTab)
                    {
                        if (!this.IsVisible)
                        {
                            this.MainWindow_Show();
                            return 1;
                        }

                        isKeyboardShortcut = true;
                        if (isShiftKey) {
                            ProgramIndex--;
                        } else {
                            ProgramIndex++;
                        }

                        isKeyboardShortcut = false;
                        return 1;
                    }
                }

                if (isKeyUp)
                {
                    Boolean isAlt = lParam.vkCode == 0xA4 || lParam.vkCode == 0xA5;

                    // NOTE: Be Careful
                    if (isAlt)
                    {
                        System.Diagnostics.Debug.WriteLine("MainWindow_Hide");
                        MainWindow_Hide();
                        ProcessItemSwitch();
                    }
                }
            }

            NextHook:
            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        public void Main_TextChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TextChanged!: {0}", textBoxElement.Text);
        }

        public void resetProgramIndex()
        {
            if (filteredWindowSummaries.Count > 1)
            {
                this.ProgramIndex = 1;
            }

            else
            {
                this.ProgramIndex = 0;
            }
        }

        public void MainWindow_Show()
        {
            reloadWindowSummaryList();
            this.Text = "";
            this.setTextBoxCollapsed();
            this.resetProgramIndex();
            this.NotifyPropertyChanged();
            this.Show();
            this.Activate();
        }

        public void MainWindow_Hide()
        {
            this.Hide();
        }

        void HandlerForCM(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        public void ProcessItemSwitch()
        {
            int index = programList.SelectedIndex;
            WindowSummary currSummary = filteredWindowSummaries[index];

            // TODO: Handle Exception
            try
            {
                currSummary.Element.SetFocus();
            }

            catch (Exception)
            {
                MainWindow_Hide();
            }
        }

        // TODO: Figure out that this gets called properly
        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook);
        }

        public void SetTargetDisplay()
        {
            string targetDeviceName = WpfApp1.Properties.Settings.Default.DisplayDevice;
            if(!String.IsNullOrEmpty(targetDeviceName))
            {
                var screen = (from s in Screen.AllScreens
                              where s.DeviceName.ToLower().Equals(targetDeviceName.ToLower())
                              select s).FirstOrDefault();
                if (screen != null)
                {
                    Left = screen.WorkingArea.Left;
                    Top = screen.WorkingArea.Top;
                    Width = screen.WorkingArea.Width;
                    Height = screen.WorkingArea.Height;
                }
            }
        }
    }
}
