using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace OBS_Remote_Controls.WPF
{
    public static class Styles
    {
        public static event Action<bool> stylesUpdated;
        public static bool darkTheme = true;
        public static string foregroundColour = darkTheme ? "#FFFFFF" : "#000000";
        public static string backgroundColour = darkTheme ? "#0D1117" : "#FFFFFF";
        public static string backgroundAltColour = darkTheme ? "#161B22" : "#E1E1E1";
        public static string accentColour = "#6400FF";
        public static string accentColourAlt = "#FF7800";

        private static bool checkTheme = false;
        private static Task themeCheckerTask;

        public static Brush GetBrush(this string _colour)
        {
            var bc = new BrushConverter();
            return (Brush)bc.ConvertFrom(_colour);
        }

        public static ResourceDictionary GetXAMLResources()
        {
            return Application.Current.Resources;
        }

        public static bool GetStyles()
        {
            try
            {
                try
                {
                    accentColour = SystemParameters.WindowGlassBrush.ToString();
                    GetXAMLResources()["XAMLAccentColour"] = accentColour.GetBrush();
                    //GetXAMLResources()["XAMLAccentAltColour"] = accentColourAlt.GetBrush();

                    darkTheme = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "0";
                    foregroundColour = darkTheme ? "#FFFFFF" : "#000000";
                    backgroundColour = darkTheme ? "#0D1117" : "#FFFFFF";
                    backgroundAltColour = darkTheme ? "#161B22" : "#E1E1E1";
                    GetXAMLResources()["XAMLForegroundColour"] = foregroundColour.GetBrush();
                    GetXAMLResources()["XAMLBackgroundColour"] = backgroundColour.GetBrush();
                    GetXAMLResources()["XAMLBackgroundAltColour"] = backgroundAltColour.GetBrush();

                    return true;
                }
                catch (Exception ex) { Logger.Error(ex); }
            }
            catch (Exception ex) { Logger.Error(ex); }

            return false;
        }

        public static bool HaveStylesChanged()
        {
            try
            {
                return SystemParameters.WindowGlassBrush.ToString() != accentColour ||
                    (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("AppsUseLightTheme").ToString() == "0") != darkTheme;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static void EnableThemeChecker()
        {
            checkTheme = true;
            themeCheckerTask = Task.Run(ThemeChecker);
        }

        public static void DisableThemeChecker()
        {
            checkTheme = false;
            themeCheckerTask.Dispose();
        }

        private static void ThemeChecker()
        {
            while (checkTheme)
            {
                if (HaveStylesChanged())
                {
                    if (GetStyles() && stylesUpdated != null) { stylesUpdated(true); }
                }
                System.Threading.Thread.Sleep(10000);
            }
        }
    }
}
