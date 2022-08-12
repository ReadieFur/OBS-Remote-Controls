using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using OBS_Remote_Controls.WPF;
using Microsoft.Toolkit.Uwp.Notifications;

namespace OBS_Remote_Controls
{
    //Called by App.xaml.cs, that is the only thing that happens in that file.
    class Program
    {
        public static JSONWriter<AppData.Structure> savedData = new JSONWriter<AppData.Structure>("./Preferences.json");

        private CustomOBSWebsocket obsWebsocket;
        private MainWindow mainWindow;
        private SystemTray systemTray;

        public Program(StartupEventArgs args)
        {
#if DEBUG
            Logger.logLevel = Logger.LogLevel.Debug;
            Logger.ShowWindow();
#else
            Logger.logLevel = Logger.LogLevel.Error;
#endif

            Styles.EnableThemeChecker();

            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;

            CheckForUpdates().ContinueWith((t) =>
            {
                if (!t.IsFaulted && !t.IsCanceled)
                {
                    string latestVersion = t.Result;
                    if (!string.IsNullOrEmpty(latestVersion) && savedData.data.preferences.showNotifications)
                    {
                        //https://docs.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
                        new ToastContentBuilder()
                            .AddArgument("update", latestVersion)
                            .AddText("An update is avaliable.")
                            .AddText($"Version {latestVersion} is avaliable. You are on version {savedData.data.versionInfo.current}.")
                            .Show();
                    }
                }
            });

            obsWebsocket = new CustomOBSWebsocket();
            obsWebsocket.Connected += (s, e) =>
            {
                if (savedData.data.preferences.showNotifications)
                {
                    new ToastContentBuilder()
                    .AddText("OBS Connected.")
                    .AddText($"Connected to OBS Websocket at: {savedData.data.clientInfo.address}")
                    .Show();
                }

#if DEBUG
               obsWebsocket.WSConnection.OnOpen += (_s, _e) => { Logger.Trace($"OnOpen"); };
               obsWebsocket.WSConnection.OnError += (_s, _e) => { Logger.Trace($"OnError"); };
               obsWebsocket.WSConnection.OnClose += (_s, _e) => { Logger.Trace($"OnClose"); };
               obsWebsocket.WSConnection.OnMessage += (_s, _e) => { Logger.Trace($"OnMessage: {_e.Data}"); };
#endif
            };
            obsWebsocket.Disconnected += (s, e) =>
            {
                if (savedData.data.preferences.showNotifications)
                {
                    new ToastContentBuilder()
                    .AddText("OBS Disconnected.")
                    .AddText($"Connected from OBS Websocket.")
                    .Show();
                }
            };

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
                if (savedData.data.preferences.showNotifications)
                {
                    new ToastContentBuilder()
                    .AddText("Minimised to tray.")
                    .AddText($"OBS Remote Controls has been minimised to the system tray.")
                    .Show();
                }
            };
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            ToastArguments args = ToastArguments.Parse(e.Argument);
            if (args.Contains("update") && args.Get("update") == savedData.data.versionInfo.latest)
            {
                System.Diagnostics.Process.Start(savedData.data.versionInfo.url);
            }
        }

        public static async Task<string> CheckForUpdates()
        {
            if (new DateTime(savedData.data.versionInfo.lastChecked).AddHours(1) < DateTime.UtcNow)
            {
                Octokit.GitHubClient gitClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("kOFReadie-OBS-Remote-Controls"));
                IReadOnlyList<Octokit.Release> releases;
                try
                {
                    releases = await gitClient.Repository.Release.GetAll(396356990);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    throw ex;
                }

                if (releases.Count > 0)
                {
                    if (new Version(releases[0].TagName) > new Version(savedData.data.versionInfo.current))
                    {
                        savedData.data.versionInfo = new AppData.Objects.VersionInfo
                        {
                            lastChecked = DateTime.UtcNow.Ticks,
                            latest = releases[0].TagName,
                            url = releases[0].HtmlUrl
                        };

                        return releases[0].TagName;
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
            ToastNotificationManagerCompat.Uninstall();
            systemTray.Close();
            mainWindow.Close();
            savedData.Save();
            Application.Current.Shutdown();
        }
    }
}
