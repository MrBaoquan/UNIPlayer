using System.Net;
using System.IO;
using UnityEngine;

namespace IOToolkit
{
    public static class IOToolkitUtil
    {
        public static string ConfigPath
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "Plugins/IOToolkit/Config/IODevice.xml");
#elif UNITY_64

                return Path.Combine(Application.dataPath, $"Plugins/x86_64/Config/IODevice.xml");
#elif UNITY_32
                var _platform = "x86";
                return Path.Combine(Application.dataPath, $"Plugins/x86/Config/IODevice.xml");
#endif
            }
        }
    }
}
