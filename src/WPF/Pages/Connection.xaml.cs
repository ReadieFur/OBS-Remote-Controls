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
using OBSWebsocketDotNet;

namespace OBS_Remote_Controls.WPF.Pages
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : Page
    {
        private readonly OBSWebsocket obsWebsocket;

        public Connection(ref OBSWebsocket _obsWebsocket)
        {
            InitializeComponent();
            
            obsWebsocket = _obsWebsocket;

            if (!string.IsNullOrEmpty(Program.savedData.data.clientInfo.address))
            {
                address.Text = Program.savedData.data.clientInfo.address;
                password.Password = Program.savedData.data.clientInfo.password;
                Connect(Program.savedData.data.clientInfo.address, Program.savedData.data.clientInfo.password);
            }
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            Connect(address.Text, password.Password);
        }

        //Add validation checks here
        private bool Connect(string _address, string _password)
        {
            status.Content = "Status: Connecting...";

            //This should only really be saved if the connection was successful, but there are a few reasons I am not doing that.
            Program.savedData.data.clientInfo.address = _address;
            Program.savedData.data.clientInfo.password = _password;

            //This NuGet package dosent seem to have a connect async function so the UI will freeze while the application is connecting. Find a way to fix this, look into 'obsWebsocket.WSConnection.ConnectAsync'?
            try
            {
                obsWebsocket.Connect(_address, _password);
                address.Text = _address;
                password.Password = _password;
                Logger.Info($"Connected to: {_address}");
                status.Content = $"Status: Connected to '{_address}'";
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                status.Content = "Status: Failed";
                return false;
            }

            /*obsWebsocket.WSConnection.ConnectAsync(_address, _password).ContinueWith((r) =>
            {
                if (r.IsFaulted)
                {
                    Logger.Error(r.Exception);
                    status.Content = "Status: Failed";
                }
                else
                {
                    Logger.Info($"Connected to: {ip.Text}");
                    status.Content = "Status: Connected";
                }
            });*/
        }
    }
}
