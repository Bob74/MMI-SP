using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MMI_SP
{
    internal static class SelfCheck
    {
        internal static bool ArePrerequisitesInstalled()
        {
            bool installed = true;
            Version dotNetVersion = new Version("4.8.0.0");
            DateTime SHVDNDate = new DateTime(2017, 12, 19);

            Dictionary<string, Version> prerequisites = new Dictionary<string, Version>
            {
                {"SHVDN-Extender.dll", new Version("1.0.0.1")},
                {"iFruitAddon2.dll", new Version("2.0.1.0")},
                {"NativeUI.dll", new Version("1.7.0.0")},
            };

            foreach (string key in prerequisites.Keys)
            {
                string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\" + key;

                if (File.Exists(fileName))
                {
                    FileInfo info = new FileInfo(fileName);
                    FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(fileName);

                    int versionCheck = new Version(fileInfo.ProductVersion).CompareTo(prerequisites[key]);
                    if (versionCheck < 0)
                    {
                        if (Config.ShowFileNotification) Tools.ShowNotification("CHAR_BLOCKED", "MMI-SP", key + " v" + fileInfo.ProductVersion + " is outdated!", "Download and install the latest version.");
                        Logger.Info("Error: " + key + " v" + fileInfo.ProductVersion + " is outdated!");
                        installed = false;
                    }
                }
                else
                {
                    if (Config.ShowFileNotification) Tools.ShowNotification("CHAR_BLOCKED", "MMI-SP", key + " is missing!", "Download and install this file before starting the game.");
                    Logger.Info("Error: " + key + " is missing!");
                    installed = false;
                }
            }

            string SHVDNFileName = AppDomain.CurrentDomain.BaseDirectory + "\\..\\ScriptHookVDotNet2.dll";
            if (File.Exists(SHVDNFileName))
            {
                FileInfo info = new FileInfo(SHVDNFileName);
                DateTime date = info.LastWriteTime;
                if (date < SHVDNDate)
                {
                    if (Config.ShowSHVDNNotification) Tools.ShowNotification("CHAR_BLOCKED", "MMI-SP", "ScriptHookVDotNet2 is outdated!", "Download and install the latest version.");
                    Logger.Info("Error: ScriptHookVDotNet2 is outdated!");
                    installed = false;
                }
            }

            if (!Tools.IsVisualCVersionHigherOrEqual(Tools.VisualCVersion.Visual_2015))
            {
                if (Config.ShowVisualCNotification) Tools.ShowNotification("CHAR_BLOCKED", "MMI-SP", "Microsoft Visual C++ is missing!", "Download and install version 2015 or 2017 x64.");
                Logger.Info("Error: Microsoft Visual C++ 2015 and 2017 x64 is missing!");
                installed = false;
            }
            if (Tools.GetNETFrameworkVersion().CompareTo(dotNetVersion) < 0)
            {
                if (Config.ShowNETFrameworkNotification) Tools.ShowNotification("CHAR_BLOCKED", "MMI-SP", "Microsoft .NET Framework is outdated!", "Download and install version 4.8 or later.");
                Logger.Info("Error: Microsoft .NET Framework is outdated!");
                installed = false;
            }

            return installed;
        }

    }
}
