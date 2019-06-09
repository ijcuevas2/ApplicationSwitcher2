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

    public class WindowSummaryManager
    {
        public static List<Process> GetRunningPrograms()
        {
            Process[] processList = Process.GetProcesses();
            List<Process> mainProcessList = new List<Process>();

            foreach (Process process in processList)
            {
                Console.WriteLine("Process: {0} Id: {1}", process.ProcessName, process.Id);
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(" ");
            }

            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    mainProcessList.Add(process);
                }
            }


            return mainProcessList;
        }

        public static ObservableCollection<WindowSummary> getWindowSummaryInfo(List<Process> processList)
        {
            ObservableCollection<WindowSummary> windowSummaries = new ObservableCollection<WindowSummary>();
            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                    // TODO: check if these are null
                    // Also check if I could just use regular icons
                    Icon associatedProgramIcon = System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName);

                    
                    
                    // ImageCodecInfo programImageCodecInfo = GetEncoderInfo("image/jpeg");
                    // Encoder qualityEncoder = Encoder.Quality;
                    // EncoderParameter programEncoderParameter = new EncoderParameter(qualityEncoder, 25L);
                    // EncoderParameters encoderParameters = new EncoderParameters(1);
                    // encoderParameters.Param[0] = programEncoderParameter;

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

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageDecoders();
            for (int i = 0; i < encoders.Length; i++)
            {
                if (encoders[i].MimeType == mimeType)
                {
                    return encoders[i];
                }
            }
            return null;
        }
    }

}