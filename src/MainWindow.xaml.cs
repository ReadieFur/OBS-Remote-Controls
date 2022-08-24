using System.ComponentModel;
using System.Reflection;
using System.Windows;
using WPFTemplate.Controls;
using WPFTemplate.Styles;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace OBSRemoteControls
{
    public partial class MainWindow : WindowChrome
    {
        public bool IsClosing { get; protected set; } = false;

        internal TaskbarIcon taskbarIcon = new();

        public MainWindow()
        {
            InitializeComponent();
            
            //Set the window icon.
            Icon = Imaging.CreateBitmapSourceFromHIcon(
                System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)!.Handle,
                Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            //Setup the tray icon.
            taskbarIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            taskbarIcon.ToolTipText = "OBS Remote Controls";
            taskbarIcon.TrayLeftMouseDown += TaskbarIcon_TrayLeftMouseDown;

            //Subscribe to window events.
            StateChanged += MainWindow_StateChanged;
            IsVisibleChanged += MainWindow_IsVisibleChanged;

            //Subscribe to theme update events.
            StylesManager.onChange += StylesManager_onChange;
            //Trigger the styles to be updated here to override the default values.
            StylesManager_onChange();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            IsClosing = true;
            if (taskbarIcon != null && !taskbarIcon.IsDisposed)
            {
                taskbarIcon.Visibility = Visibility.Collapsed;
                taskbarIcon.Dispose();
            }
            base.OnClosing(e);
        }

        private void StylesManager_onChange()
        {
            Dispatcher.Invoke(() =>
            {
                Foreground = StylesManager.foreground;
                Background = StylesManager.background;
                BackgroundAlt = StylesManager.backgroundAlt;
                Accent = StylesManager.accent;
            });
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsClosing) return;
            taskbarIcon.Visibility = IsVisible ? Visibility.Collapsed : Visibility.Visible;
            StylesManager.useSystemTheme = IsVisible; //This enables and disables the system theme update check timer.
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            //The tray icon shouldn't be visible here so hide if it is.
            //Otherwise the state is correct and we should show the window.
            if (IsVisible) taskbarIcon.Visibility = Visibility.Hidden;
            else
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        private void MainWindow_StateChanged(object? sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized) Hide();
        }
    }
}
