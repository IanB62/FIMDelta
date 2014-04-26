using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace FimDelta
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();

            if (e.Args.Length > 2)
            {
                mainWindow.SourceFileName = e.Args[0];
                mainWindow.TargetFileName = e.Args[1];
                mainWindow.ChangesFileName = e.Args[2];
            }

            if (e.Args.Length > 3)
            {
                mainWindow.FilteredChangesFileName = e.Args[3];
            }

            mainWindow.Show();
        }
    }
}
