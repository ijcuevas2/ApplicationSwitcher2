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
            SetTargetDisplay();
            MainWindow_Show();
            //MainWindow_Hide();
        }

        public void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ContentPresenter).HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
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

        //public static string Truncate(this string value)
        //{
        //    return value.Length <= 20 ? value : value.Substring(0, 20) + "...";
        //}

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
                //Console.WriteLine("In ProgramIndex: {0}", _programIndex);
                //Console.WriteLine("value: {0}", value);
                //Console.WriteLine("filteredWindowSummaries.Count: {0}", filteredWindowSummaries.Count);
                if (filteredWindowSummaries.Count < 1)
                {
                    return;
                }

                // TODO: refactor code for edge case
                //if (filteredWindowSummaries.Count == 1)
                //{
                //    _programIndex = 0;
                //    return;
                //}

                int upperBound = filteredWindowSummaries.Count - 1;

                if (value < 0)
                {
                    value = upperBound;
                }

                if (value > upperBound)
                {
                    value = 0;
                }

                if (_programIndex != value)
                {
                    _programIndex = value;
                    Console.WriteLine("_programIndex in conditional: {0}", _programIndex);
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


        // NOTE: NotifyPropertyChanged is not called here
        public int CaretIndex
        {
            get { return textBoxElement.CaretIndex; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value > Text.Length)
                {
                    value = Text.Length;
                }


                if (value != textBoxElement.CaretIndex) {
                    textBoxElement.CaretIndex = value;
                }
            }
        }

        public string Text
        {
            get {
                System.Diagnostics.Debug.Print("Text Getter");
                return textBoxElement.Text;
            }

            set {
                System.Diagnostics.Debug.Print("Text Setter");
                textBoxElement.Text = value;
            }
        }

        public string SelectedText
        {
            get {
                return textBoxElement.SelectedText;
            }

            set {
                textBoxElement.SelectedText = value;
            }
        }

        public void updateWindowSummaries()
        {
            filteredWindowSummaries.Clear();
            System.Diagnostics.Debug.Print("updateWindowSummaries Text: {0}", textBoxElement);
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

            // this.NotifyPropertyChanged();
            if (this.filteredWindowSummaries.Count > 0)
            {
                this.programList.SelectedItem = this.filteredWindowSummaries[0];
            }
        }


        public void decrementCursorIndex() {
            if (SelectedText.Length > 0)
            {
                textBoxElement.SelectionLength = 0;
                return;
            }

            CaretIndex--;
        }

        public void incrementCursorIndex()
        {
            if (SelectedText.Length > 0)
            {
                int targetIndex = CaretIndex + textBoxElement.SelectionLength;
                CaretIndex = targetIndex;
                return;
            }

            CaretIndex++;
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
            //System.Diagnostics.Debug.WriteLine("e.SystemKey: {0}", e.SystemKey);
            //System.Diagnostics.Debug.WriteLine("KeyModifiers: {0}", Keyboard.Modifiers);
            //System.Diagnostics.Debug.WriteLine("e.Key: {0}", e.Key);

            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }

            //else
            //{
            //    base.OnKeyDown(e);
            //}
        }

        private int? selectionPivot = null;
        public void SetTextSelection(bool rightDir)
        {
            if (SelectedText.Length == 0 && selectionPivot == null)
            {
                selectionPivot = CaretIndex;
                if (rightDir)
                {
                    //textBoxElement.SelectionStart = CaretIndex;
                    //textBoxElement.SelectionLength += 1;
                    textBoxElement.Select(CaretIndex, 1);
                }

                else
                {
                    //textBoxElement.SelectionStart = CaretIndex - 1;
                    //textBoxElement.SelectionLength += 1;
                    if (CaretIndex > 0)
                    {
                        textBoxElement.Select(CaretIndex - 1, 1);
                    }
                }

                return;
            }

            if (rightDir)
            {
                if (CaretIndex < selectionPivot)
                {
                    textBoxElement.Select(CaretIndex + 1, SelectedText.Length - 1);
                }

                else
                {
                    int targetLength = SelectedText.Length + 1;
                    if (CaretIndex + targetLength <= Text.Length)
                    {
                        textBoxElement.Select(CaretIndex, targetLength);
                    }
                }
            }

            else
            {
                if (CaretIndex == selectionPivot && textBoxElement.SelectionLength == 1)
                {
                    textBoxElement.SelectionLength = 0;
                }

                int currentSelectionSpan = CaretIndex + textBoxElement.SelectionLength;

                if (currentSelectionSpan <= selectionPivot)
                {
                    if (CaretIndex > 0)
                    {
                        textBoxElement.Select(CaretIndex - 1, SelectedText.Length + 1);
                    }
                }

                else if (currentSelectionSpan > selectionPivot)
                {
                    textBoxElement.Select(CaretIndex, SelectedText.Length - 1);
                }
            }
        }

        public void textBoxElementSelection(int startIndex, int selectionLength)
        {
            textBoxElement.SelectionLength = 0;
            textBoxElement.Select(startIndex, selectionLength);
        }

        private int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSSTRUCT lParam)
        {
            //System.Diagnostics.Debug.Print("LowLevelKeyboardProc");
            if (nCode >= 0)
            {
                bool isKeyDown = wParam == 256 || wParam == 260;
                bool isKeyUp = wParam == 257 || wParam == 261;
                if (isKeyDown)
                {
                    Key currentKey = KeyInterop.KeyFromVirtualKey(lParam.vkCode);
                    bool isShiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                    bool isCtrlModifier = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
                    bool isShiftModifier = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                    bool isAltModifier = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;

                    if (isCtrlModifier && isKeyEquals(currentKey, Key.A))
                    {
                        textBoxElement.SelectAll();
                        goto NextHook;
                    }

                    if (isKeyEquals(currentKey, Key.RightCtrl))
                    {
                        System.Diagnostics.Debug.Print("_programIndex: {0}", _programIndex);
                        System.Diagnostics.Debug.Print("ProgramIndex: {0}", ProgramIndex);
                        System.Diagnostics.Debug.Print("SelectedIndex: {0}", programList.SelectedIndex);
                        System.Diagnostics.Debug.Print("SelectedItem: {0}", programList.SelectedItem);
                        System.Diagnostics.Debug.Print("CaretIndex: {0}", textBoxElement.CaretIndex);
                        goto NextHook;
                    }

                    if (isKeyEquals(currentKey, Key.Back))
                    {
                        if (SelectedText.Length > 0)
                        {
                            SelectedText = "";
                            goto NextHook;
                        }

                        if (CaretIndex < 2)
                        {
                            Text = Text.Substring(CaretIndex);
                        }

                        else
                        {
                            int prevCaretIndex = CaretIndex;
                            int upperTarget = Math.Min(Text.Length, CaretIndex);
                            Text = Text.Substring(0, CaretIndex - 1) + Text.Substring(upperTarget);
                            CaretIndex = prevCaretIndex - 1;
                        }

                        goto NextHook;
                    }

                    if (isKeyEquals(currentKey, Key.Delete))
                    {
                        if (SelectedText.Length > 0)
                        {
                            SelectedText = "";
                        }

                        else
                        {
                            int prevCaretIndex = CaretIndex;
                            int upperTarget = Math.Min(Text.Length, CaretIndex + 1);
                            Text = Text.Substring(0, CaretIndex) + Text.Substring(upperTarget);
                            CaretIndex = prevCaretIndex;
                        }
                    }

                    if (isKeyEquals(currentKey, Key.Home))
                    {
                        CaretIndex = 0;
                    }

                    if (isKeyEquals(currentKey, Key.End))
                    {
                        CaretIndex = Text.Length;
                    }

                    if (isKeyEquals(currentKey, Key.PageUp))
                    {
                        textBoxElement.CaretIndex = 5;
                    }

                    // TODO: check for unnecessary decrementing or incrementing
                    if (isKeyEquals(currentKey, Key.Left))
                    {
                        if (isShiftModifier)
                        {
                            if (isCtrlModifier)
                            {
                                textBoxElementSelection(0, CaretIndex);
                            }

                            else
                            {
                                SetTextSelection(false);
                            }

                            goto NextHook;
                        }

                        decrementCursorIndex();
                        goto NextHook;
                    }

                    if (isKeyEquals(currentKey, Key.Right))
                    {

                        System.Diagnostics.Debug.Print("SelectedText Update");
                        if (isShiftModifier)
                        {
                            System.Diagnostics.Debug.Print("CtrlModifier SelectedText Update");
                            if (isCtrlModifier)
                            {
                                CaretIndex += SelectedText.Length;
                                textBoxElementSelection(CaretIndex, Text.Length - CaretIndex);
                            }

                            else
                            {
                                SetTextSelection(true);
                            }

                            goto NextHook;
                        }

                        incrementCursorIndex();
                    }

                    // TODO: Refactor this section
                    if (isKeyEquals(currentKey, Key.Space))
                    {
                        if (Text.Length == 0)
                        {
                            goto NextHook;
                        }

                        string currChar = " ";
                        textBoxElement.AppendText(currChar);
                    }

                    if (isAlphaNumericKeyPress(currentKey))
                    {
                        setTextBoxVisible();

                        char currChar = currentKey.ToString()[0];

                        if (!isShiftKey)
                        {
                            currChar = Char.ToLower(currChar);
                        }

                        string currString = currChar.ToString();
                        int currIndex = CaretIndex;
                        string targetText = Text.Insert(CaretIndex, currString);
                        System.Diagnostics.Debug.Print("targetText: {0}", targetText);
                        Text = targetText;
                        CaretIndex = currIndex + 1;

                        // NOTE: this is the fix
                        goto NextHook;
                    }

                    if (isKeyEquals(currentKey, Key.Up))
                    {
                        ProgramIndex--;
                    }

                    if (isKeyEquals(currentKey, Key.Down))
                    {
                        ProgramIndex++;
                    }

                    if (isKeyEquals(currentKey, Key.Enter))
                    {
                        navigateToProgram();
                    }


                    bool isAltTab = lParam.vkCode == 0x09 && lParam.flags == 32;
                    if (isAltTab)
                    {
                        if (!this.IsVisible)
                        {
                            this.MainWindow_Show();
                            return 1;
                        }

                        isKeyboardShortcut = true;
                        if (isShiftKey)
                        {
                            Console.WriteLine("Keyboard Shortcut Decreasing Index");
                            ProgramIndex--;
                        }
                        else
                        {
                            Console.WriteLine("Keyboard Shortcut Increasing Index");
                            ProgramIndex++;
                        }

                        isKeyboardShortcut = false;
                        return 1;
                    }
                }

                if (isKeyUp)
                {
                    bool isAlt = lParam.vkCode == 0xA4 || lParam.vkCode == 0xA5;

                    // NOTE: Be Careful
                    if (isAlt)
                    {
                        System.Diagnostics.Debug.WriteLine("MainWindow_Hide");
                        navigateToProgram();
                    }

                    if ((Keyboard.IsKeyUp(Key.LeftShift) || Keyboard.IsKeyUp(Key.RightShift))
                        && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    {
                        selectionPivot = null;
                    }
                }
            }

            NextHook:
            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        public void navigateToProgram()
        {
            MainWindow_Hide();
            ProcessItemSwitch();
        }

        public void Main_TextChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TextChanged!: {0}", textBoxElement.Text);
        }

        public void resetProgramIndex()
        {
            if (filteredWindowSummaries.Count > 1)
            {
                this._programIndex = 1;
            }

            else
            {
                this._programIndex = 0;
            }
        }

        public void MainWindow_Show()
        {
            reloadWindowSummaryList();
            this.Text = "";
            this.setTextBoxCollapsed();
            this.resetProgramIndex();
            this.NotifyPropertyChanged();
            this.GiveFocus();
        }

        public void GiveFocus()
        {
            this.Show();
            this.Activate();
            this.Focus();
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
            // int index = programList.SelectedIndex;
            int index = ProgramIndex;

            // TODO: Handle Exception
            try
            {
                WindowSummary currSummary = filteredWindowSummaries[index];
                currSummary.Element.SetFocus();
            }

            catch (Exception)
            {
                MainWindow_Hide();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("Window Closed");
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

        public void textBoxElement_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Text Changed!!");
            this.updateWindowSummaries();
        }
    }
}
