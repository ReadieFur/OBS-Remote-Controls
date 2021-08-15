using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBS_Remote_Controls.WPF;
using OBSWebsocketDotNet;

namespace OBS_Remote_Controls
{
    class Startup : IDisposable
    {
        private OBSWebsocket obsWebsocket;
        private MainWindow mainWindow;
        private SystemTray systemTray;

        public Startup()
        {
#if DEBUG
            Logger.logLevel = Logger.LogLevel.Trace;
#else
            Logger.logLevel = Logger.LogLevel.Info;
#endif

            Styles.EnableThemeChecker();

            obsWebsocket = new OBSWebsocket();

            systemTray = new SystemTray();
            systemTray.trayIcon.MouseClick += (s, e) =>
            {
                systemTray.trayIcon.Visible = false;
                mainWindow.Show();
            };

            mainWindow = new MainWindow(ref obsWebsocket);
            mainWindow.Show();
            mainWindow.closeButton.Click += (s, e) => { Dispose(); };
            mainWindow.minimiseButton.Click += (s, e) =>
            {
                systemTray.trayIcon.Visible = true;
                mainWindow.Hide();
            };
        }

        public void Dispose()
        {
            systemTray.Close();
            mainWindow.Close();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
