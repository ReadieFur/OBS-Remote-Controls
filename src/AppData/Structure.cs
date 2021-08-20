using System.Collections.Generic;

namespace OBS_Remote_Controls.AppData
{
    class Structure
    {
        public Objects.VersionInfo versionInfo = new Objects.VersionInfo();
        public Objects.ClientInfo clientInfo = new Objects.ClientInfo();
        public Objects.Preferences preferences = new Objects.Preferences();
        public List<Objects.Hotkey> hotkeys = new List<Objects.Hotkey>();
    }
}
