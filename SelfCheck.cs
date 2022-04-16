using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

using MMI_SP.Common;

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

        private enum VisualCppVersion
        {
            Visual_2017,
            Visual_2015,
            Visual_2013,
            Visual_2012,
            Visual_2010,
            Visual_2008,
            Visual_2005,
        }

        private static readonly Dictionary<VisualCppVersion, Version> visualCVersion = new Dictionary<VisualCppVersion, Version>
        {
            { VisualCppVersion.Visual_2017, new Version("14.1.0.0")},
            { VisualCppVersion.Visual_2015, new Version("14.0.0.0")},
            { VisualCppVersion.Visual_2013, new Version("12.0.0.0")},
            { VisualCppVersion.Visual_2012, new Version("11.0.0.0")},
            { VisualCppVersion.Visual_2010, new Version("10.0.0.0")},
            { VisualCppVersion.Visual_2008, new Version("9.0.0.0")},
            { VisualCppVersion.Visual_2005, new Version("8.0.0.0")}
        };

        private static Version RequiredVisualCppVersion { get => visualCVersion[_requiredVisualCppVersion]; }
        private readonly static VisualCppVersion _requiredVisualCppVersion = VisualCppVersion.Visual_2015;

        private static Version RequiredDotNetVersion { get => _requiredDotNetVersion; }
        private readonly static Version _requiredDotNetVersion = new Version("4.8.0.0");
        
        private static List<Dependency> Dependencies { get => _dependencies; }
        private readonly static List<Dependency> _dependencies = new List<Dependency>
        {
            new Dependency("ScriptHookVDotNet2.dll", "2.10.13.0", AppDomain.CurrentDomain.BaseDirectory + @"\.."),
            new Dependency("SHVDN-Extender.dll", "1.0.0.1"),
            new Dependency("iFruitAddon2.dll", "2.1.0.0"),
            new Dependency("NativeUI.dll", "1.9.0.0")
        };

        internal static bool Check()
        {
            return CheckDependencies() && CheckVisualCppVersion() && CheckDotNetVersion();
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
                        if (Config.ShowFileNotification) Utils.ShowNotification("CHAR_BLOCKED", "MMI-SP", dependency.FileName + " " + installedVersion + " is outdated!", "Download and install the latest version.");
                        Logger.Error(dependency.FileName + " " + installedVersion + " is outdated!");
                        installed = false;
                    }
                }
                else
                {
                    if (Config.ShowFileNotification) Utils.ShowNotification("CHAR_BLOCKED", "MMI-SP", dependency.FileName + " is missing!", "Download and install this file before starting the game.");
                    Logger.Error(dependency.FullPath + " file is missing!");
                    installed = false;
                }
            }
            
            Logger.Debug("Dependencies checked");
            
            return installed;
        }

        private static bool CheckVisualCppVersion()
        {
            bool installed = true;

            Logger.Debug("Checking Visual C++ version...");
            if (GetVisualCppVersion() < RequiredVisualCppVersion)
            {
                if (Config.ShowVisualCNotification) Utils.ShowNotification("CHAR_BLOCKED", "MMI-SP", "Microsoft Visual C++ is missing!", "Download and install version 2015 or 2017 x64.");
                Logger.Error("Microsoft Visual C++ 2015 and 2017 x64 is missing!");
                installed = false;
            }
            Logger.Debug("Visual C++ version checked");

            return installed;
        }
        
        private static bool CheckDotNetVersion()
        {
            bool installed = true;

            Logger.Debug("Checking .NET version...");
            if (GetNETFrameworkVersion() < RequiredDotNetVersion)
            {
                if (Config.ShowNETFrameworkNotification) Utils.ShowNotification("CHAR_BLOCKED", "MMI-SP", "Microsoft .NET Framework is outdated!", "Download and install version 4.8 or later.");
                Logger.Error("Microsoft .NET Framework is outdated!");
                installed = false;
            }
            Logger.Debug(".NET version checked");

            return installed;
        }

        private static Version GetVisualCppVersion()
        {
            Version highestVersion = new Version(0, 0, 0, 0);

            string[] filters = { "msvcp*.dll", "msvcr*.dll" };
            List<string> files = new List<string>();

            // Gathering Visual C++ system files
            Logger.Debug("Gathering Visual C++ system files...");
            foreach (string filter in filters)
            {
                files.AddRange(Directory.GetFiles(Environment.SystemDirectory, filter));
            }
            Logger.Debug($"Visual C++ files found {string.Join(", ", files.ToArray())}");

            // Retrieving highest version
            Logger.Debug("Retrieving highest Visual C++ version...");
            foreach (string file in files)
            {
                Version currentFileVersion = new Version(FileVersionInfo.GetVersionInfo(file).ProductVersion);
                if (currentFileVersion > highestVersion) highestVersion = currentFileVersion;
            }
            Logger.Debug($"Highest Visual C++ version found: {highestVersion}");

            return highestVersion;
        }

        private static Version GetNETFrameworkVersion()
        {
            Version highestVersion = new Version(0, 0, 0, 0);

            try
            {
                // Checking registry key to find .Net Framework v4 version
                foreach (string regKey in new string[]{ @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client\", @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" })
                {
                    Logger.Debug("Checking registry key: " + regKey);
                    using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(regKey))
                    {
                        Logger.Debug("Checking registry value 'Version'");
                        if (ndpKey?.GetValue("Version") != null)
                        {
                            return new Version((string)ndpKey.GetValue("Version"));
                        }

                        Logger.Debug("Checking registry value 'Release'");
                        if (ndpKey?.GetValue("Release") != null)
                        {
                            int releaseKey = (int)ndpKey.GetValue("Release");

                            if (releaseKey >= 528449)
                            {
                                return new Version("4.8.0.0");
                            }
                            else if (releaseKey >= 461308)
                            {
                                return new Version("4.7.1.0");
                            }
                            else if (releaseKey >= 460798)
                            {
                                return new Version("4.7.0.0");
                            }
                            else
                            {
                                return new Version("4.0.0.0");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return highestVersion;
        }

    }
}
