using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static OBS_Remote_Controls.Logger;

namespace OBS_Remote_Controls.WPF
{
    /// <summary>
    /// Interaction logic for LoggerWindow.xaml
    /// </summary>
    public partial class LoggerWindow : Window
    {
        private bool applicationShutdown = false;

        public static FontFamily font = new FontFamily("Consolas");

        public static class Colours
        {
            public static Brush white = Colors.White.ToString().GetBrush();
            public static Brush gray = Colors.Gray.ToString().GetBrush();
            public static Brush yellow = Colors.Yellow.ToString().GetBrush();
            public static Brush red = Colors.Red.ToString().GetBrush();
            public static Brush darkMagenta = Colors.DarkMagenta.ToString().GetBrush();
            public static Brush cyan = Colors.Cyan.ToString().GetBrush();
            public static Brush darkGray = Colors.DarkGray.ToString().GetBrush();
        }

        private readonly int frameLevel;

        public LoggerWindow(int _frameLevel = 2)
        {
            InitializeComponent();
            frameLevel = _frameLevel;
            Application.Current.Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            applicationShutdown = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            messages.Blocks.Clear();
            if (!applicationShutdown)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void Debug(object _message) { WriteLog(LogLevel.Debug, _message.ToString(), Colours.gray); }
        public void Debug(Exception _ex) { WriteLog(LogLevel.Debug, _ex.Message, Colours.gray); }
        public void Info(object _message) { WriteLog(LogLevel.Info, _message.ToString(), Colours.white); }
        public void Info(Exception _ex) { WriteLog(LogLevel.Info, _ex.Message, Colours.white); }
        public void Warning(object _message) { WriteLog(LogLevel.Warning, _message.ToString(), Colours.yellow); }
        public void Warning(Exception _ex) { WriteLog(LogLevel.Warning, _ex.Message, Colours.yellow); }
        public void Error(object _message) { WriteLog(LogLevel.Error, _message.ToString(), Colours.red); }
        public void Error(Exception _ex) { WriteLog(LogLevel.Error, _ex.Message, Colours.red); }
        public void Critical(object _message) { WriteLog(LogLevel.Critical, _message.ToString(), Colours.darkMagenta); }
        public void Critical(Exception _ex) { WriteLog(LogLevel.Critical, _ex.Message, Colours.darkMagenta); }
        public void Notice(object _message) { WriteLog(LogLevel.Notice, _message.ToString(), Colours.cyan); }
        public void Notice(Exception _ex) { WriteLog(LogLevel.Notice, _ex.Message, Colours.cyan); }
        public void Trace(object _message) { WriteLog(LogLevel.Trace, _message.ToString(), Colours.darkGray); }
        public void Trace(Exception _ex) { WriteLog(LogLevel.Trace, _ex.Message, Colours.darkGray); }

        private void WriteLog(LogLevel _logLevel, string _message, Brush colour)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.LineHeight = 5;

            Run run = new Run();
            MethodBase stackTraceMethod = new StackTrace().GetFrame(frameLevel).GetMethod();
            run.Text = $"[{_logLevel} @ {DateTime.Now} | {stackTraceMethod.DeclaringType}/{stackTraceMethod.Name}] {_message}";
            run.Foreground = colour;
            run.FontFamily = font;
            run.FontWeight = FontWeight.FromOpenTypeWeight(300); //Light.
            run.FontSize = 12;

            paragraph.Inlines.Add(run);

            //This sometimes get thread access errors and 'Dispatcher.Invoke' dosen't help with that.
            try
            {
                messages.Blocks.Add(paragraph);

                if ((bool)autoscroll.IsChecked)
                {
                    messagesContainer.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                //Logger.Critical(ex);
            }
        }
    }
}
