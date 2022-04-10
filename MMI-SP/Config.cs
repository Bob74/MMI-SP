using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

using GTA;

namespace MMI_SP
{
    using T = Translator;
    
    internal static class Config
    {
        private static string _baseDir = AppDomain.CurrentDomain.BaseDirectory + "\\MMI";
        private static string _configFile = _baseDir + "\\config.ini";
        private static string _bannerImage = _baseDir + "\\banner.png";
        private static string _insuranceImage = _baseDir + "\\insurance.png";
        private static string _languageFile = _baseDir + "\\default.xml";

        private static ScriptSettings _settings;
        public static ScriptSettings Settings { get => _settings; private set => _settings = value; }



        private static bool _checkForUpdate = true;
        public static bool CheckForUpdate { get => _checkForUpdate; }
        private static bool _showSHVDNNotification = true;
        public static bool ShowSHVDNNotification { get => _showSHVDNNotification; private set => _showSHVDNNotification = value; }
        private static bool _showFileNotification = true;
        public static bool ShowFileNotification { get => _showFileNotification; private set => _showFileNotification = value; }
        private static bool _showVisualCNotification = true;
        public static bool ShowVisualCNotification { get => _showVisualCNotification; private set => _showVisualCNotification = value; }
        private static bool _showNETFrameworkNotification = true;
        public static bool ShowNETFrameworkNotification { get => _showNETFrameworkNotification; private set => _showNETFrameworkNotification = value; }


        public static void Initialize()
        {
            if (!Directory.Exists(_baseDir))
            {
                Logger.Info("Creating config directory.");
                Directory.CreateDirectory(_baseDir);
            }
            if (!File.Exists(_configFile)) {
                Logger.Info("Creating config file.");
                File.WriteAllText(_configFile, Properties.Resources.config);
            }
            if (!File.Exists(_bannerImage)) {
                Logger.Info("Creating banner image file.");
                Properties.Resources.banner.Save(_bannerImage);
            }
            if (!File.Exists(_insuranceImage))
            {
                Logger.Info("Creating insurance image file.");
                Properties.Resources.insurance.Save(_insuranceImage);
            }
            if (!File.Exists(_languageFile)) {
                Logger.Info("Creating default language file.");
                File.WriteAllText(_languageFile, Properties.Resources._default);
            }
            if (File.Exists(_configFile))
                _settings = ScriptSettings.Load(_configFile);
            else
                Logger.Info("Error: Settings.Initialize - Config file cannot be found! " + _configFile);

            LoadConfigValues();
        }


        /// <summary>
        /// Set the variables according to the values contained in the config file.
        /// </summary>
        private static void LoadConfigValues()
        {
            // Dummy to force the game to load the texture
            SE.UI.DrawTexture(AppDomain.CurrentDomain.BaseDirectory + "\\MMI\\insurance.png", 1000, 2.0f, 2.0f, Color.FromArgb(35, 199, 128));

            // Checks
            _checkForUpdate = Config.Settings.GetValue("Check", "CheckForUpdate", true);
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
                Logger.Info("Error: MMI - Unable to read float value InsuranceCostMultiplier in config file.");
            else
                InsuranceManager.InsuranceMult = Math.Abs(insuranceMult);
            if (!float.TryParse(Config.Settings.GetValue<string>("Insurance", "RecoverCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out recoverMult))
                Logger.Info("Error: MMI - Unable to read float value RecoverCostMultiplier in config file.");
            else
                InsuranceManager.RecoverMult = Math.Abs(recoverMult);
            if (!float.TryParse(Config.Settings.GetValue<string>("Insurance", "StolenCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out stolenMult))
                Logger.Info("Error: MMI - Unable to read float value StolenCostMultiplier in config file.");
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

    }
}
