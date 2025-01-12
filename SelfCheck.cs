using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using GTA.UI;

namespace MMI_SP
{
    internal static class SelfCheck
    {
        private class Dependency
        {
            public string FileName { get => _fileName; }
            private readonly string _fileName;

            public string FilePath { get => _filePath; }
            private readonly string _filePath;

            public string FullPath { get => $@"{_filePath}\{_fileName}"; }

            public Version Version { get => _version; }
            private readonly Version _version;

            public Dependency(string fileName, string version, string filePath = null)
            {
                if (filePath == null)
                {
                    filePath = AppDomain.CurrentDomain.BaseDirectory;
                }
                
                _fileName = fileName;
                _filePath = filePath;
                _version = new Version(version);
            }
        }

        private static List<Dependency> Dependencies { get => _dependencies; }
        private readonly static List<Dependency> _dependencies = new List<Dependency>
        {
            new Dependency("iFruitAddon2.dll", "2.1.0.0"),
            new Dependency("NativeUI.dll", "1.9.0.0")
        };

        internal static bool Check()
        {
            return CheckDependencies();
        }

        private static bool CheckDependencies()
        {
            bool installed = true;
 
            Logger.Debug("Checking dependencies versions...");
            foreach (Dependency dependency in Dependencies)
            {
                Logger.Debug($"Checking {dependency.FileName} version...");

                if (File.Exists(dependency.FullPath))
                {
                    Version installedVersion = new Version(FileVersionInfo.GetVersionInfo(dependency.FullPath).ProductVersion);
                    
                    Logger.Debug($"{dependency.FileName} version: {installedVersion} (expected {dependency.Version})");

                    if (installedVersion < dependency.Version)
                    {
                        if (Config.ShowFileNotification) Notification.Show(NotificationIcon.Blocked, "MMI-SP", dependency.FileName + " " + installedVersion + " is outdated!", "Download and install the latest version.");
                        Logger.Error(dependency.FileName + " " + installedVersion + " is outdated!");
                        installed = false;
                    }
                }
                else
                {
                    if (Config.ShowFileNotification) Notification.Show(NotificationIcon.Blocked, "MMI-SP", dependency.FileName + " is missing!", "Download and install this file before starting the game.");
                    Logger.Error(dependency.FullPath + " file is missing!");
                    installed = false;
                }
            }
            
            Logger.Debug("Dependencies checked");
            
            return installed;
        }

    }
}
