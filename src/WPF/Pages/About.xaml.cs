using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBS_Remote_Controls.WPF.Pages
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();

            version.Content = Program.savedData.data.versionInfo.current;
            Program.CheckForUpdates().ContinueWith((t) =>
            {
                if (!t.IsFaulted && !t.IsCanceled)
                {
                    string latestVersion = t.Result;
                    if (!string.IsNullOrEmpty(latestVersion))
                    {
                        version.Content = $"{Program.savedData.data.versionInfo.current} (version {latestVersion} avaliable)";
                    }
                }
            });

            logs.Content = !Logger.IsWindowVisible() ? "Show log window" : "Hide log window";
            notifications.IsChecked = Program.savedData.data.preferences.showNotifications;
        }

        private void logs_Click(object sender, RoutedEventArgs e)
        {
            if (Logger.IsWindowVisible())
            {
                logs.Content = "Show log window";
                Logger.HideWindow();
            }
            else
            {
                logs.Content = "Hide log window";
                Logger.ShowWindow();
            }
        }

        private void notifications_Toggled(object sender, RoutedEventArgs e)
        {
            Program.savedData.data.preferences.showNotifications = notifications.IsChecked??true;
        }
    }
}
