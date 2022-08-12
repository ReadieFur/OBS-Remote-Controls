namespace OBSRemoteControlsCustom.Configuration
{
    public struct SConfiguration
    {
        public static int VERSION = 1;
        
        public int configurationVersion;
        public string ipAddress;
        public int port;
        public string password;
        //The higher this number, the less CPU used by the global input hook, however as a result some events may be missed.
        public int hookUpdateRateMS;
        public List<SOBSMacro> macros;

        public SConfiguration()
        {
            configurationVersion = VERSION;
            ipAddress = "127.0.0.1";
            port = 4444;
            password = "";
            hookUpdateRateMS = 10;
            macros = new();
        }
    }
}
