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

namespace ApplicationSwitcher
{
    
    public class WindowSummary
    {
        // TODO: include window desktop number
        // Ultimately, a program has to be in a window summary
        public AutomationElement Element { get; set; }
        public ImageSource ProgramIcon { get; set; }
        public string ProgramName { get; set; }
        public string ProgramWindowTitle { get; set; }
    }

    public class ProgramSummary
    {
        public Process mainWindowProcess;
        Icon programIcon;
        public List<ChildWindowSummary> childProgramSummaries;

        public ProgramSummary(Process process, Icon icon)
        {
            mainWindowProcess = process;
            programIcon = icon;
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
                }
            }

            // NOTE: Done Adding Processes

            // NOTE: this is somewhat based off of an incorrect assumption
            Console.WriteLine("Done Adding Program Summaries");

            return mainProcessList;
        }

        public static void getRunningChildPrograms()
        {
            Process[] processList = Process.GetProcesses();
            List<Process> mainProcessList = new List<Process>();
            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    //ProgramSummary currProgramSummary = new ProgramSummary(process, icon);
                    //uint processId = (uint)process.Id;
                    //programSummaryDict.Add(processId, currProgramSummary);
                    //MyEnumWindows.GetWindowTitles(true);
                    //Console.WriteLine("MyEnumWindows Result");
                    foreach (ChildWindowSummary childWindowSummary in MyEnumWindows.childWindowSummaries)
                    {
                        ProgramSummary currSummary = null;
                        if (programSummaryDict.TryGetValue(childWindowSummary.lpdwProcessId, out currSummary))
                        {
                            currSummary.AddChildWindowSummary(childWindowSummary);
                        }
                    }
                }

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
                        Boolean match = processWindowInfo(info);
                        if (match)
                        {
                            WindowSummary childProgram = new WindowSummary();
                        }
                    }
                }
            }
        }

        private static Boolean processWindowInfo(WINDOWINFO info)
        {
            Dictionary<string, long> windowStyles = new Dictionary<string, long>();

            windowStyles.Add("WS_MAXIMIZE", 0x01000000L);
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
                    //Console.WriteLine("Hex: {0:X}", info.dwStyle);
                    //if ((info.dwStyle & dict["WS_MINIMIZE"]) == 0)
                    //{
                    //    continue;
                    //}
                }

                else
                {
                    match = (info.dwExStyle & value) != 0;
                    //Console.WriteLine("Hex Extended: {0:X}", info.dwExStyle);
                    //if ((info.dwExStyle & dict["WS_EX_TOPMOST"]) == 0)
                    //{
                    //    continue;
                    //}
                }

                Console.WriteLine("Match: {0}", match);

                if (match)
                {
                    Console.WriteLine("");
                    if (entry.Key == "WS_EX_TOPMOST" || entry.Key == "WS_MINIMIZE")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static ObservableCollection<WindowSummary> getWindowSummaryInfo(List<Process> processList)
        {
            ObservableCollection<WindowSummary> windowSummaries = new ObservableCollection<WindowSummary>();
            MyEnumWindows.GetWindowTitles(true);
            foreach (ChildWindowSummary summary in MyEnumWindows.childWindowSummaries)
            {
                // Call get the running child programs here? or just iterate 
                Process process = Process.GetProcessById((int)summary.lpdwProcessId);
                AutomationElement element = AutomationElement.FromHandle(summary.windowHandle);
                Icon associatedProgramIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);

                WindowSummary currWindowSummary = new WindowSummary();
                currWindowSummary.Element = element;
                currWindowSummary.ProgramIcon = ToImageSource(associatedProgramIcon);
                currWindowSummary.ProgramName = process.ProcessName;
                currWindowSummary.ProgramWindowTitle = summary.title;
                windowSummaries.Add(currWindowSummary);
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