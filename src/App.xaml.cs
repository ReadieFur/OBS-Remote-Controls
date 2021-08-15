using System.Windows;

namespace OBS_Remote_Controls
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Startup app;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            app = new Startup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            app.Dispose();
            base.OnExit(e);
        }
    }
}
