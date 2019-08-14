using System;
using System.Windows;
using System.ComponentModel;

namespace ApplicationSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
        // NOTE: variables for setting up windows icon
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        // private bool _isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // MainWindow.Closing += MainWindow_Closing;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = ApplicationSwitcher.Properties.Resources.Icon1;
            _notifyIcon.Visible = true;
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        //public void App_Deactivated(object sender, EventArgs e)
        //{
        //    MainWindow_Hide();
        //}

        // TODO: Might not be necessary
        // private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        // {
        //     if (!_isExit)
        //     {
        //         e.Cancel = true;
        //         MainWindow.Hide();
        //     }
        // }

        private void ExitApplication()
        {
            // _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        public void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }

                else
                {
                    MainWindow.Show();
                }
            }
        }

        public void MainWindow_Hide()
        {
            MainWindow.Hide();
        }
    }
}
