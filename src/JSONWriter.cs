using System;
using System.IO;
using Newtonsoft.Json;

namespace OBS_Remote_Controls
{
    public class JSONWriter<JSONStructure> where JSONStructure : new()
    {
        private readonly string dataPath;

        public readonly JSONStructure data = new JSONStructure();

        public JSONWriter(string _dataPath)
        {
            dataPath = _dataPath;

            try
            {
                if (!File.Exists(dataPath))
                {
                    Logger.Trace($"Create file: {dataPath}");
                    CreatePath(dataPath);
                }
                else
                {
                    string json = File.ReadAllText(dataPath);

#if DEBUG
                    Logger.Trace($"Load file: {dataPath}\nContents: {json}");
#else
                    Logger.Trace($"Load file: {dataPath}");
#endif

                    if (!string.IsNullOrEmpty(json))
                    {
                        data = JsonConvert.DeserializeObject<JSONStructure>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Critical(ex);
            }
        }

        public bool Save()
        {
            if (data != null)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(data, Formatting.None);
#if DEBUG
                    Logger.Trace(json);
#endif

                    if (!File.Exists(dataPath)) { CreatePath(dataPath); }
                    File.WriteAllText(dataPath, json);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            
            return false;
        }

        private static bool CreatePath(string _path)
        {
            Logger.Trace($"'{_path}'");
            _path = _path.Replace('/', '\\');
            Logger.Trace($"'{_path}'");
            string[] path = _path.Split('\\');
            string constructedPath = "";

            try
            {
                for (int i = 0; i < path.Length - 1; i++)
                {
                    constructedPath += $"{path[i]}\\";
                    Directory.CreateDirectory(constructedPath);
                }

                constructedPath += path[path.Length - 1];
                if (path[path.Length - 1].Contains("."))
                {
                    File.Create(constructedPath).Close();
                }
                else
                {
                    Directory.CreateDirectory(constructedPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }
    }
}
