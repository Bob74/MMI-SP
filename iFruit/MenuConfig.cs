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
            AddMenuConfigCheckbox(submenuGeneral, "General", "PersistentInsuredVehicles", Config.PersistentVehicles, T.GetString("ConfigMenuGeneralPersistent"));

            UIMenu submenuiFruit = AddSubMenu(_menuPool, _mainMenu, _menuTitle, "iFruit", _offset);
            AddMenuConfigList(submenuiFruit, "iFruit", "PhoneVolume", Config.iFruitVolume, T.GetString("ConfigMenuiFruitPhoneVolume"), 0, 100, 5);
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitInsure", Config.CaniFruitInsure, T.GetString("ConfigMenuiFruitInsure"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitCancel", Config.CaniFruitCancel, T.GetString("ConfigMenuiFruitCancel"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitRecover", Config.CaniFruitRecover, T.GetString("ConfigMenuiFruitRecover"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitStolen", Config.CaniFruitStolen, T.GetString("ConfigMenuiFruitStolen"));
            AddMenuConfigCheckbox(submenuiFruit, "iFruit", "CaniFruitPlate", Config.CaniFruitPlate, T.GetString("ConfigMenuiFruitPlate"));
            
            UIMenu submenuNotifications = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemNotify"), _offset);
            AddMenuConfigCheckbox(submenuNotifications, "Check", "CheckForUpdate", Config.CheckForUpdate, T.GetString("ConfigMenuNotifyUpdate"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowSHVDNNotification", Config.ShowSHVDNNotification, T.GetString("ConfigMenuNotifySHVDN"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowFileNotification", Config.ShowFileNotification, T.GetString("ConfigMenuNotifyFile"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowVisualCNotification", Config.ShowSHVDNNotification, T.GetString("ConfigMenuNotifyVisualC"));
            AddMenuConfigCheckbox(submenuNotifications, "Check", "ShowNETFrameworkNotification", Config.ShowSHVDNNotification, T.GetString("ConfigMenuNotifyNET"));
            submenuNotifications.AddItem(new UIMenuItem(T.GetString("ConfigMenuNotifyReboot")) { Enabled = false } );

            UIMenu submenuInsurance = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemInsurance"), _offset);
            AddMenuConfigList(submenuInsurance, "Insurance", "InsuranceCostMultiplier", Config.InsuranceMult, GetCostMultiplierDescription("InsuranceCostMultiplier"), 0f, 100f, 0.1f);
            AddMenuConfigList(submenuInsurance, "Insurance", "RecoverCostMultiplier", Config.RecoverMult, GetCostMultiplierDescription("RecoverCostMultiplier"), 0f, 100f, 0.1f);
            AddMenuConfigList(submenuInsurance, "Insurance", "StolenCostMultiplier", Config.StolenMult, GetCostMultiplierDescription("StolenCostMultiplier"), 0f, 100f, 0.1f);

            UIMenu submenuBringVehicle = AddSubMenu(_menuPool, _mainMenu, _menuTitle, T.GetString("ConfigMenuItemBringVehicle"), _offset);
            AddMenuConfigList(submenuBringVehicle, "BringVehicle", "BringVehicleBasePrice", Config.BringVehicleBasePrice, T.GetString("ConfigMenuBringVehiclePrice"), 0, 2000, 50);
            AddMenuConfigBringVehicleInstant(submenuBringVehicle, "BringVehicle", "BringVehicleInstant", Config.BringVehicleInstant, "");
            AddMenuConfigList(submenuBringVehicle, "BringVehicle", "BringVehicleRadius", Config.BringVehicleRadius, T.GetString("ConfigMenuBringVehicleRadius"), 10, 2000, 5);
            AddMenuConfigList(submenuBringVehicle, "BringVehicle", "BringVehicleTimeout", Config.BringVehicleTimeout, T.GetString("ConfigMenuBringVehicleTimeout"), 1, 30, 1);
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
                    Config.Settings.SetValue("General", key, item.Items[index].ToString());
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
                    Config.UpdateValue(key, item.Checked);
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
                    Config.Settings.SetValue(section, key, ((float)item.Items[index]).ToString().ToString().Replace(",", "."));
                    Config.UpdateValue(key, (float)item.Items[index]);
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
                    Config.Settings.SetValue(section, key, (int)item.Items[index]);
                    Config.UpdateValue(key, (int)item.Items[index]);
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
                    Config.UpdateValue(key, item.Checked);
                    Config.Settings.Save();
                }
            };
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
