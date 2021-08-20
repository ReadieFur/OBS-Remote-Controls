using System;
using System.Windows;
using System.Windows.Controls;
using OBSWebsocketDotNet;
using System.Text.RegularExpressions;

namespace OBS_Remote_Controls.WPF.Pages
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : Page
    {
        private Regex addressRegex = new Regex(@"ws:\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):\d{4}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        private bool Connect(string _address, string _password)
        {
            status.Content = "Status: Connecting...";

            Match match = addressRegex.Match(_address);
            if (match == null || match.Length != _address.Length)
            {
                status.Content = "Status: Failed";
                return false;
            }

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

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                password.Password = Program.savedData.data.clientInfo.password;
            }
        }

        private void address_TextChanged(object sender, TextChangedEventArgs e)
        {
            string _address = address.Text;
            if (!string.IsNullOrEmpty(_address))
            {
                Match match = addressRegex.Match(_address);
                if (match == null || match.Length != _address.Length)
                {
                    //I can't seem to get this to work. This code is reached but I can't seem to get the colour to change.
                    address.BorderBrush = "#FF0000".GetBrush();
                }
                else
                {
                    address.BorderBrush = Styles.foregroundColour.GetBrush();
                }
            }
            else
            {
                address.BorderBrush = Styles.foregroundColour.GetBrush();
            }
        }
    }
}
