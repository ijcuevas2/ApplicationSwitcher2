using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace WpfApp1
{
    
    public class WindowSummary
    {
        // TODO: include window desktop number
        public ImageSource ProgramIcon { get; set; }
        public string ProgramName { get; set; }
        public string ProgramWindowTitle { get; set; }
    }

    public class ProgramSummary
    {
        public Process mainWindowProcess;
        public List<ChildWindowSummary> childProgramSummaries;

        public ProgramSummary(Process process)
        {
            mainWindowProcess = process;
            childProgramSummaries = new List<ChildWindowSummary>();
        }

        public void AddChildWindowSummary(ChildWindowSummary childWindowSummary)
        {
            childProgramSummaries.Add(childWindowSummary);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public long dwStyle;
        public long dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;

        public WINDOWINFO(Boolean? filler): this() // Allows for automatic initialization of "cbSize" with "new WINDOWINFO(null / true / false)"
        {
            cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
        }
    }

    public class WindowSummaryManager
    {
        // List<List<Process>> programProcessList; 
        public static Dictionary<uint, ProgramSummary> programSummaryDict = new Dictionary<uint, ProgramSummary>();

        public static List<Process> GetRunningPrograms()
        {
            Console.WriteLine("GetRunningPrograms()");
            Process[] processList = Process.GetProcesses();
            List<Process> mainProcessList = new List<Process>();

            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    mainProcessList.Add(process);
                    
                    ProgramSummary currProgramSummary = new ProgramSummary(process);
                    uint processId = (uint)process.Id;
                    programSummaryDict.Add(processId, currProgramSummary);

                    // programSummaryDict[processId].AddProgramSummary(sample);
                    // programSummaryDict[processId];
                    
                    Process[] currentProcessArr = Process.GetProcessesByName(process.ProcessName);

                    // NOTE: debug code
                    // foreach (Process subProcess in currentProcessArr)
                    // {
                    //     Console.WriteLine("CurrentProcessName: {0}, processId: {1}, mainWindowHandle: {2}", subProcess.ProcessName, subProcess.Id, subProcess.MainWindowHandle);
                    //     // process
                    // }

                    // public int 
                    // List<Process> currentProcessList = new List<Process>();
                    // programProcessList.Add(currentProcessList);
                    // MyEnumWindows.EnumWindowsCallback(process.MainWindowHandle, (IntPtr)1);
                }
            }

            // NOTE: Done Adding Processes 

            MyEnumWindows.GetWindowTitles(true);
            Console.WriteLine("MyEnumWindows Result");
            foreach (ChildWindowSummary childWindowSummary in MyEnumWindows.childWindowSummaries)
            {
                ProgramSummary currSummary = null;
                if (programSummaryDict.TryGetValue(childWindowSummary.lpdwProcessId, out currSummary))
                {
                    currSummary.AddChildWindowSummary(childWindowSummary);
                }
            }

            Console.WriteLine("Done Adding Program Summaries");
            foreach(KeyValuePair<uint, ProgramSummary> entry in programSummaryDict)
            {
                Console.WriteLine("Current childProgramSummaries Length: {0}", entry.Value.childProgramSummaries.Count);
                foreach (ChildWindowSummary summary in entry.Value.childProgramSummaries)
                {
                    Console.WriteLine("Summary Title: {0}", summary.title);
                    Console.WriteLine("Summary Process: {0}", summary.lpdwProcessId);

                    WINDOWINFO info = new WINDOWINFO();
                    info.cbSize = (UInt32)Marshal.SizeOf(info);
                    MyEnumWindows.GetWindowInfo(summary.windowHandle, ref info);
                    processWindowInfo(info);
                }
            }


            return mainProcessList;
        }

        private static void processWindowInfo(WINDOWINFO info)
        {
            Dictionary<string, long> windowStyles = new Dictionary<string, long>();

            //windowStyles.Add("WS_BORDER", 0x00800000L);
            //windowStyles.Add("WS_CAPTION", 0x00C00000L);
            //windowStyles.Add("WS_CHILD", 0x40000000L);
            // windowStyles.Add("WS_CHILDWINDOW", 0x40000000L);
            //windowStyles.Add("WS_CLIPCHILDREN", 0x02000000L);
            //windowStyles.Add("WS_CLIPSIBLINGS", 0x04000000L);
            //windowStyles.Add("WS_DISABLED", 0x08000000L);
            //windowStyles.Add("WS_DLGFRAME", 0x00400000L);
            //windowStyles.Add("WS_GROUP", 0x00020000L);
            //windowStyles.Add("WS_HSCROLL", 0x00100000L);
            //windowStyles.Add("WS_ICONIC", 0x20000000L);
            windowStyles.Add("WS_MAXIMIZE", 0x01000000L);
            //windowStyles.Add("WS_MAXIMIZEBOX", 0x00010000L);
            windowStyles.Add("WS_MINIMIZE", 0x20000000L);
            //windowStyles.Add("WS_MINIMIZEBOX", 0x00020000L);
            //windowStyles.Add("WS_OVERLAPPED", 0x00000000L);
            //windowStyles.Add("WS_POPUP", 0x80000000L);
            //windowStyles.Add("WS_SIZEBOX", 0x00040000L);
            //windowStyles.Add("WS_SYSMENU", 0x00080000L);
            //windowStyles.Add("WS_TABSTOP", 0x00010000L);
            //windowStyles.Add("WS_THICKFRAME", 0x00040000L);
            //windowStyles.Add("WS_TILED", 0x00000000L);
            //windowStyles.Add("WS_VISIBLE", 0x10000000L);
            //windowStyles.Add("WS_VSCROLL", 0x00200000L);

            // NOTE: Not necessary I guess?
            Dictionary<string, long> extendedWindowStyles = new Dictionary<string, long>();
            //extendedWindowStyles.Add("WS_EX_ACCEPTFILES", 0x00000010L);
            //extendedWindowStyles.Add("WS_EX_APPWINDOW", 0x00040000L);
            //extendedWindowStyles.Add("WS_EX_CLIENTEDGE", 0x00000010L);
            //extendedWindowStyles.Add("WS_EX_COMPOSITED", 0x02000000L);
            //extendedWindowStyles.Add("WS_EX_CONTEXTHELP", 0x00000400L);
            //extendedWindowStyles.Add("WS_EX_CONTROLPARENT", 0x00010000L);
            //extendedWindowStyles.Add("WS_EX_DLGMODALFRAME", 0x00000001L);
            //extendedWindowStyles.Add("WS_EX_LAYERED", 0x00080000);
            //extendedWindowStyles.Add("WS_EX_LAYOUTRTL", 0x00400000L);
            //extendedWindowStyles.Add("WS_EX_LEFTSCROLLBAR", 0x00004000L);
            //extendedWindowStyles.Add("WS_EX_LTRREADING", 0x00000000L);
            //extendedWindowStyles.Add("WS_EX_MDICHILD", 0x00000040L);
            //extendedWindowStyles.Add("WS_EX_NOACTIVATE", 0x08000000L);
            //extendedWindowStyles.Add("WS_EX_NOREDIRECTIONBITMAP", 0x00200000L);
            //extendedWindowStyles.Add("WS_EX_OVERLAPPEDWINDOW"); 
            //extendedWindowStyles.Add("WS_EX_PALETTEWINDOW"); 
            //extendedWindowStyles.Add("WS_EX_RIGHT", 0x00001000L);
            //extendedWindowStyles.Add("WS_EX_RIGHTSCROLLBAR", 0x00001000L);
            //extendedWindowStyles.Add("WS_EX_RTLREADING", 0x00002000L);
            //extendedWindowStyles.Add("WS_EX_STATICEDGE", 0x00020000L);
            //extendedWindowStyles.Add("WS_EX_TOOLWINDOW", 0x00000080L);
            extendedWindowStyles.Add("WS_EX_TOPMOST", 0x00000008L);
            //extendedWindowStyles.Add("WS_EX_TRANSPARENT", 0x00000020L);
            //extendedWindowStyles.Add("WS_EX_WINDOWEDGE", 0x00000100L);

            iterateThroughDict(windowStyles, info, false);
            iterateThroughDict(extendedWindowStyles, info, true);
        }

        public static void iterateThroughDict(Dictionary<string, long> dict, WINDOWINFO info, Boolean extended)
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
                    // Console.WriteLine("Hex Extended: {0:X}", info.dwExStyle);
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
                        Console.WriteLine("Current Style: {0}", entry.Key);
                        Console.WriteLine("Current Value: {0}", entry.Value);
                    }
                }
            }
        }

        public static ObservableCollection<WindowSummary> getWindowSummaryInfo(List<Process> processList)
        {
            ObservableCollection<WindowSummary> windowSummaries = new ObservableCollection<WindowSummary>();
            foreach (Process process in processList)
            {
                int currentProcessId = Process.GetCurrentProcess().Id;
                if (currentProcessId == process.Id)
                {
                    // Application.Current.Windows;l
                    System.Diagnostics.Debug.WriteLine("Skipping Process!!");
                    continue;
                }

                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                    Icon associatedProgramIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                    WindowSummary currWindowSummary = new WindowSummary();
                    currWindowSummary.ProgramIcon = ToImageSource(associatedProgramIcon);
                    currWindowSummary.ProgramName = process.ProcessName;
                    currWindowSummary.ProgramWindowTitle = process.MainWindowTitle;
                    windowSummaries.Add(currWindowSummary);
                }
            }

            return windowSummaries;
        }

        private static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, 
                                      Int32Rect.Empty, 
                                      BitmapSizeOptions.FromEmptyOptions()); 
            return imageSource;
        }
    }

}