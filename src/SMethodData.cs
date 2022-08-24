using System.Collections.Generic;

namespace OBSRemoteControls
{
    public struct SMethodData
    {
        public string method { get; init; }
        public Dictionary<string, object> parameters { get; init; }

        public SMethodData()
        {
            method = string.Empty;
            parameters = new();
        }
    }
}
