using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MMI_SP
{
    using T = Translator;

    internal static class Updater
    {
        private class MMIVersion
        {
            private readonly Version _version;
            public Version Version { get => _version; }

            private readonly string _changelog;
            public string Changelog { get => _changelog; }

            public MMIVersion(string version, string changelog)
            {
                _version = new Version(version);
                _changelog = changelog;
            }
        }
        
        private const string _urlVersionFile = "https://raw.githubusercontent.com/Bob74/MMI-SP/master/version";
        private const string _urlVersionChangelogFile = "https://raw.githubusercontent.com/Bob74/MMI-SP/master/versionlog";
        private readonly static Version _currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public static void CheckForUpdate()
        {
            // Non thread blocking check for update
            Logger.Debug("Starting update check Task...");
            Task.Run(() =>
            {
                Logger.Debug("Fetching version files...");
                MMIVersion latestVersion = FetchLatestVersionInfo();

                if (latestVersion != null)
                {
                    Logger.Debug("Version files fetched");
                    if (latestVersion.Version > _currentVersion)
                    {
                        NotifyNewUpdate(latestVersion);
                    }
                    else
                    {
                        Logger.Debug("No update available");
                    }
                }
                else
                {
                    Logger.Debug("Error: Failed to fetch version files");
                }
            });
            Logger.Debug("Update checking Task is started");
        }

        /// <summary>
        /// Downloads the latest version info from the github repo
        /// </summary>
        /// <returns>MMIVersion object containing version number and changelog</returns>
        private static MMIVersion FetchLatestVersionInfo()
        {
            string version = "";
            string versionLog = "";
            
            try
            {
                using (WebClient wc = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    // Remove insecure protocols (SSL3, TLS 1.0, TLS 1.1)
                    ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                    ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
                    ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
                    // Add TLS 1.2
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    
                    wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                    wc.Headers.Add("Cache-Control", "no-cache");
                    wc.Encoding = Encoding.UTF8;
                    version = wc.DownloadString(_urlVersionFile).Replace("\r", "").Replace("\n", "");
                    versionLog = wc.DownloadString(_urlVersionChangelogFile).Replace("\r", "").Replace("\n", "");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            if (version != "") return new MMIVersion(version, versionLog);
            
            return null;
        }

        /// <summary>
        /// Notifies the user that a new update is available
        /// </summary>
        /// <param name="latestVersion">Object returned by FetchLatestVersionInfo</param>
        private static void NotifyNewUpdate(MMIVersion latestVersion)
        {
            string text = latestVersion.Changelog;

            try
            {
                WebClient client = new WebClient();
                
                if (text.Length > 90)
                {
                    string[] textlines = text.Split('\n');
                    string textToDiplay = "";

                    foreach (string line in textlines)
                    {
                        if (textToDiplay.Length + line.Length <= 90)
                        {
                            if (textToDiplay != "")
                                textToDiplay += "~n~" + line;
                            else
                                textToDiplay = line;
                        }
                        else
                        {
                            Tools.ShowNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("UpdateAvailable"), textToDiplay);
                            textToDiplay = line;
                        }
                    }
                    // Displays the last line
                    if (textToDiplay != "" && textToDiplay != "\r" && textToDiplay != "\n" && textToDiplay != "\r\n")
                    {
                        Tools.ShowNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("UpdateAvailable"), textToDiplay);
                    }
                }
                else
                {
                    Tools.ShowNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("UpdateAvailable"), text);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

    }
}
