using System;
using System.Reflection;

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
    }
}
