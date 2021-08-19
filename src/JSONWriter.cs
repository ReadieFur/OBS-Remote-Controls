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
                if (!PathExists(dataPath))
                {
                    CreatePath(dataPath);
                }
                else
                {
                    string json = File.ReadAllText(dataPath);

#if DEBUG
                    Logger.Trace(json);
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

                    if (!PathExists(dataPath)) { CreatePath(dataPath); }
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

        //These will both slow down the program a bit especially if the path is long but it will have to make do.
        private static bool PathExists(string _path)
        {
            _path = _path.Replace('/', '\\');
            string[] path = _path.Split('\\');
            string constructedPath = "";

            try
            {
                for (int i = 0; i < path.Length - 1; i++)
                {
                    constructedPath += path[i];
                    if (Directory.Exists(constructedPath)) { return false; }
                }

                constructedPath += path[path.Length - 1];

                if (path[path.Length - 1].Contains(".") && File.Exists(constructedPath))
                {
                    return true;
                }
                else
                {
                    Directory.CreateDirectory(constructedPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        private static bool CreatePath(string _path)
        {
            _path = _path.Replace('/', '\\');
            string[] path = _path.Split('\\');
            string constructedPath = "";

            try
            {
                for (int i = 0; i < path.Length - 1; i++)
                {
                    constructedPath += path[i];
                    Directory.CreateDirectory(constructedPath);
                }

                if (path[path.Length - 1].Contains("."))
                {
                    File.Create(_path).Close();
                }
                else
                {
                    Directory.CreateDirectory(_path);
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
