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
        internal static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory + "\\MMI";
        internal static readonly string ConfigFile = BaseDir + "\\config.ini";
        internal static readonly string BannerImage = BaseDir + "\\banner.png";
        internal static readonly string InsuranceImage = BaseDir + "\\insurance.png";
        internal static readonly string LanguageFile = BaseDir + "\\default.xml";
        
        private static ScriptSettings _settings;
        public static ScriptSettings Settings { get => _settings; }



        private static bool _checkForUpdate = true;
        public static bool CheckForUpdate { get => _checkForUpdate; }
        private static bool _showSHVDNNotification = true;
        public static bool ShowSHVDNNotification { get => _showSHVDNNotification; }
        private static bool _showFileNotification = true;
        public static bool ShowFileNotification { get => _showFileNotification; }
        private static bool _showVisualCNotification = true;
        public static bool ShowVisualCNotification { get => _showVisualCNotification; }
        private static bool _showNETFrameworkNotification = true;
        public static bool ShowNETFrameworkNotification { get => _showNETFrameworkNotification; }


        public static void Initialize()
        {
            if (!Directory.Exists(BaseDir))
            {
                Logger.Debug("Creating config directory.");
                Directory.CreateDirectory(BaseDir);
            }
            if (!File.Exists(ConfigFile)) {
                Logger.Debug("Creating config file.");
                File.WriteAllText(ConfigFile, Properties.Resources.config);
            }
            if (!File.Exists(BannerImage)) {
                Logger.Debug("Creating banner image file.");
                Properties.Resources.banner.Save(BannerImage);
            }
            if (!File.Exists(InsuranceImage))
            {
                Logger.Debug("Creating insurance image file.");
                Properties.Resources.insurance.Save(InsuranceImage);
            }
            if (!File.Exists(LanguageFile)) {
                Logger.Debug("Creating default language file.");
                File.WriteAllText(LanguageFile, Properties.Resources._default);
            }
            
            if (File.Exists(ConfigFile))
            {
                _settings = ScriptSettings.Load(ConfigFile);
            }
            else
            {
                Logger.Error("Settings.Initialize - Config file cannot be found! " + ConfigFile);
            }

            LoadConfigValues();
        }


        /// <summary>
        /// Set the variables according to the values contained in the config file.
        /// </summary>
        private static void LoadConfigValues()
        {
            // Dummy to force the game to load the texture
            SE.UI.DrawTexture(InsuranceImage, 1000, 2.0f, 2.0f, Color.FromArgb(35, 199, 128));

            // Checks
            _checkForUpdate = Settings.GetValue("Check", "CheckForUpdate", true);
            _showSHVDNNotification = Settings.GetValue("Check", "ShowSHVDNNotification", true);
            _showFileNotification = Settings.GetValue("Check", "ShowFileNotification", true);
            _showVisualCNotification = Settings.GetValue("Check", "ShowVisualCNotification", true);
            _showNETFrameworkNotification = Settings.GetValue("Check", "ShowNETFrameworkNotification", true);


            // General
            string languageFile = Settings.GetValue("General", "language", "default");
            T.Initialize(BaseDir + languageFile + ".xml");
            InsuranceObserver.PersistentVehicles = Settings.GetValue("General", "PersistentInsuredVehicles", false);


            // BringVehicle
            InsuranceManager.BringVehicleBasePrice = Settings.GetValue("BringVehicle", "BringVehicleBasePrice", 200);
            InsuranceObserver.BringVehicleRadius = Settings.GetValue("BringVehicle", "BringVehicleRadius", 100);
            InsuranceObserver.BringVehicleTimeout = Settings.GetValue("BringVehicle", "BringVehicleTimeout", 5);
            InsuranceManager.BringVehicleInstant = Settings.GetValue("BringVehicle", "BringVehicleInstant", false);


            // Insurance
            if (float.TryParse(Settings.GetValue<string>("Insurance", "InsuranceCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out float insuranceMult))
            {
                InsuranceManager.InsuranceMult = Math.Abs(insuranceMult);
            }
            else
            {
                Logger.Info("Error: MMI - Unable to read float value InsuranceCostMultiplier in config file.");
            }

            if (float.TryParse(Settings.GetValue<string>("Insurance", "RecoverCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out float recoverMult))
            {
                InsuranceManager.RecoverMult = Math.Abs(recoverMult);
            }
            else
            {
                Logger.Info("Error: MMI - Unable to read float value RecoverCostMultiplier in config file.");
            }

            if (float.TryParse(Settings.GetValue<string>("Insurance", "StolenCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out float stolenMult))
            { 
                InsuranceManager.StolenMult = Math.Abs(stolenMult);
            }
            else
            {
                Logger.Info("Error: MMI - Unable to read float value StolenCostMultiplier in config file.");
            }                

            // iFruit
            iFruit.MMISound.Volume = Settings.GetValue("General", "PhoneVolume", 25);
            iFruit.iFruitMMI.CaniFruitInsure = Settings.GetValue("iFruit", "CaniFruitInsure", false);
            iFruit.iFruitMMI.CaniFruitCancel = Settings.GetValue("iFruit", "CaniFruitCancel", false);
            iFruit.iFruitMMI.CaniFruitRecover = Settings.GetValue("iFruit", "CaniFruitRecover", true);
            iFruit.iFruitMMI.CaniFruitStolen = Settings.GetValue("iFruit", "CaniFruitStolen", false);
            iFruit.iFruitMMI.CaniFruitPlate = Settings.GetValue("iFruit", "CaniFruitPlate", false);
        }

    }
}
