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
    public partial class App : Application
    {
        private Program app;

        private void Application_Startup(object sender, StartupEventArgs args)
        {
            app = new Program(args);
        }
    }
}
