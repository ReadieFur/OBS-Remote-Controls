using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using OBS_Remote_Controls.WPF;
using OBSWebsocketDotNet;

namespace OBS_Remote_Controls
{
    //Called by App.xaml.cs, that is the only thing that happens in that file.
    class Program
    {
        public static JSONWriter<AppData.Structure> savedData = new JSONWriter<AppData.Structure>("./Preferences.json");

        private OBSWebsocket obsWebsocket;
        private MainWindow mainWindow;
        private SystemTray systemTray;

        public Program(StartupEventArgs args)
        {
#if DEBUG
            Logger.logLevel = Logger.LogLevel.Trace;
#else
            Logger.logLevel = Logger.LogLevel.Info;
#endif

            Styles.EnableThemeChecker();

            Logger.ShowWindow();

            Task.Run(async () =>
            {
                string latestVersion = await CheckForUpdates();
                Logger.Info(latestVersion ?? "null");
            });

            obsWebsocket = new OBSWebsocket();

            systemTray = new SystemTray();
            systemTray.trayIcon.MouseClick += (s, e) =>
            {
                systemTray.trayIcon.Visible = false;
                mainWindow.Show();
            };

            mainWindow = new MainWindow(ref obsWebsocket);
            mainWindow.Show();
            mainWindow.closeButton.Click += (s, e) => { Exit(); };
            mainWindow.minimiseButton.Click += (s, e) =>
            {
                systemTray.trayIcon.Visible = true;
                mainWindow.Hide();
            };
        }

        internal static async Task<string> CheckForUpdates()
        {
            if (new DateTime(savedData.data.versionInfo.lastChecked).AddHours(1) < DateTime.UtcNow)
            {
                Octokit.GitHubClient gitClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("kOFReadie-OBS-Remote-Controls"));

                IReadOnlyList<Octokit.RepositoryTag> versions = await gitClient.Repository.GetAllTags(266821806); //Change to releases to get the release URL.

                if (versions.Count > 0)
                {
                    Logger.Debug(new Version(versions[0].Name).ToString());
                    Logger.Debug(new Version(savedData.data.versionInfo.current).ToString());

                    if (new Version(versions[0].Name) > new Version(savedData.data.versionInfo.current))
                    {
                        savedData.data.versionInfo = new AppData.Objects.VersionInfo
                        {
                            lastChecked = DateTime.UtcNow.Ticks,
                            latest = versions[0].Name
                        };

                        return versions[0].Name;
                    }
                }
            }
            else if (new Version(savedData.data.versionInfo.latest) > new Version(savedData.data.versionInfo.current))
            {
                return savedData.data.versionInfo.latest;
            }

            return null;
        }

        private void Exit()
        {
            systemTray.Close();
            mainWindow.Close();
            savedData.Save();
            Application.Current.Shutdown();
        }
    }
}
