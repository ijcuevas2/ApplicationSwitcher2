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
        public List<OtherProgramSummary> childProgramSummaries;

        public ProgramSummary(Process process)
        {
            mainWindowProcess = process;
            childProgramSummaries = new List<OtherProgramSummary>();
        }

        public void AddProgramSummary(OtherProgramSummary otherProgramSummary)
        {
            childProgramSummaries.Add(otherProgramSummary);
        }
    }

    public class OtherProgramSummary
    {
        public string title;
        public int windowHandle;
    }

    public class WindowSummaryManager
    {
        // List<List<Process>> programProcessList; 
        public static Dictionary<int, ProgramSummary> programSummaryDict = new Dictionary<int, ProgramSummary>();

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
                    int processId = process.Id;
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

            MyEnumWindows.GetWindowTitles(true);
            Console.WriteLine("MyEnumWindows Result");
            foreach (var item in MyEnumWindows.windowTitles)
            {
                Console.WriteLine("item: {0}", item);
            }

            return mainProcessList;
        }

        //public static ObservableCollection<WindowSummary> getWindowSummaryInfo(List<Process> processList)
        //{
        //    ObservableCollection<WindowSummary> windowSummaries = new ObservableCollection<WindowSummary>();
        //    foreach (Process process in processList)
        //    {
        //        int currentProcessId = Process.GetCurrentProcess().Id;
        //        if (currentProcessId == process.Id)
        //        {
        //            // Application.Current.Windows;l
        //            System.Diagnostics.Debug.WriteLine("Skipping Process!!");
        //            continue;
        //        }

        //        if (!String.IsNullOrEmpty(process.MainWindowTitle))
        //        {
        //            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
        //            Icon associatedProgramIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);
        //            WindowSummary currWindowSummary = new WindowSummary();
        //            currWindowSummary.ProgramIcon = ToImageSource(associatedProgramIcon);
        //            currWindowSummary.ProgramName = process.ProcessName;
        //            currWindowSummary.ProgramWindowTitle = process.MainWindowTitle;
        //            windowSummaries.Add(currWindowSummary);
        //        }
        //    }

        //    return windowSummaries;
        //}

        private static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, 
                                      Int32Rect.Empty, 
                                      BitmapSizeOptions.FromEmptyOptions()); 
            return imageSource;
        }
    }

}