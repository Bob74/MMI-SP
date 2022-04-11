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
        
        public static ScriptSettings Settings { get => _settings; }
        private static ScriptSettings _settings;


        // Checks
        private static bool _checkForUpdate;
        public static bool CheckForUpdate { get => _checkForUpdate; }
        private static bool _showSHVDNNotification;
        public static bool ShowSHVDNNotification { get => _showSHVDNNotification; }
        private static bool _showFileNotification;
        public static bool ShowFileNotification { get => _showFileNotification; }
        private static bool _showVisualCNotification;
        public static bool ShowVisualCNotification { get => _showVisualCNotification; }
        private static bool _showNETFrameworkNotification;
        public static bool ShowNETFrameworkNotification { get => _showNETFrameworkNotification; }


        // BringVehicle
        private static int _bringVehicleBasePrice;
        public static int BringVehicleBasePrice { get => _bringVehicleBasePrice; }
        private static int _bringVehicleRadius;
        public static int BringVehicleRadius { get => _bringVehicleRadius; }
        private static int _bringVehicleTimeout;
        public static int BringVehicleTimeout { get => _bringVehicleTimeout; }
        private static bool _bringVehicleInstant;
        public static bool BringVehicleInstant { get => _bringVehicleInstant; }


        // General
        private static bool _persistentVehicles;
        public static bool PersistentVehicles { get => _persistentVehicles; }


        // Insurance
        private static float _insuranceMult;
        public static float InsuranceMult { get => _insuranceMult; }
        private static float _recoverMult;
        public static float RecoverMult { get => _recoverMult; }
        private static float _stolenMult;
        public static float StolenMult { get => _stolenMult; }


        // iFruit
        public static int iFruitVolume { get => _iFruitVolume; }
        private static int _iFruitVolume;
        public static bool CaniFruitInsure { get => _caniFruitInsure; }
        private static bool _caniFruitInsure;
        public static bool CaniFruitCancel { get => _caniFruitCancel; }
        private static bool _caniFruitCancel;
        public static bool CaniFruitRecover { get => _caniFruitRecover; }
        private static bool _caniFruitRecover;
        public static bool CaniFruitStolen { get => _caniFruitStolen; }
        private static bool _caniFruitStolen;
        public static bool CaniFruitPlate { get => _caniFruitPlate; }
        private static bool _caniFruitPlate;


        public static void Initialize()
        {
            if (!Directory.Exists(BaseDir))
            {
                Logger.Debug("Creating config directory");
                Directory.CreateDirectory(BaseDir);
            }
            if (!File.Exists(ConfigFile))
            {
                Logger.Debug("Creating config file");
                File.WriteAllText(ConfigFile, Properties.Resources.config);
            }
            if (!File.Exists(BannerImage))
            {
                Logger.Debug("Creating banner image file");
                Properties.Resources.banner.Save(BannerImage);
            }
            if (!File.Exists(InsuranceImage))
            {
                Logger.Debug("Creating insurance image file");
                Properties.Resources.insurance.Save(InsuranceImage);
            }
            if (!File.Exists(LanguageFile))
            {
                Logger.Debug("Creating default language file");
                File.WriteAllText(LanguageFile, Properties.Resources._default);
            }
            
            if (File.Exists(ConfigFile))
            {
                Logger.Debug("Loading config file");
                _settings = ScriptSettings.Load(ConfigFile);
            }
            else
            {
                Logger.Error("Settings.Initialize - Config file cannot be found! " + ConfigFile);
            }

            Logger.Debug("Reading config file");
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
            T.Initialize(BaseDir + "\\" + languageFile + ".xml");
            _persistentVehicles = Settings.GetValue("General", "PersistentInsuredVehicles", false);


            // BringVehicle
            _bringVehicleBasePrice = Settings.GetValue("BringVehicle", "BringVehicleBasePrice", 200);
            _bringVehicleRadius = Settings.GetValue("BringVehicle", "BringVehicleRadius", 100);
            _bringVehicleTimeout = Settings.GetValue("BringVehicle", "BringVehicleTimeout", 5);
            _bringVehicleInstant = Settings.GetValue("BringVehicle", "BringVehicleInstant", false);



            // Insurance
            if (float.TryParse(Settings.GetValue<string>("Insurance", "InsuranceCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out float insuranceMult))
            {
                _insuranceMult = Math.Abs(insuranceMult);
            }
            else
            {
                Logger.Error("Unable to read float value InsuranceCostMultiplier in config file.");
            }

            if (float.TryParse(Settings.GetValue<string>("Insurance", "RecoverCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out float recoverMult))
            {
                _recoverMult = Math.Abs(recoverMult);
            }
            else
            {
                Logger.Error("Unable to read float value RecoverCostMultiplier in config file.");
            }

            if (float.TryParse(Settings.GetValue<string>("Insurance", "StolenCostMultiplier", "1.0"), NumberStyles.Any, CultureInfo.InvariantCulture, out float stolenMult))
            { 
                _stolenMult = Math.Abs(stolenMult);
            }
            else
            {
                Logger.Error("Unable to read float value StolenCostMultiplier in config file.");
            }                

            // iFruit
            _iFruitVolume = Settings.GetValue("General", "PhoneVolume", 25);
            if (_iFruitVolume < 0) _iFruitVolume = 0;
            if (_iFruitVolume > 100) _iFruitVolume = 100;
            _caniFruitInsure = Settings.GetValue("iFruit", "CaniFruitInsure", false);
            _caniFruitCancel = Settings.GetValue("iFruit", "CaniFruitCancel", false);
            _caniFruitRecover = Settings.GetValue("iFruit", "CaniFruitRecover", true);
            _caniFruitStolen = Settings.GetValue("iFruit", "CaniFruitStolen", false);
            _caniFruitPlate = Settings.GetValue("iFruit", "CaniFruitPlate", false);
        }

        public static void UpdateValue(string key, object value)
        {
            if (string.Compare(key, "PersistentInsuredVehicles", true) == 0)
                _persistentVehicles = (bool)value;
            else if (string.Compare(key, "InsuranceCostMultiplier", true) == 0)
                _insuranceMult = (float)value;
            else if (string.Compare(key, "RecoverCostMultiplier", true) == 0)
                _recoverMult = (float)value;
            else if (string.Compare(key, "StolenCostMultiplier", true) == 0)
                _stolenMult = (float)value;
            else if (string.Compare(key, "PhoneVolume", true) == 0)
                _iFruitVolume = (int)value;
            else if (string.Compare(key, "CaniFruitInsure", true) == 0)
                _caniFruitInsure = (bool)value;
            else if (string.Compare(key, "CaniFruitCancel", true) == 0)
                _caniFruitCancel = (bool)value;
            else if (string.Compare(key, "CaniFruitRecover", true) == 0)
                _caniFruitRecover = (bool)value;
            else if (string.Compare(key, "CaniFruitStolen", true) == 0)
                _caniFruitStolen = (bool)value;
            else if (string.Compare(key, "CaniFruitPlate", true) == 0)
                _caniFruitPlate = (bool)value;
            else if (string.Compare(key, "BringVehicleBasePrice", true) == 0)
                _bringVehicleBasePrice = (int)value;
            else if (string.Compare(key, "BringVehicleInstant", true) == 0)
                _bringVehicleInstant = (bool)value;
            else if (string.Compare(key, "BringVehicleRadius", true) == 0)
                _bringVehicleRadius = (int)value;
            else if (string.Compare(key, "BringVehicleTimeout", true) == 0)
                _bringVehicleTimeout = (int)value;
        }

    }
}
