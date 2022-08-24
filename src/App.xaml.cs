using System;
using System.Windows;
using WPFTemplate.Extensions;

namespace OBSRemoteControls
{
    public partial class App : Application
    {
        private MainWindow mainWindow;

        //Entrypoint of the program.
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new();
            await mainWindow.ShowAsync();
            Environment.Exit(0);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
