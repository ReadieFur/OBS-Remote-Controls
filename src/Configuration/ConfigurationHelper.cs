using CSharpTools.ConsoleExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OBSRemoteControlsCustom.Configuration
{
    internal static class ConfigurationHelper
    {
        public static readonly JsonSerializerSettings JSON_SERIALIZER_SETTINGS = new JsonSerializerSettings();
        public static readonly string CONFIGURATION_FILE_PATH = Environment.CurrentDirectory + "\\configuration.json";

        static ConfigurationHelper()
        {
            JSON_SERIALIZER_SETTINGS.Converters.Add(new StringEnumConverter());
            JSON_SERIALIZER_SETTINGS.Formatting = Formatting.Indented;
        }

        public static bool LoadConfiguration(out SConfiguration configuration)
        {
            configuration = new SConfiguration();

            //if (!File.Exists(configFilePath)) throw new Exception("The configuration file does not exist.");
            //if (!File.Exists(configFilePath)) return false;
            if (!File.Exists(CONFIGURATION_FILE_PATH))
            {
                if (WriteDefaultConfiguration())
                    Logger.Info("Written default configuration file, please edit this file and then re-launch the program.").Wait();
                return false;
            }

            try
            {
                string configFileContent = File.ReadAllText(CONFIGURATION_FILE_PATH);
                configuration = JsonConvert.DeserializeObject<SConfiguration>(configFileContent, JSON_SERIALIZER_SETTINGS);
            }
            catch (Exception ex)
            {
                Logger.Trace(ex).Wait();
                return false;
            }

            if (configuration.configurationVersion != SConfiguration.VERSION)
                Logger.Warning("The configuration file is outdated, please update it and then re-launch the program.").Wait();

            return true;
        }

        //Not encrypting the password. No real reason too as this is only for local testing and no sensitive data is shared anyway.
        public static bool SaveConfiguration(SConfiguration configuration)
        {
            try
            {
                string configFileContent = JsonConvert.SerializeObject(configuration, JSON_SERIALIZER_SETTINGS);
                File.WriteAllText(CONFIGURATION_FILE_PATH, configFileContent);
            }
            catch (Exception ex)
            {
                Logger.Trace(ex.Message).Wait();
                return false;
            }
            return true;
        }

        public static bool WriteDefaultConfiguration() => SaveConfiguration(new SConfiguration());
    }
}
