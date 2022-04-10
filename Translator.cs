using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;

using GTA;

namespace MMI_SP
{
    static class Translator
    {
        private class LocalizedString
        {
            public string ID { get; set; }
            public string value { get; set; }
            public LocalizedString(string id, string str)
            {
                ID = id;
                value = str;
            }
        }

        private static string _languageFilePath;
        private static XElement _languageFile;

        private static List<LocalizedString> _strings = new List<LocalizedString>();

    
        public static void Initialize(string fileName)
        {
            _languageFilePath = fileName;
            if (!File.Exists(_languageFilePath))
            {
                Logger.Info("Error: Language file does not exist! " + _languageFilePath + " (Check the language value in the config file and check if the file exist)");
                SE.UI.DrawNotification("MMI-SP: ERROR - Language file does not exist! See \"GTA V\\MMI-SP.log\"");
            }
            else
            {
                _languageFile = XElement.Load(_languageFilePath);
                GetAllStrings();
            }     
        }

        public static string GetString(string ID)
        {
            LocalizedString result = _strings.Find(x => x.ID == ID);
            if (result != null)
                return ReplaceVariablesInString(result.value);
            else
                return "UNKNOWN";
        }

        private static void GetAllStrings()
        {
            if (_languageFile.Element("Strings") != null)
            {
                foreach (XElement section in _languageFile.Element("Strings").Elements())
                    if (section != null)
                        foreach (XElement str in section.Elements())
                            _strings.Add(new LocalizedString(str.Name.LocalName, str.Value));
            }
            else
                Logger.Info("Error: Translator.GetAllStrings - Incomplete language file (cannot find \"Strings\").");
        }

        private static string ReplaceVariablesInString(string str)
        {
            Vehicle playerVehicle = Game.Player.LastVehicle;

            if (str.Contains("$VehicleInsuranceCost"))
                str = str.Replace("$VehicleInsuranceCost", InsuranceManager.GetVehicleInsuranceCost(playerVehicle).ToString());
            if (str.Contains("$VehicleRecoverCost"))
                str = str.Replace("$VehicleRecoverCost", InsuranceManager.GetVehicleInsuranceCost(playerVehicle, InsuranceManager.Multiplier.Recover).ToString());
            if (str.Contains("$VehicleStolenCost"))
                str = str.Replace("$VehicleStolenCost", InsuranceManager.GetVehicleInsuranceCost(playerVehicle, InsuranceManager.Multiplier.Stolen).ToString());
            if (str.Contains("$VehicleFriendlyName"))
                str = str.Replace("$VehicleFriendlyName", SE.Vehicle.GetVehicleFriendlyName(playerVehicle, false));
            if (str.Contains("$VehicleFriendlyNameFull"))
                str = str.Replace("$VehicleFriendlyNameFull", SE.Vehicle.GetVehicleFriendlyName(playerVehicle));

            if (str.Contains("$InsureVehicle"))
                str = str.Replace("$InsureVehicle", GetString("InsureVehicle"));
            if (str.Contains("$CancelInsurance"))
                str = str.Replace("$CancelInsurance", GetString("CancelInsurance"));
            if (str.Contains("$RecoverVehicle"))
                str = str.Replace("$RecoverVehicle", GetString("RecoverVehicle"));
            if (str.Contains("$StolenVehicle"))
                str = str.Replace("$StolenVehicle", GetString("StolenVehicle"));
            if (str.Contains("$BringVehicle"))
                str = str.Replace("$BringVehicle", GetString("BringVehicle"));
            if (str.Contains("$PlateChange"))
                str = str.Replace("$PlateChange", GetString("PlateChange"));

            return str;
        }

    }
}
