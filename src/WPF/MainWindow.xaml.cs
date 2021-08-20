using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OBSWebsocketDotNet;

namespace OBS_Remote_Controls.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OBSWebsocket obsWebsocket;

        private readonly IReadOnlyDictionary<ListViewItem, Page> pages;

        public MainWindow(ref OBSWebsocket _obsWebsocket)
        {
            InitializeComponent();

            obsWebsocket = _obsWebsocket;
            pages = new Dictionary<ListViewItem, Page>
            {
                { new ListViewItem { Tag = "Connection" }, new Pages.Connection(ref obsWebsocket) },
                { new ListViewItem { Tag = "About" }, new Pages.About() }
            };
        }

        private void topBar_MouseDown(object sender, MouseButtonEventArgs _e)
        {
            if (_e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs ev)
        {
            windowBorder.Visibility = Visibility.Visible;

            foreach (KeyValuePair<ListViewItem, Page> kv in pages)
            {
                kv.Key.Height = 50;
                kv.Key.FontSize = 14;
                kv.Key.FontWeight = FontWeight.FromOpenTypeWeight(700); //Bold
                kv.Key.Content = kv.Key.Tag;
                kv.Key.Foreground = Styles.foregroundColour.GetBrush();
                kv.Key.MouseLeftButtonUp += (s, e) => { ChangePage(kv.Key); };
                tabs.Items.Add(kv.Key);
            }

            ChangePage(pages.First(kv => (string)kv.Key.Tag == "Connection").Key);
        }

        private void ChangePage(ListViewItem tab)
        {
            foreach (KeyValuePair<ListViewItem, Page> kv in pages)
            {
                if (kv.Key.Tag == tab.Tag)
                {
                    kv.Key.Background = Styles.accentColour.GetBrush();
                    Frame.Content = kv.Value;
                }
                else
                {
                    kv.Key.Background = Styles.backgroundColour.GetBrush();
                }
            }
        }
    }
}
