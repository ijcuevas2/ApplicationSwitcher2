using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;
using System.Diagnostics;

namespace ApplicationSwitcher
{
    
    public class WindowSummary
    {
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

        // Allows for automatic initialization of "cbSize" with "new WINDOWINFO(null / true / false)"
        public WINDOWINFO(Boolean? filler): this() 
        {
            cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
        }
    }

    public class WindowSummaryManager
    {
        public static Dictionary<uint, ProgramSummary> programSummaryDict = new Dictionary<uint, ProgramSummary>();
        public static List<Process> GetRunningPrograms()
        {
            Process[] processList = Process.GetProcesses();
            List<Process> mainProcessList = new List<Process>();

            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    mainProcessList.Add(process);
                }
            }

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
                    foreach (ChildWindowSummary summary in entry.Value.childProgramSummaries)
                    {
                        //System.Diagnostics.Debug.WriteLine("Summary Title: {0}", summary.title);
                        //System.Diagnostics.Debug.WriteLine("Summary Process: {0}", summary.lpdwProcessId);

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

            Dictionary<string, long> extendedWindowStyles = new Dictionary<string, long>();
            extendedWindowStyles.Add("WS_EX_TOPMOST", 0x00000008L);

            Boolean styleMatch = iterateThroughDict(windowStyles, info, false);
            Boolean exStyleMatch = iterateThroughDict(extendedWindowStyles, info, true);
            Boolean viableChildWindow = styleMatch && exStyleMatch;
            return viableChildWindow;
        }

        public static Boolean iterateThroughDict(Dictionary<string, long> dict, WINDOWINFO info, Boolean extended)
        {
            foreach(KeyValuePair<string, long> entry in dict)
            {
                long value = entry.Value;

                Boolean match = false;
                if (!extended)
                {
                    match = (info.dwStyle & value) != 0;
                }

                else
                {
                    match = (info.dwExStyle & value) != 0;
                }


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

        public static ObservableCollection<WindowSummary> getWindowSummaryInfo(List<Process> processList)
        {
            ObservableCollection<WindowSummary> windowSummaries = new ObservableCollection<WindowSummary>();
            MyEnumWindows.GetWindowTitles(true);
            foreach (ChildWindowSummary summary in MyEnumWindows.childWindowSummaries)
            {
                Process process = Process.GetProcessById((int)summary.lpdwProcessId);
                AutomationElement element = AutomationElement.FromHandle(summary.windowHandle);
                Icon associatedProgramIcon;
                try
                {
                    associatedProgramIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Win32Exception ex: {0}", ex);
                    associatedProgramIcon = new Icon(SystemIcons.Application, 20, 20);
                }

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