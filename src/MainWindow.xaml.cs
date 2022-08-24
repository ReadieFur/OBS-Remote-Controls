using WPFTemplate.Controls;
using WPFTemplate.Styles;

namespace OBSRemoteControls
{
    public partial class MainWindow : WindowChrome
    {
        public MainWindow()
        {
            InitializeComponent();

            StylesManager.onChange += StylesManager_onChange;
            //Trigger the styles to be updated here to override the default values.
            StylesManager_onChange();
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
    }
}
