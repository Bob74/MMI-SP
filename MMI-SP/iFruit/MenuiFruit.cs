using System.Collections.Generic;

using NativeUI;
using GTA;
using GTA.Native;

namespace MMI_SP.iFruit
{
    using T = Translator;

    class MenuiFruit
    {
        private MenuPool _menuPool;
        internal void MenuPoolProcessMenus() { _menuPool.ProcessMenus(); }

        private UIMenu _mainMenu = new UIMenu("", "Menu");
        internal UIMenu GetMainmenu() { return _mainMenu; }

        private string _banner = "scripts\\MMI\\banner.png";

        // We need to use the same InsuranceManager in the whole plugin or we will have issues with its variables
        // (ie: _recoveredVehList would never be checked since it would be in another instance of MMI)
        private InsuranceManager _insurance = InsuranceManager.GetCurrentInstance();
        private InsuranceObserver _observer = InsuranceObserver.GetCurrentInstance();

        // Sub menus
        UIMenu _submenuBring;

        //Main Base
        public MenuiFruit()
        {
            if (System.IO.File.Exists(_banner)) _mainMenu.SetBannerType(_banner);
            _menuPool = new MenuPool();
            _menuPool.Add(_mainMenu);
        }

        internal void Show()
        {
            _mainMenu.Visible = true;
            Function.Call(Hash._0xFC695459D4D0E219, 0.5f, 0.5f);    // Cursor position centered
        }


        /// <summary>
        /// Creates the menu
        /// </summary>
        internal void Create()
        {
            MenuRecover(_mainMenu);
            CreateMenuBring(_mainMenu);
            _menuPool.RefreshIndex();
        }
        /// <summary>
        /// Remove everything in the menu and recreates it.
        /// Allow us to have dynamic menu creation.
        /// </summary>
        internal void Reset()
        {
            _mainMenu.MenuItems.Clear();
            Create();
        }

        
        /// <summary>
        /// Recover a detroyed vehicle.
        /// </summary>
        /// <param name="menu"></param>
        private void MenuRecover(UIMenu menu)
        {
            UIMenu submenuRecover = _menuPool.AddSubMenu(menu, T.GetString("RecoverVehicle"), T.GetString("RecoverVehicleDesc"));
            if (System.IO.File.Exists(_banner)) submenuRecover.SetBannerType(_banner);

            List<string> deadVehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true);
            if (deadVehicleList.Count > 0)
            {
                foreach (string vehID in deadVehicleList)
                {
                    int cost = _insurance.GetVehicleInsuranceCost(vehID, InsuranceManager.Multiplier.Recover);
                    UIMenuItem recoverVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false) + " (" + cost + "$)", T.GetString("NotifyDeliverVehicle"));
                    submenuRecover.AddItem(recoverVehicle);

                    submenuRecover.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == recoverVehicle)
                        {
                            if (SE.Player.AddCashToPlayer(-1 * cost))
                            {
                                MMISound.Play(MMISound.SoundFamily.Okay);
                                _insurance.RecoverVehicle(vehID);
                                UI.Notify(T.GetString("NotifyDeliverVehicle"));
                                recoverVehicle.Enabled = false;

                                // Rebuild Bring menu
                                BuildMenuBring();
                            }
                            else
                            {
                                MMISound.Play(MMISound.SoundFamily.NoMoney);
                                UI.Notify(T.GetString("NotifyNoMoney"));
                            }
                        }
                    };
                }
            }
            else
            {
                UIMenuItem recoverVehicle = new UIMenuItem(T.GetString("Empty"), T.GetString("RecoverVehicleItemEmptyDesc"));
                recoverVehicle.Enabled = false;
                submenuRecover.AddItem(recoverVehicle);
            }

        }

        /// <summary>
        /// Bring the vehicle to the player
        /// </summary>
        /// <param name="menu"></param>
        private void CreateMenuBring(UIMenu menu)
        {
            _submenuBring = _menuPool.AddSubMenu(menu, T.GetString("BringVehicle"), T.GetString("BringVehicleDesc"));
            if (System.IO.File.Exists(_banner)) _submenuBring.SetBannerType(_banner);
            BuildMenuBring();
        }
        private void BuildMenuBring()
        {
            if (_submenuBring.MenuItems.Count > 0)
            {
                for (int i = _submenuBring.MenuItems.Count - 1; i >= 0; i--)
                    _submenuBring.RemoveItemAt(i);
            }

            if (InsuranceObserver.GetBringableVehicles().Count > 0)
            {
                foreach (Vehicle veh in InsuranceObserver.GetBringableVehicles())
                {
                    string vehID = Tools.GetVehicleIdentifier(veh);
                    int cost = (int)((Game.Player.Character.Position.DistanceTo(veh.Position) / 1000) * InsuranceManager.BringVehicleBasePrice);
                    UIMenuItem bringVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false) + " (" + cost + "$)", T.GetString("BringVehicleDesc"));
                    _submenuBring.AddItem(bringVehicle);

                    _submenuBring.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == bringVehicle)
                        {
                            if (SE.Player.AddCashToPlayer(-1 * cost))
                            {
                                MMISound.Play(MMISound.SoundFamily.Okay);
                                _observer.BringVehicleToPlayer(veh, cost, InsuranceManager.BringVehicleInstant);
                                bringVehicle.Enabled = false;
                                UI.Notify(T.GetString("NotifyBringVehicle"));
                            }
                            else
                            {
                                MMISound.Play(MMISound.SoundFamily.NoMoney);
                                UI.Notify(T.GetString("NotifyNoMoney"));
                            }
                        }
                    };
                }
            }
            else
            {
                UIMenuItem bringVehicle = new UIMenuItem(T.GetString("Empty"), T.GetString("BringVehicleItemEmptyDesc"));
                bringVehicle.Enabled = false;
                _submenuBring.AddItem(bringVehicle);
            }
        }

    }
}

