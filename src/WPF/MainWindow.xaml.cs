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
using System.Windows.Shapes;
using OBSWebsocketDotNet;

namespace OBS_Remote_Controls.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OBSWebsocket obsWebsocket;

        public MainWindow(ref OBSWebsocket _obsWebsocket)
        {
            InitializeComponent();
            windowBorder.Visibility = Visibility.Visible;
            obsWebsocket = _obsWebsocket;
        }

        private void topBar_MouseDown(object sender, MouseButtonEventArgs _e)
        {
            if (_e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
