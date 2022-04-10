using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

using GTA;


/*
    Changelog (Update assembly version!):

        1.2.1 (10/04/2022)  - Fixed persistence issue.
                            - Released (ugly) source code.

        1.2.0a(25/02/2018)  - Fixed an issue where insuring a vehicle with the office without having used the phone before lead to a crash.
                            - Fixed a bug where recovered vehicle blip wouldn't disappear if you changed its plate number before getting into the vehicle.
                            - Fixed missing translations.

        1.2.0 (24/02/2018)  - You can edit the config file in game using a phone contact! Settings will take effect immediately unless specified otherwise.
                            - License plate number can now be changed! Go to the office or use you phone if you've enabled this choice.
                            - Larger choice of insurable vehicles. Barely everything besides trains and main character's vehicles.
                            - Insured vehicles are now persistent by default. They won't despawn until they dies. This setting can be changed.
                            - You can now select available choices in your phone menu (insure a car, report as stolen, etc.). Edit the mods settings to enable a choice.
                            - The menus are a bit more friendly (some choices will disappear when needed instead of being grayed out).
                            - Insured icon is now only displayed on vehicles that can be insured.
                            - Added code related to bring helicopters and planes to you since these vehicles are now insurable.
                            - Log file (MMI-SP.log) is reset at launch.
                            - Readjusted prices. Remember you can tweak the price using the mod settings.
                            - Fixed an issue where the mod could crash if another mod spawns vehicles with badly formated color for tire or neon.
                            - Fixed an issue where you could ask MMI to bring you another character's vehicle under some circumstances.

        1.1.4a(13/02/2018): - Ability to disable updates check and notifications when a prerequisite is missing or outdated
                            - Better Visual C++ version detection

        1.1.4 (12/02/2018): - The driver now stops and leaves the car when he is close to the player instead of only when he has reached his destination
                            - The mod checks if all prerequisites are installed and tells you if anything is missing or outdated
                            - Clearer error messages when missing prerequisites and less crash
                            - Now using SHVDN-Extender

        1.1.3a(10/02/2018): - Hot fix: Stop removing all blips from the vehicle you enter

        1.1.3 (10/02/2018): - Added a "safe mode" when entering the office. If there is a problem with your game at this moment, the office won't be displayed but the menu will appear and the mod will still work.
                            - MMI phone contact's volume is adjustable via config file

        1.1.2a(31/01/2018): - MMI can now bring you your insured vehicle if it exists on the map (you will need to set PersistentInsuredVehicles to true in the config file if you want to be able to let your vehicle somewhere and later ask MMI to bring it to you)
                            - Persistent vehicles are now able to despawn when destroyed

        1.1.2 (30/01/2018): - Fixed a bug where convertible vehicles couldn't be spawn
                            - Fixed Tornado roof not being saved properly
                            - Now the phone will close shortly after the MMI menu appears (NativeUI & the phone uses the same controls. It made you browse the phone while you were browsing the menu)
                            - The driver will be a bit nicer with your car.
                            - Updated iFruitAddon2

        1.1.1 (28/01/2018): - Detailed config file
                            - Fixed a crash when some mod's files were missing
                            - Fixed an issue when you wanted MMI to bring you many vehicles at the same time
                            - Added a timeout if the driver gets lost, the vehicle will return to the depot and you will be refund
                            - Updated iFruitAddon2

        1.1.0 (27/01/2018): - Licence Plate Randomizer not needed anymore
                            - You can now ask MMI to bring your freshly recovered vehicle to you!
                            - Added Update detection (from an online Github file)
                            - Menu are updated when they needs to. No longer need to close and open menu again under certain circumstances
                            - Fixed some missing texts

        1.0.1 (26/01/2018): - Fixed an issue with the vehicle recovery description in the menus.
                            - Force persistence of office NPC and Props

        1.0.0 (26/01/2018): - Initial release
*/
namespace MMI_SP
{
    using T = Translator;

    class MMI : Script
    {
        private static bool _initialized = false;
        public static bool Initialized { get => _initialized; private set => _initialized = value; }

        private static bool _checkForUpdate = true;
        public static bool CheckForUpdate { get => _checkForUpdate; private set => _checkForUpdate = value; }
        private static bool _showSHVDNNotification = true;
        public static bool ShowSHVDNNotification { get => _showSHVDNNotification; private set => _showSHVDNNotification = value; }
        private static bool _showFileNotification = true;
        public static bool ShowFileNotification { get => _showFileNotification; private set => _showFileNotification = value; }
        private static bool _showVisualCNotification = true;
        public static bool ShowVisualCNotification { get => _showVisualCNotification; private set => _showVisualCNotification = value; }
        private static bool _showNETFrameworkNotification = true;
        public static bool ShowNETFrameworkNotification { get => _showNETFrameworkNotification; private set => _showNETFrameworkNotification = value; }


        public MMI()
        {
            Tick += Initialize;
        }

        private void Initialize(object sender, EventArgs e)
        {
            // Reset log file
            Logger.ResetLogFile();

            while (Game.IsLoading)
                Yield();
            while (Game.IsScreenFadingIn)
                Yield();

            LoadConfigValues();

            if (ArePrerequisitesInstalled() && CheckForUpdate)
                if (IsUpdateAvailable()) NotifyNewUpdate();

            _initialized = true;

            Tick -= Initialize;
        }

        private bool ArePrerequisitesInstalled()
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
                        if (ShowFileNotification) Debug.DebugNotification("CHAR_BLOCKED", "MMI-SP", key + " v" + fileInfo.ProductVersion + " is outdated!", "Download and install the latest version.");
                        Logger.Log("Error: " + key + " v" + fileInfo.ProductVersion + " is outdated!");
                        installed = false;
                    }
                }
                else
                {
                    if (ShowFileNotification) Debug.DebugNotification("CHAR_BLOCKED", "MMI-SP", key + " is missing!", "Download and install this file before starting the game.");
                    Logger.Log("Error: " + key + " is missing!");
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
                    if (ShowSHVDNNotification) Debug.DebugNotification("CHAR_BLOCKED", "MMI-SP", "ScriptHookVDotNet2 is outdated!", "Download and install the latest version.");
                    Logger.Log("Error: ScriptHookVDotNet2 is outdated!");
                    installed = false;
                }
            }

            if (!Tools.IsVisualCVersionHigherOrEqual(Tools.VisualCVersion.Visual_2015))
            {
                if (ShowVisualCNotification) Debug.DebugNotification("CHAR_BLOCKED", "MMI-SP", "Microsoft Visual C++ is missing!", "Download and install version 2015 or 2017 x64.");
                Logger.Log("Error: Microsoft Visual C++ 2015 and 2017 x64 is missing!");
                installed = false;
            }
            if (Tools.GetNETFrameworkVersion().CompareTo(dotNetVersion) < 0)
            {
                if (ShowNETFrameworkNotification) Debug.DebugNotification("CHAR_BLOCKED", "MMI-SP", "Microsoft .NET Framework is outdated!", "Download and install version 4.8 or later.");
                Logger.Log("Error: Microsoft .NET Framework is outdated!");
                installed = false;
            }

            return installed;
        }

        /// <summary>
        /// Set the variables according to the values contained in the config file.
        /// </summary>
        private void LoadConfigValues()
        {
            Config.Initialize();

            // Dummy to force the game to load the texture
            SE.UI.DrawTexture(AppDomain.CurrentDomain.BaseDirectory + "\\MMI\\insurance.png", 1000, 2.0f, 2.0f, Color.FromArgb(35, 199, 128));

            // Checks
            CheckForUpdate = Config.Settings.GetValue("Check", "CheckForUpdate", true);
            ShowSHVDNNotification = Config.Settings.GetValue("Check", "ShowSHVDNNotification", true);
            ShowFileNotification = Config.Settings.GetValue("Check", "ShowFileNotification", true);
            ShowVisualCNotification = Config.Settings.GetValue("Check", "ShowVisualCNotification", true);
            ShowNETFrameworkNotification = Config.Settings.GetValue("Check", "ShowNETFrameworkNotification", true);


            // General
            string languageFile = Config.Settings.GetValue("General", "language", "default");
            T.Initialize(AppDomain.CurrentDomain.BaseDirectory + "\\MMI\\" + languageFile + ".xml");
            InsuranceObserver.PersistentVehicles = Config.Settings.GetValue("General", "PersistentInsuredVehicles", false);
            

            // BringVehicle
            InsuranceManager.BringVehicleBasePrice = Config.Settings.GetValue("BringVehicle", "BringVehicleBasePrice", 200);
            InsuranceObserver.BringVehicleRadius = Config.Settings.GetValue("BringVehicle", "BringVehicleRadius", 100);
            InsuranceObserver.BringVehicleTimeout = Config.Settings.GetValue("BringVehicle", "BringVehicleTimeout", 5);
            InsuranceManager.BringVehicleInstant = Config.Settings.GetValue("BringVehicle", "BringVehicleInstant", false);


            // Insurance
            float insuranceMult = 1.0f, recoverMult = 1.0f, stolenMult = 1.0f;
            if (!float.TryParse(Config.Settings.GetValue<string>("Insurance", "InsuranceCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out insuranceMult))
                Logger.Log("Error: MMI - Unable to read float value InsuranceCostMultiplier in config file.");
            else
                InsuranceManager.InsuranceMult = Math.Abs(insuranceMult);
            if (!float.TryParse(Config.Settings.GetValue<string>("Insurance", "RecoverCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out recoverMult))
                Logger.Log("Error: MMI - Unable to read float value RecoverCostMultiplier in config file.");
            else
                InsuranceManager.RecoverMult = Math.Abs(recoverMult);
            if (!float.TryParse(Config.Settings.GetValue<string>("Insurance", "StolenCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out stolenMult))
                Logger.Log("Error: MMI - Unable to read float value StolenCostMultiplier in config file.");
            else
                InsuranceManager.StolenMult = Math.Abs(stolenMult);


            // iFruit
            iFruit.MMISound.Volume = Config.Settings.GetValue("General", "PhoneVolume", 25);
            iFruit.iFruitMMI.CaniFruitInsure = Config.Settings.GetValue("iFruit", "CaniFruitInsure", false);
            iFruit.iFruitMMI.CaniFruitCancel = Config.Settings.GetValue("iFruit", "CaniFruitCancel", false);
            iFruit.iFruitMMI.CaniFruitRecover = Config.Settings.GetValue("iFruit", "CaniFruitRecover", true);
            iFruit.iFruitMMI.CaniFruitStolen = Config.Settings.GetValue("iFruit", "CaniFruitStolen", false);
            iFruit.iFruitMMI.CaniFruitPlate = Config.Settings.GetValue("iFruit", "CaniFruitPlate", false);
        }

        private bool IsUpdateAvailable()
        {
            Version onlineVersion;

            try
            {
                WebClient client = new WebClient();
                string downloadedString = client.DownloadString("https://raw.githubusercontent.com/Bob74/MMI-SP/master/version");

                downloadedString = downloadedString.Replace("\r", "");
                downloadedString = downloadedString.Replace("\n", "");

                onlineVersion = new Version(downloadedString);

                client.Dispose();

                if (onlineVersion.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                Logger.Log("Error: IsUpdateAvailable - " + e.Message);
            }

            return false;
        }

        private void NotifyNewUpdate()
        {
            string text = T.GetString("UpdateText");

            try
            {
                WebClient client = new WebClient();
                string downloadedString = client.DownloadString("https://raw.githubusercontent.com/Bob74/MMI-SP/master/versionlog");
                text = downloadedString;

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
                            Debug.DebugNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("UpdateAvailable"), textToDiplay);
                            textToDiplay = line;
                        }
                    }
                    // Displays the last line
                    if (textToDiplay != "" && textToDiplay != "\r" && textToDiplay != "\n" && textToDiplay != "\r\n")
                        Debug.DebugNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("UpdateAvailable"), textToDiplay);
                }
                else
                {
                    Debug.DebugNotification("char_mp_mors_mutual",
                    "MORS MUTUAL INSURANCE",
                    T.GetString("UpdateAvailable"),
                    text);
                }
            }
            catch (Exception e)
            {
                Logger.Log("Error: NotifyNewUpdate - " + e.Message);
            }
        }
    }
}
