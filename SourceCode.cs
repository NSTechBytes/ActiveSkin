using System;
using System.IO;
using System.Runtime.InteropServices;
using Rainmeter;

namespace PluginSkinStatus
{
    internal class Measure
    {
        private string skinSection;
        private API api;  // Store an instance of API

        internal Measure()
        {
        }

        // Set up the measure with parameters from the Rainmeter skin
        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            this.api = api;  // Assign the API instance
            skinSection = api.ReadString("SkinSection", "");  // Read the section name from Rainmeter skin

            if (string.IsNullOrEmpty(skinSection))
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: SkinSection parameter is not specified or empty.");
            }
        }

        // Function to read the Rainmeter.ini file and get the Active status for the specified section
        private int GetActiveStatus()
        {
            // Path to Rainmeter.ini file (default path in AppData folder)
            string iniFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rainmeter", "Rainmeter.ini");

            // Ensure the file exists
            if (!File.Exists(iniFilePath))
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: Rainmeter.ini file not found.");
                return 0;
            }

            // Read all lines from the ini file
            string[] lines = File.ReadAllLines(iniFilePath);
            bool sectionFound = false;

            // Parse the file line by line
            foreach (string line in lines)
            {
                // Check if we're in the target section
                if (line.Trim() == $"[{skinSection}]")
                {
                    sectionFound = true;
                    continue;
                }

                // Once in the section, find Active setting
                if (sectionFound)
                {
                    if (line.Trim().StartsWith("Active="))
                    {
                        string activeValue = line.Trim().Substring("Active=".Length);
                        if (int.TryParse(activeValue, out int result))
                        {
                            return result;  // Returns 1 if active, 0 if not.
                        }
                    }
                }
            }

            // If the section or Active setting wasn't found, log an error.
            api.Log(API.LogType.Error, $"PluginSkinStatus.dll: Section [{skinSection}] or Active setting not found.");
            return 0;
        }

        // Called by Rainmeter to update the measure
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
            measure.Reload(new Rainmeter.API(rm), ref maxValue);  // Pass the API instance here
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }
    }
}
