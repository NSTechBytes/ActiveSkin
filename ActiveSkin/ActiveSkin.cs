using System;
using System.IO;
using System.Runtime.InteropServices;
using Rainmeter;

namespace PluginSkinStatus
{
    internal class Measure
    {
        private string skinSection;
        private string settingsPath;
        private API api;  

        internal Measure()
        {
        }

        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            this.api = api; 
            skinSection = api.ReadString("SkinSection", "");  

            if (string.IsNullOrEmpty(skinSection))
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: SkinSection parameter is not specified or empty.");
            }

            settingsPath = api.ReplaceVariables("#SETTINGSPATH#");
        }

       
        private int GetActiveStatus()
        {
          
            string iniFilePath = Path.Combine(settingsPath, "Rainmeter.ini");

            
            if (!File.Exists(iniFilePath))
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: Rainmeter.ini file not found at " + iniFilePath);
                return 0;
            }

           
            string[] lines = File.ReadAllLines(iniFilePath);
            bool sectionFound = false;

          
            foreach (string line in lines)
            {
                
                if (line.Trim() == $"[{skinSection}]")
                {
                    sectionFound = true;
                    continue;
                }

               
                if (sectionFound)
                {
                    if (line.Trim().StartsWith("Active="))
                    {
                        string activeValue = line.Trim().Substring("Active=".Length);
                        if (int.TryParse(activeValue, out int result))
                        {
                            return result; 
                        }
                    }
                }
            }

            return 0;
        }

       
        internal double Update()
        {
            return (double)GetActiveStatus();
        }
    }

    public static class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue); 
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }
    }
}
