using System;
using System.Reflection;
using Forms = System.Windows.Forms;
using HotkeyController = OBS_Remote_Controls.Hotkeys;

namespace OBS_Remote_Controls.AppData
{
    class Objects
    {
        public class VersionInfo
        {
            public long lastChecked = DateTime.MinValue.Ticks;
            public readonly string current = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            public string latest = "";
            public string url = "";
        }

        public class ClientInfo
        {
            public string address = "";
            public string password = "";
        }

        public class Preferences
        {
            public bool showNotifications = true;
        }

        public class Hotkey
        {
            public Forms.Keys key = Forms.Keys.None; //This value should be checked, if it is set to none then dont save the hotkey.
            public HotkeyController.KeyModifiers combination = HotkeyController.KeyModifiers.NoRepeat; //Placeholder  value.
            public CustomOBSWebsocket.OBSActions action = CustomOBSWebsocket.OBSActions.SaveReplayBuffer; //Placeholder value.
        }
    }
}
