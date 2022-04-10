using System;
using System.IO;

using GTA;

namespace MMI_SP
{
    static class Config
    {
        private static string _baseDir = AppDomain.CurrentDomain.BaseDirectory + "\\MMI";
        private static string _configFile = _baseDir + "\\config.ini";
        private static string _bannerImage = _baseDir + "\\banner.png";
        private static string _insuranceImage = _baseDir + "\\insurance.png";
        private static string _languageFile = _baseDir + "\\default.xml";

        private static ScriptSettings _settings;
        public static ScriptSettings Settings { get => _settings; private set => _settings = value; }

        public static void Initialize()
        {
            if (!Directory.Exists(_baseDir))
            {
                Logger.Log("Creating config directory.");
                Directory.CreateDirectory(_baseDir);
            }
            if (!File.Exists(_configFile)) {
                Logger.Log("Creating config file.");
                File.WriteAllText(_configFile, Properties.Resources.config);
            }
            if (!File.Exists(_bannerImage)) {
                Logger.Log("Creating banner image file.");
                Properties.Resources.banner.Save(_bannerImage);
            }
            if (!File.Exists(_insuranceImage))
            {
                Logger.Log("Creating insurance image file.");
                Properties.Resources.insurance.Save(_insuranceImage);
            }
            if (!File.Exists(_languageFile)) {
                Logger.Log("Creating default language file.");
                File.WriteAllText(_languageFile, Properties.Resources._default);
            }
            if (File.Exists(_configFile))
                _settings = ScriptSettings.Load(_configFile);
            else
                Logger.Log("Error: Settings.Initialize - Config file cannot be found! " + _configFile);
        }

    }
}
