using System.ComponentModel;
using System.Windows;
using Forms = System.Windows.Forms;

namespace OBS_Remote_Controls.WPF
{
    /// <summary>
    /// Interaction logic for SystemTray.xaml
    /// </summary>
    public partial class SystemTray : Window
    {
        public Forms.NotifyIcon trayIcon;

        public SystemTray()
        {
            InitializeComponent();
            trayIcon = new Forms.NotifyIcon();
            trayIcon.Icon = new System.Drawing.Icon("Resources/Icon.ico");
            trayIcon.Text = "OBS Controls";
        }

        protected override void OnClosing(CancelEventArgs _e)
        {
            trayIcon.Dispose();
            base.OnClosing(_e);
        }

        private void NotifyIcon_MouseClick(object sender, Forms.MouseEventArgs _e)
        {
            //For now there won't be a context menu. Handled by Startup.cs
            /*if (_e.Button == Forms.MouseButtons.Right)
            {
                Top = Forms.Cursor.Position.Y - Height;
                Left = Forms.Cursor.Position.X - Width;
                Show();
                Activate();
            }*/
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Hide();
        }
    }
}
