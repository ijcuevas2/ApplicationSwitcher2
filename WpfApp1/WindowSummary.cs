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

    // public class OtherProgramSummary
    // {
    //     public string title;
    //     public int windowHandle;
    // }

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
            }

            return mainProcessList;
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