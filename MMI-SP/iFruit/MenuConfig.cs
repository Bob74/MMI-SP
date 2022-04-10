using System;
using System.Collections.Generic;
using System.Drawing;

using NativeUI;
using GTA;
using GTA.Native;
using System.IO;

namespace MMI_SP.iFruit
{
    using T = Translator;

    class MenuConfig
    {
        private static string _menuTitle = T.GetString("ConfigMenuTitle");
        public string MenuTitle { get => _menuTitle; set => _menuTitle = value; }

        private static readonly Point _offset = SE.UI.GetScreenCoordinatesFromFloat(0.565f, 0.2f);

        private MenuPool _menuPool;
        UIMenu _mainMenu = new UIMenu(_menuTitle, T.GetString("ConfigMenuSubtitle"), _offset);


        internal void MenuPoolProcessMenus() { _menuPool.ProcessMenus(); }

        //Main Base
        public MenuConfig()
        {
            _menuPool = new MenuPool();
            _menuPool.Add(_mainMenu);

            UIMenu submenuGeneral = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemGeneral"), _offset);
            AddMenuConfigLanguage(submenuGeneral, "Language", Config.Settings.GetValue("General", "language", "default"), T.GetString("ConfigMenuGeneralLanguage"));
            AddMenuConfigCheckbox(submenuGeneral, "General", "PersistentInsuredVehicles", InsuranceObserver.PersistentVehicles, T.GetString("ConfigMenuGeneralPersistent"));

            UIMenu submenuiFruit = AddSubMenu(_menuPool, _mainMenu, _menuTitle, "iFruit", _offset);
            AddMenuConfigList(submenuiFruit, "iFruit", "PhoneVolume", MMISound.Volume, T.GetString("ConfigMenuiFruitPhoneVolume"), 0, 100, 5);
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitInsure", iFruitMMI.CaniFruitInsure, T.GetString("ConfigMenuiFruitInsure"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitCancel", iFruitMMI.CaniFruitCancel, T.GetString("ConfigMenuiFruitCancel"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitRecover", iFruitMMI.CaniFruitRecover, T.GetString("ConfigMenuiFruitRecover"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitStolen", iFruitMMI.CaniFruitStolen, T.GetString("ConfigMenuiFruitStolen"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitPlate", iFruitMMI.CaniFruitPlate, T.GetString("ConfigMenuiFruitPlate"));

            UIMenu submenuNotifications = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemNotify"), _offset);
            AddMenuConfigCheckbox(submenuNotifications, "Check", "CheckForUpdate", MMI.CheckForUpdate, T.GetString("ConfigMenuNotifyUpdate"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowSHVDNNotification", MMI.ShowSHVDNNotification, T.GetString("ConfigMenuNotifySHVDN"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowFileNotification", MMI.ShowFileNotification, T.GetString("ConfigMenuNotifyFile"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowVisualCNotification", MMI.ShowSHVDNNotification, T.GetString("ConfigMenuNotifyVisualC"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowNETFrameworkNotification", MMI.ShowSHVDNNotification, T.GetString("ConfigMenuNotifyNET"));
            submenuNotifications.AddItem(new UIMenuItem(T.GetString("ConfigMenuNotifyReboot")) { Enabled = false } );

            UIMenu submenuInsurance = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemInsurance"), _offset);
            AddMenuConfigList(submenuInsurance, "Insurance", "InsuranceCostMultiplier", InsuranceManager.InsuranceMult, GetCostMultiplierDescription("InsuranceCostMultiplier"), 0f, 100f, 0.1f);
            AddMenuConfigList(submenuInsurance, "Insurance", "RecoverCostMultiplier", InsuranceManager.RecoverMult, GetCostMultiplierDescription("RecoverCostMultiplier"), 0f, 100f, 0.1f);
            AddMenuConfigList(submenuInsurance, "Insurance", "StolenCostMultiplier", InsuranceManager.StolenMult, GetCostMultiplierDescription("StolenCostMultiplier"), 0f, 100f, 0.1f);

            UIMenu submenuBringVehicle = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemBringVehicle"), _offset);
            AddMenuConfigList(submenuBringVehicle, "BringVehicle", "BringVehicleBasePrice", InsuranceManager.BringVehicleBasePrice, T.GetString("ConfigMenuBringVehiclePrice"), 0, 2000, 50);
            AddMenuConfigBringVehicleInstant(submenuBringVehicle, "BringVehicle", "BringVehicleInstant", InsuranceManager.BringVehicleInstant, "");
            AddMenuConfigList(submenuBringVehicle, "BringVehicle", "BringVehicleRadius", InsuranceObserver.BringVehicleRadius, T.GetString("ConfigMenuBringVehicleRadius"), 10, 2000, 5);
            AddMenuConfigList(submenuBringVehicle, "BringVehicle", "BringVehicleTimeout", InsuranceObserver.BringVehicleTimeout, T.GetString("ConfigMenuBringVehicleTimeout"), 1, 30, 1);
        }

        internal void Show()
        {
            _mainMenu.Visible = true;
            Function.Call(Hash._0xFC695459D4D0E219, 0.5f, 0.5f);    // Cursor position centered
        }



        private void AddMenuConfigLanguage(UIMenu menu, string key, string value, string description)
        {
            bool found = false;
            int counter = 0;
            List<dynamic> languages = new List<dynamic>();

            foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\MMI\\", "*.xml"))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Name != "db.xml")
                { 
                    string name = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
                    languages.Add(name);
                    if (!found)
                        if (string.Compare(value, name, true) == 0)
                            found = true;
                        else
                            counter++;
                }
            }

            UIMenuListItem listItem = new UIMenuListItem(key, languages, counter, description);
            menu.AddItem(listItem);
            menu.OnListChange += (sender, item, index) =>
            {
                if (item == listItem)
                {
                    Config.Settings.SetValue("General", key, item.IndexToItem(index).ToString());
                    Config.Settings.Save();
                }
            };
        }

        private void AddMenuConfigBringVehicleInstant(UIMenu menu, string section, string key, bool isChecked, string description)
        {
            string textTrue = T.GetString("ConfigMenuBringVehicleInstantTrue");
            string textFalse = T.GetString("ConfigMenuBringVehicleInstantFalse");

            UIMenuCheckboxItem notifyItem = new UIMenuCheckboxItem(key, isChecked, description);
            if (notifyItem.Checked)
                notifyItem.Description = textTrue;
            else
                notifyItem.Description = textFalse;

            menu.AddItem(notifyItem);
            menu.OnCheckboxChange += (sender, item, index) =>
            {
                if (item == notifyItem)
                {
                    Config.Settings.SetValue(section, key, item.Checked);
                    UpdateValue(key, item.Checked);
                    Config.Settings.Save();
                    if (item.Checked)
                        item.Description = textTrue;
                    else
                        item.Description = textFalse;
                }
            };
        }

        private void AddMenuConfigList(UIMenu menu, string section, string key, float value, string description, float startValue, float stopValue, float increment)
        {
            bool found = false;
            int counter = 0;
            List<dynamic> values = new List<dynamic>();

            for (float i = startValue; i < stopValue; i += increment)
            {
                values.Add(Math.Round(i, 1, MidpointRounding.AwayFromZero));
                if (!found)
                    if (Math.Round(value, 1, MidpointRounding.AwayFromZero) == Math.Round(i, 1, MidpointRounding.AwayFromZero))
                        found = true;
                    else
                        counter++;
            }

            UIMenuListItem listItem = new UIMenuListItem(key, values, counter, description);
            menu.AddItem(listItem);
            menu.OnListChange += (sender, item, index) =>
            {
                if (item == listItem)
                {
                    Config.Settings.SetValue(section, key, ((float)item.IndexToItem(index)).ToString().ToString().Replace(",", "."));
                    UpdateValue(key, (float)item.IndexToItem(index));
                    Config.Settings.Save();
                }
            };
        }
        private void AddMenuConfigList(UIMenu menu, string section, string key, int value, string description, int startValue, int stopValue, int increment)
        {
            bool found = false;
            int counter = 0;
            List<dynamic> values = new List<dynamic>();

            for (int i = startValue; i <= stopValue; i += increment)
            {
                values.Add(i);
                if (!found)
                    if (value == i)
                        found = true;
                    else
                        counter++;
            }

            UIMenuListItem listItem = new UIMenuListItem(key, values, counter, description);
            menu.AddItem(listItem);
            menu.OnListChange += (sender, item, index) =>
            {
                if (item == listItem)
                {
                    Config.Settings.SetValue(section, key, (int)item.IndexToItem(index));
                    UpdateValue(key, (int)item.IndexToItem(index));
                    Config.Settings.Save();
                }
            };
        }
        private void AddMenuConfigCheckbox(UIMenu menu, string section, string key, bool isChecked, string description)
        {
            UIMenuCheckboxItem notifyItem = new UIMenuCheckboxItem(key, isChecked, description);
            menu.AddItem(notifyItem);
            menu.OnCheckboxChange += (sender, item, index) =>
            {
                if (item == notifyItem)
                {
                    Config.Settings.SetValue(section, key, item.Checked);
                    UpdateValue(key, item.Checked);
                    Config.Settings.Save();
                }
            };
        }




        private void UpdateValue(string key, object value)
        {
            if (string.Compare(key, "PersistentInsuredVehicles", true) == 0)
                InsuranceObserver.PersistentVehicles = (bool)value;
            else if (string.Compare(key, "InsuranceCostMultiplier", true) == 0)
                InsuranceManager.InsuranceMult = (float)value;
            else if (string.Compare(key, "RecoverCostMultiplier", true) == 0)
                InsuranceManager.RecoverMult = (float)value;
            else if (string.Compare(key, "StolenCostMultiplier", true) == 0)
                InsuranceManager.StolenMult = (float)value;
            else if (string.Compare(key, "PhoneVolume", true) == 0)
                MMISound.Volume = (int)value;
            else if (string.Compare(key, "CaniFruitInsure", true) == 0)
                iFruitMMI.CaniFruitInsure = (bool)value;
            else if (string.Compare(key, "CaniFruitCancel", true) == 0)
                iFruitMMI.CaniFruitCancel = (bool)value;
            else if (string.Compare(key, "CaniFruitRecover", true) == 0)
                iFruitMMI.CaniFruitRecover = (bool)value;
            else if (string.Compare(key, "CaniFruitStolen", true) == 0)
                iFruitMMI.CaniFruitStolen = (bool)value;
            else if (string.Compare(key, "CaniFruitPlate", true) == 0)
                iFruitMMI.CaniFruitPlate = (bool)value;
            else if (string.Compare(key, "BringVehicleBasePrice", true) == 0)
                InsuranceManager.BringVehicleBasePrice = (int)value;
            else if (string.Compare(key, "BringVehicleInstant", true) == 0)
                InsuranceManager.BringVehicleInstant = (bool)value;
            else if (string.Compare(key, "BringVehicleRadius", true) == 0)
                InsuranceObserver.BringVehicleRadius = (int)value;
            else if (string.Compare(key, "BringVehicleTimeout", true) == 0)
                InsuranceObserver.BringVehicleTimeout = (int)value;
        }

        private string GetCostMultiplierDescription(string costType)
        {
            Vehicle playerVehicle = Game.Player.LastVehicle;
            if (playerVehicle != null)
            {
                if (string.Compare(costType, "InsuranceCostMultiplier", true) == 0)
                    return T.GetString("ConfigMenuInsuranceInsurance") + " " + T.GetString("ConfigMenuInsuranceInsuranceEx");
                else if (string.Compare(costType, "RecoverCostMultiplier", true) == 0)
                    return T.GetString("ConfigMenuInsuranceRecover") + " " + T.GetString("ConfigMenuInsuranceRecoverEx");
                else if (string.Compare(costType, "StolenCostMultiplier", true) == 0)
                    return T.GetString("ConfigMenuInsuranceStolen") + " " + T.GetString("ConfigMenuInsuranceStolenEx");
            }
            else
            {
                if (string.Compare(costType, "InsuranceCostMultiplier", true) == 0)
                    return T.GetString("ConfigMenuInsuranceInsurance");
                else if (string.Compare(costType, "RecoverCostMultiplier", true) == 0)
                    return T.GetString("ConfigMenuInsuranceRecover");
                else if (string.Compare(costType, "StolenCostMultiplier", true) == 0)
                    return T.GetString("ConfigMenuInsuranceStolen");
            }
            return "";
        }

        // Workaround since NativeUI.MenuPool doesn't have an AddSubMenu function supporting menu Offset
        private UIMenu AddSubMenu(MenuPool pool, UIMenu menu, string title, string text, Point offset)
        {
            var item = new UIMenuItem(text);
            menu.AddItem(item);
            var submenu = new UIMenu(title, text, offset);
            pool.Add(submenu);
            menu.BindMenuToItem(submenu, item);
            return submenu;
        }

    }
}
