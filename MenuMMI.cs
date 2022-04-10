using System.Collections.Generic;

using NativeUI;
using GTA;
using GTA.Native;

using MMI_SP.iFruit;

namespace MMI_SP
{
    using T = Translator;

    class MenuMMI
    {
        private MenuPool _menuPool;
        internal void MenuPoolProcessMenus() { _menuPool.ProcessMenus(); }

        private UIMenu _mainMenu = new UIMenu("", "Menu");
        internal UIMenu GetMainmenu() { return _mainMenu; }

        private string _banner = "scripts\\MMI\\banner.png";

        private bool _openedFromiFruit = false;
        public bool OpenedFromiFruit { get => _openedFromiFruit; private set => _openedFromiFruit = value; }

        // We need to use the same InsuranceManager in the whole plugin or we will have issues with its variables
        // (ie: _recovereddVehList would never be checked since it would be in another instance of MMI)
        private InsuranceManager _insurance = InsuranceManager.GetCurrentInstance();
        private InsuranceObserver _observer = InsuranceObserver.GetCurrentInstance();

        // Sub menus
        UIMenuItem _itemInsure;
        UIMenu _submenuRecover;
        UIMenu _submenuStolen;
        UIMenu _submenuCancel;
        UIMenu _submenuPlate;
        UIMenu _submenuBring;

        //Main Base
        public MenuMMI()
        {
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
            if (System.IO.File.Exists(_banner)) _mainMenu.SetBannerType(_banner);

            if (OpenedFromiFruit)
            {
                if (iFruitMMI.CaniFruitInsure) BuildItemInsure();
                if (iFruitMMI.CaniFruitCancel) CreateMenuCancel(_mainMenu);
                if (iFruitMMI.CaniFruitRecover) CreateMenuRecover(_mainMenu);
                if (iFruitMMI.CaniFruitStolen) CreateMenuStolen(_mainMenu);
                if (iFruitMMI.CaniFruitPlate) CreateMenuPlate(_mainMenu);
                CreateMenuBring(_mainMenu);
            }
            else
            {
                BuildItemInsure();
                CreateMenuCancel(_mainMenu);
                CreateMenuRecover(_mainMenu);
                CreateMenuStolen(_mainMenu);
                CreateMenuPlate(_mainMenu);
            }

            _menuPool.RefreshIndex();
        }
        /// <summary>
        /// Remove everything in the menu and recreates it.
        /// Allow us to have dynamic menu creation.
        /// </summary>
        internal void Reset(bool iFruit = false)
        {
            OpenedFromiFruit = iFruit;
            if (_mainMenu != null) _mainMenu.MenuItems.Clear();
            Create();
        }

        /// <summary>
        /// Set the menu index to the first item.
        /// </summary>
        /// <param name="menu"></param>
        private void RefreshMenuIndex(UIMenu menu, string itemDescription)
        {
            if (menu != null)
            {
                if (menu.MenuItems.Count <= 0)
                {
                    UIMenuItem cancelContract = new UIMenuItem(T.GetString("Empty"), itemDescription);
                    cancelContract.Enabled = false;
                    menu.AddItem(cancelContract);

                    menu.CurrentSelection = 0;
                }
                else
                {
                    if (menu.CurrentSelection > menu.MenuItems.Count - 1)
                        menu.CurrentSelection = 0;
                }
                
                menu.UpdateScaleform();
            }
        }

        /// <summary>
        /// Insure a vehicle by adding it to the database.
        /// </summary>
        private void BuildItemInsure()
        {
            Vehicle veh = Game.Player.LastVehicle;
            if (veh.Exists())
            {
                int cost = InsuranceManager.GetVehicleInsuranceCost(veh);

                if (!InsuranceManager.IsVehicleInsured(Tools.GetVehicleIdentifier(veh)))
                {
                    if (InsuranceManager.IsVehicleInsurable(veh))
                    {
                        _itemInsure = new UIMenuItem(T.GetString("InsureVehicle"), T.GetString("InsureVehicleDesc") + "\n" + SE.Vehicle.GetVehicleFriendlyName(veh, false) + ".");
                        _itemInsure.SetRightLabel(cost + "$");
                        _mainMenu.AddItem(_itemInsure);
                    }
                    else
                    {
                        _itemInsure = new UIMenuItem(T.GetString("InsureVehicle"), T.GetString("VehicleWrongType") + " " + SE.Vehicle.GetVehicleFriendlyName(veh) + ".");
                        _itemInsure.Enabled = false;
                        _mainMenu.AddItem(_itemInsure);
                    }
                }
                else
                {
                    _itemInsure = new UIMenuItem(T.GetString("InsureVehicle"), T.GetString("VehicleAlreadyInsured") + "\n" + SE.Vehicle.GetVehicleFriendlyName(veh, false) + ".");
                    _itemInsure.Enabled = false;
                    _mainMenu.AddItem(_itemInsure);
                }

                _mainMenu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == _itemInsure)
                    {
                        if (Game.Player.LastVehicle != null)
                        {
                            if (Game.Player.LastVehicle.Exists())
                            {
                                if (!InsuranceManager.IsVehicleInsured(Tools.GetVehicleIdentifier(Game.Player.LastVehicle)))
                                {
                                    if (InsuranceManager.IsVehicleInsurable(Game.Player.LastVehicle))
                                    {
                                        if (SE.Player.AddCashToPlayer(-1 * cost))
                                            InsureVehicle(Game.Player.LastVehicle); // IMPORTANT: if we use "veh", it will always use the same vehicle in the function!
                                        else
                                        {
                                            if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                            UI.Notify(T.GetString("NotifyNoMoney"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }
        /// <summary>
        /// Update the item's text according to the insured vehicle status.
        /// </summary>
        private void RefreshItemInsure()
        {
            if (_itemInsure != null)
            {
                Vehicle veh = Game.Player.LastVehicle;
                if (veh != null)
                {
                    if (veh.Exists())
                    {
                        if (!InsuranceManager.IsVehicleInsured(Tools.GetVehicleIdentifier(veh)))
                        {
                            if (InsuranceManager.IsVehicleInsurable(veh))
                            {
                                int cost = InsuranceManager.GetVehicleInsuranceCost(veh);
                                _itemInsure.Text = T.GetString("InsureVehicle");
                                _itemInsure.SetRightLabel(cost + "$");
                                _itemInsure.Description = T.GetString("InsureVehicleDesc") + "\n" + SE.Vehicle.GetVehicleFriendlyName(veh, false) + ".";
                                _itemInsure.Enabled = true;
                            }
                        }
                        else
                        {
                            _itemInsure.Text = T.GetString("InsureVehicle");
                            _itemInsure.Description = T.GetString("VehicleAlreadyInsured") + "\n" + SE.Vehicle.GetVehicleFriendlyName(veh, false) + ".";
                            _itemInsure.Enabled = false;
                        }
                    }
                    else
                    {
                        _mainMenu.RemoveItemAt(0);
                    }
                }
                else
                {
                    _mainMenu.RemoveItemAt(0);
                }
            }
        }
        private void InsureVehicle(Vehicle veh)
        {
            if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
            _insurance.InsureVehicle(veh);
            UI.Notify(T.GetString("NotifyVehicleIsInsured"));
            _itemInsure.Enabled = false;

            // Updates
            RefreshMenuIndex(_submenuCancel, T.GetString("CancelInsuranceItemEmptyDesc"));
            RebuildMenuCancel();

            RefreshMenuIndex(_submenuStolen, T.GetString("StolenVehicleItemEmptyDesc"));
            RebuildMenuStolen();

            if (OpenedFromiFruit)
            {
                RefreshMenuIndex(_submenuBring, T.GetString("BringVehicleItemEmptyDesc"));
                RebuildMenuBring();
            }

            RefreshMenuIndex(_submenuPlate, T.GetString("PlateChangeItemEmptyDesc"));
            RebuildMenuPlate();
        }


        /// <summary>
        /// Cancel a contract by removing the vehicle from the database.
        /// </summary>
        /// <param name="menu"></param>
        private void CreateMenuCancel(UIMenu menu)
        {
            _submenuCancel = _menuPool.AddSubMenu(menu, T.GetString("CancelInsurance"), T.GetString("CancelInsuranceDesc"));
            if (System.IO.File.Exists(_banner)) _submenuCancel.SetBannerType(_banner);
            RebuildMenuCancel();
        }
        private void RebuildMenuCancel()
        {
            _submenuCancel.Clear();

            List<string> vehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), false);
            vehicleList.AddRange(_insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true));

            if (vehicleList.Count > 0)
            {
                foreach (string vehID in vehicleList)
                {
                    UIMenuItem cancelContract = new UIMenuItem(_insurance.GetVehicleModelName(vehID), T.GetString("CancelInsuranceItemDesc"));
                    cancelContract.SetRightLabel(_insurance.GetVehicleLicensePlate(vehID));
                    _submenuCancel.AddItem(cancelContract);

                    _submenuCancel.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == cancelContract)
                        {
                            if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                            _insurance.CancelVehicle(vehID);
                            UI.Notify(T.GetString("NotifyCanceled"));
                            cancelContract.Enabled = false;

                            _submenuCancel.RemoveItemAt(index);

                            // Updates
                            RefreshMenuIndex(_submenuCancel, T.GetString("CancelInsuranceItemEmptyDesc"));

                            RefreshItemInsure();

                            RefreshMenuIndex(_submenuRecover, T.GetString("RecoverVehicleItemEmptyDesc"));
                            RebuildMenuRecover();

                            RefreshMenuIndex(_submenuStolen, T.GetString("StolenVehicleItemEmptyDesc"));
                            RebuildMenuStolen();

                            RefreshMenuIndex(_submenuPlate, T.GetString("PlateChangeItemEmptyDesc"));
                            RebuildMenuPlate();

                            if (OpenedFromiFruit)
                            {
                                RefreshMenuIndex(_submenuBring, T.GetString("BringVehicleItemEmptyDesc"));
                                RebuildMenuBring();
                            }
                        }
                    };
                }
            }
            else
            {
                UIMenuItem cancelContract = new UIMenuItem(T.GetString("Empty"), T.GetString("CancelInsuranceItemEmptyDesc"));
                cancelContract.Enabled = false;
                _submenuCancel.AddItem(cancelContract);
            }
        }


        /// <summary>
        /// Recover a detroyed vehicle.
        /// </summary>
        /// <param name="menu"></param>
        private void CreateMenuRecover(UIMenu menu)
        {
            _submenuRecover = _menuPool.AddSubMenu(menu, T.GetString("RecoverVehicle"), T.GetString("RecoverVehicleDesc"));
            if (System.IO.File.Exists(_banner)) _submenuRecover.SetBannerType(_banner);
            RebuildMenuRecover();
        }
        private void RebuildMenuRecover()
        {
            _submenuRecover.Clear();

            List<string> deadVehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true);
            if (deadVehicleList.Count > 0)
            {
                foreach (string vehID in deadVehicleList)
                {
                    int cost = _insurance.GetVehicleInsuranceCost(vehID, InsuranceManager.Multiplier.Recover);
                    UIMenuItem recoverVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false), T.GetString("NotifyDeliverVehicle"));
                    recoverVehicle.SetRightLabel(cost + "$");
                    _submenuRecover.AddItem(recoverVehicle);

                    _submenuRecover.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == recoverVehicle)
                        {
                            if (SE.Player.AddCashToPlayer(-1 * cost))
                            {
                                if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                _insurance.RecoverVehicle(vehID);
                                UI.Notify(T.GetString("NotifyDeliverVehicle"));
                                recoverVehicle.Enabled = false;

                                _submenuRecover.RemoveItemAt(index);

                                // Updates
                                RefreshMenuIndex(_submenuRecover, T.GetString("RecoverVehicleItemEmptyDesc"));
                                RebuildMenuStolen();

                                if (OpenedFromiFruit) RebuildMenuBring();
                            }
                            else
                            {
                                if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
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
                _submenuRecover.AddItem(recoverVehicle);
            }
        }


        /// <summary>
        /// Recover a "stolen" vehicle (vehicle that vanished).
        /// </summary>
        /// <param name="menu"></param>
        private void CreateMenuStolen(UIMenu menu)
        {
            _submenuStolen = _menuPool.AddSubMenu(menu, T.GetString("StolenVehicle"), T.GetString("StolenVehicleDesc"));
            if (System.IO.File.Exists(_banner)) _submenuStolen.SetBannerType(_banner);
            RebuildMenuStolen();
        }
        private void RebuildMenuStolen()
        {
            _submenuStolen.Clear();

            List<string> aliveVehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), false);
            if (aliveVehicleList.Count > 0)
            {
                foreach (string vehID in aliveVehicleList)
                {
                    int cost = _insurance.GetVehicleInsuranceCost(vehID, InsuranceManager.Multiplier.Stolen);
                    UIMenuItem stolenVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false), T.GetString("NotifyDeliverVehicle"));
                    stolenVehicle.SetRightLabel(cost + "$");
                    _submenuStolen.AddItem(stolenVehicle);

                    _submenuStolen.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == stolenVehicle)
                        {
                            if (SE.Player.AddCashToPlayer(-1 * cost))
                            {
                                if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);

                                // Remove the vehicle from the world to avoid vehicle duplication
                                foreach (Vehicle veh in World.GetAllVehicles())
                                {
                                    if (Tools.GetVehicleIdentifier(veh) == vehID)
                                    {
                                        if (veh.CurrentBlip != null) veh.CurrentBlip.Remove();
                                        veh.Delete();
                                    }
                                }

                                _insurance.RecoverVehicle(vehID);

                                UI.Notify(T.GetString("NotifyDeliverVehicle"));
                                stolenVehicle.Enabled = false;

                                _submenuStolen.RemoveItemAt(index);

                                // Updates
                                RefreshMenuIndex(_submenuStolen, T.GetString("StolenVehicleItemEmptyDesc"));

                                if (OpenedFromiFruit)
                                {
                                    RefreshMenuIndex(_submenuBring, T.GetString("BringVehicleItemEmptyDesc"));
                                    RebuildMenuBring();
                                }
                            }
                            else
                            {
                                if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                UI.Notify(T.GetString("NotifyNoMoney"));
                            }
                        }
                    };
                }
            }
            else
            {
                UIMenuItem stolenVehicle = new UIMenuItem(T.GetString("Empty"), T.GetString("StolenVehicleItemEmptyDesc"));
                stolenVehicle.Enabled = false;
                _submenuStolen.AddItem(stolenVehicle);
            }
        }


        /// <summary>
        /// Allow us to edit the license plate number of our vehicles.
        /// </summary>
        /// <param name="menu"></param>
        private void CreateMenuPlate(UIMenu menu)
        {
            _submenuPlate = _menuPool.AddSubMenu(menu, T.GetString("PlateChange"), T.GetString("PlateChangeDesc"));
            if (System.IO.File.Exists(_banner)) _submenuPlate.SetBannerType(_banner);
            RebuildMenuPlate();
        }
        private void RebuildMenuPlate()
        {
            int price = 1000;

            _submenuPlate.Clear();

            List<string> vehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), false);
            vehicleList.AddRange(_insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true));

            if (vehicleList.Count > 0)
            {
                foreach (string vehID in vehicleList)
                {
                    UIMenuItem changePlate = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false));
                    changePlate.SetRightLabel(price.ToString() + "$");
                    _submenuPlate.AddItem(changePlate);

                    _submenuPlate.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == changePlate)
                        {
                            if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                            string oldVehID = vehID;
                            string oldPlate = _insurance.GetVehicleLicensePlate(vehID);
                            string newPlate = Game.GetUserInput(oldPlate, 7);   // 7 = 8 caractères
                            newPlate = newPlate.PadRight(8);
                            newPlate = newPlate.ToUpperInvariant();

                            if (SE.Vehicle.IsValidPlateNumber(newPlate))
                            {
                                if (newPlate != oldPlate && newPlate != "")
                                {
                                    if (SE.Player.AddCashToPlayer(-1 * price))
                                    {
                                        string newVehID = _insurance.ChangeVehicleLicensePlate(vehID, newPlate);

                                        // Refresh item text
                                        item.Text = _insurance.GetVehicleFriendlyName(newVehID, false);

                                        // Update in game vehicle
                                        for (int i = InsuranceObserver.InsuredVehList.Count - 1; i >= 0; i--)
                                        {
                                            if (Tools.GetVehicleIdentifier(InsuranceObserver.InsuredVehList[i]) == vehID)
                                            {
                                                // Update the plate on the in game's vehicles
                                                InsuranceObserver.InsuredVehList[i].NumberPlate = newPlate;

                                                // Remove the previous vehicle identifiers from the list
                                                InsuranceObserver.InsuredVehList.RemoveAt(i);
                                            }
                                        }

                                        // Update BlipsToRemove dictionnary
                                        if (InsuranceObserver.BlipsToRemove.ContainsKey(oldVehID))
                                        {
                                            Blip vehBlip = InsuranceObserver.BlipsToRemove[oldVehID];
                                            InsuranceObserver.BlipsToRemove.Remove(oldVehID);
                                            InsuranceObserver.BlipsToRemove.Add(newVehID, vehBlip);
                                        }
                   
                                        UI.Notify(T.GetString("NotifyPlateChanged") + "~n~" + "[" + oldPlate + "]" + " => " + "[" + newPlate + "]");
                                        item.Enabled = false;

                                        // Updates
                                        // Need to wait for the vehicle to be detected by the InsuranceObserver's timer
                                        BigMessageThread.MessageInstance.ShowSimpleShard(T.GetString("PlateChangeUpdateDB"), T.GetString("PlateChangeUpdateDBDesc"), _observer.DelayDetectInsuredVehicles + 1000);
                                        Script.Wait(_observer.DelayDetectInsuredVehicles + 1000);

                                        RefreshItemInsure();

                                        RebuildMenuCancel();
                                        RebuildMenuRecover();
                                        RebuildMenuStolen();
                                        if (OpenedFromiFruit) RebuildMenuBring();
                                        RebuildMenuPlate();
                                    }
                                    else
                                    {
                                        if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                        UI.Notify(T.GetString("NotifyNoMoney"));
                                    }
                                }
                            }
                            else
                            {
                                UI.Notify(T.GetString("NotifyWrongPlate"));
                            }
                        }
                    };
                }
            }
            else
            {
                UIMenuItem changePlate = new UIMenuItem(T.GetString("Empty"), T.GetString("PlateChangeItemEmptyDesc"));
                changePlate.Enabled = false;
                _submenuPlate.AddItem(changePlate);
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
            RebuildMenuBring();
        }
        private void RebuildMenuBring()
        {
            _submenuBring.Clear();

            if (InsuranceObserver.GetBringableVehicles().Count > 0)
            {
                foreach (Vehicle veh in InsuranceObserver.GetBringableVehicles())
                {
                    string vehID = Tools.GetVehicleIdentifier(veh);

                    if (SE.Player.GetCurrentCharacterName(true) == _insurance.GetVehicleOwner(vehID))
                    {
                        int cost = (int)((Game.Player.Character.Position.DistanceTo(veh.Position) / 1000) * InsuranceManager.BringVehicleBasePrice);
                        UIMenuItem bringVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false), T.GetString("BringVehicleDesc"));
                        bringVehicle.SetRightLabel(cost + "$");
                        _submenuBring.AddItem(bringVehicle);

                        _submenuBring.OnItemSelect += (sender, item, index) =>
                        {
                            if (item == bringVehicle)
                            {
                                if (SE.Player.AddCashToPlayer(-1 * cost))
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                    _observer.BringVehicleToPlayer(veh, cost, InsuranceManager.BringVehicleInstant);
                                    bringVehicle.Enabled = false;
                                    UI.Notify(T.GetString("NotifyBringVehicle"));

                                    _submenuBring.RemoveItemAt(index);

                                    // Updates
                                    RefreshMenuIndex(_submenuBring, T.GetString("BringVehicleItemEmptyDesc"));
                                }
                                else
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                    UI.Notify(T.GetString("NotifyNoMoney"));
                                }
                            }
                        };
                    }
                }
            }
            else
            {
                UIMenuItem bringVehicle = new UIMenuItem(T.GetString("Empty"), T.GetString("BringVehicleItemEmptyDesc"));
                bringVehicle.Enabled = false;
                _submenuBring.AddItem(bringVehicle);
            }
        }










            /*
            private void OnMenuClose(UIMenu sender)
            {
                if (sender == _submenuPlate)
                {
                    if (_submenuRecover != null)
                        BuildMenuRecover();

                    if (_submenuCancel != null)
                        BuildMenuCancel();

                    if (_submenuStolen != null)
                        BuildMenuStolen();

                    BuildMenuPlate();
                }
                else if (sender == _submenuCancel)
                {
                    if (_itemInsure != null)
                    {
                        Vehicle playerVehicle = Game.Player.LastVehicle;
                        if (playerVehicle != null)
                            if (playerVehicle.Exists())
                            {
                                if (!InsuranceManager.IsVehicleInsured(Tools.GetVehicleIdentifier(playerVehicle)))
                                {
                                    if (InsuranceManager.IsVehicleInsurable(playerVehicle))
                                    {
                                        int cost = InsuranceManager.GetVehicleInsuranceCost(playerVehicle);
                                        _itemInsure.Text = T.GetString("InsureVehicle") + " (" + cost + "$)";
                                        _itemInsure.Description = T.GetString("InsureVehicleDesc") + "\n" + SE.Vehicle.GetVehicleFriendlyName(playerVehicle, false) + ".";
                                        _itemInsure.Enabled = true;
                                    }
                                }
                            }
                    }

                    if (_submenuStolen != null)
                        if (_submenuStolen.MenuItems.Count > 0) _submenuStolen.CurrentSelection = 0;
                            _submenuStolen.UpdateScaleform();
                }
            }

            /// <summary>
            /// Insure a vehicle by adding it to the database.
            /// </summary>
            private void CreateItemInsure()
            {
                Vehicle playerVehicle = Game.Player.LastVehicle;
                if (playerVehicle.Exists())
                    BuildItemInsure(playerVehicle);
            }
            private void BuildItemInsure(Vehicle veh)
            {
                if (!InsuranceManager.IsVehicleInsured(Tools.GetVehicleIdentifier(veh)))
                {
                    if (InsuranceManager.IsVehicleInsurable(veh))
                    {
                        int cost = InsuranceManager.GetVehicleInsuranceCost(veh);
                        _itemInsure = new UIMenuItem(T.GetString("InsureVehicle") + " (" + cost + "$)", T.GetString("InsureVehicleDesc") + "\n" + SE.Vehicle.GetVehicleFriendlyName(veh, false) + ".");
                        _mainMenu.AddItem(_itemInsure);

                        _mainMenu.OnItemSelect += (sender, item, index) =>
                        {
                            if (item == _itemInsure)
                            {
                                if (SE.Player.AddCashToPlayer(-1 * cost))
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                    _insurance.InsureVehicle(veh);
                                    UI.Notify(T.GetString("NotifyVehicleIsInsured"));
                                    _itemInsure.Enabled = false;

                                    // Rebuild impacted menus
                                    if (OpenedFromiFruit)
                                    {
                                        if (iFruitMMI.CaniFruitCancel) BuildMenuCancel();
                                        if (iFruitMMI.CaniFruitStolen) BuildMenuStolen();
                                        if (iFruitMMI.CaniFruitPlate) BuildMenuPlate();
                                        BuildMenuBring();
                                    }
                                    else
                                    {
                                        BuildMenuCancel();
                                        BuildMenuStolen();
                                        BuildMenuPlate();
                                    }
                                }
                                else
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                    UI.Notify(T.GetString("NotifyNoMoney"));
                                }
                            }
                        };
                    }
                    else
                    {
                        UIMenuItem insureVehicle = new UIMenuItem(T.GetString("InsureVehicle"), T.GetString("VehicleWrongType") + " " + SE.Vehicle.GetVehicleFriendlyName(veh) + ".");
                        insureVehicle.Enabled = false;
                        _mainMenu.AddItem(insureVehicle);
                    }
                }
                else
                {
                    UIMenuItem insureVehicle = new UIMenuItem(T.GetString("InsureVehicle"), T.GetString("VehicleAlreadyInsured") + "\n" + SE.Vehicle.GetVehicleFriendlyName(veh, false) + ".");
                    insureVehicle.Enabled = false;
                    _mainMenu.AddItem(insureVehicle);
                }
            }

            /// <summary>
            /// Cancel a contract by removing the vehicle from the database.
            /// </summary>
            /// <param name="menu"></param>
            private void CreateMenuCancel(UIMenu menu)
            {
                _submenuCancel = _menuPool.AddSubMenu(menu, T.GetString("CancelInsurance"), T.GetString("CancelInsuranceDesc"));
                if (System.IO.File.Exists(_banner)) _submenuCancel.SetBannerType(_banner);
                BuildMenuCancel();

                _submenuCancel.OnMenuClose += OnMenuClose;
            }
            private void BuildMenuCancel()
            {
                _submenuCancel.Clear();

                List<string> vehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), false);
                vehicleList.AddRange(_insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true));

                if (vehicleList.Count > 0)
                {
                    foreach (string vehID in vehicleList)
                    {
                        UIMenuItem cancelContract = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false), T.GetString("CancelInsuranceItemDesc"));
                        _submenuCancel.AddItem(cancelContract);

                        _submenuCancel.OnItemSelect += (sender, item, index) =>
                        {
                            if (item == cancelContract)
                            {
                                if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                _insurance.CancelVehicle(vehID);
                                UI.Notify(T.GetString("NotifyCanceled"));
                                cancelContract.Enabled = false;

                                // Rebuild impacted menus
                                if (OpenedFromiFruit)
                                {
                                    if (iFruitMMI.CaniFruitRecover) BuildMenuRecover();
                                    if (iFruitMMI.CaniFruitStolen) BuildMenuStolen();
                                    if (iFruitMMI.CaniFruitPlate) BuildMenuPlate();
                                    BuildMenuBring();
                                }
                                else
                                {
                                    BuildMenuRecover();
                                    BuildMenuStolen();
                                    BuildMenuPlate();
                                }

                                _submenuCancel.RemoveItemAt(index);
                                if (_submenuCancel.MenuItems.Count > 0) _submenuCancel.CurrentSelection = 0;
                                _submenuCancel.UpdateScaleform();
                            }
                        };
                    }

                    if (_submenuCancel.MenuItems.Count > 0) _submenuCancel.CurrentSelection = 0;
                    _submenuCancel.UpdateScaleform();
                }
                else
                {
                    UIMenuItem cancelContract = new UIMenuItem(T.GetString("Empty"), T.GetString("CancelInsuranceItemEmptyDesc"));
                    cancelContract.Enabled = false;
                    _submenuCancel.AddItem(cancelContract);
                }
            }

            /// <summary>
            /// Recover a detroyed vehicle.
            /// </summary>
            /// <param name="menu"></param>
            private void CreateMenuRecover(UIMenu menu)
            {
                _submenuRecover = _menuPool.AddSubMenu(menu, T.GetString("RecoverVehicle"), T.GetString("RecoverVehicleDesc"));
                if (System.IO.File.Exists(_banner)) _submenuRecover.SetBannerType(_banner);
                BuildMenuRecover();
            }
            private void BuildMenuRecover()
            {
                _submenuRecover.Clear();

                List<string> deadVehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true);
                if (deadVehicleList.Count > 0)
                {
                    foreach (string vehID in deadVehicleList)
                    {
                        int cost = _insurance.GetVehicleInsuranceCost(vehID, InsuranceManager.Multiplier.Recover);
                        UIMenuItem recoverVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false) + " (" + cost + "$)", T.GetString("NotifyDeliverVehicle"));
                        _submenuRecover.AddItem(recoverVehicle);

                        _submenuRecover.OnItemSelect += (sender, item, index) =>
                        {
                            if (item == recoverVehicle)
                            {
                                if (SE.Player.AddCashToPlayer(-1 * cost))
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                    _insurance.RecoverVehicle(vehID);
                                    UI.Notify(T.GetString("NotifyDeliverVehicle"));
                                    recoverVehicle.Enabled = false;

                                    // Rebuild impacted menus
                                    if (OpenedFromiFruit)
                                    {
                                        if (iFruitMMI.CaniFruitStolen) BuildMenuStolen();
                                        BuildMenuBring();
                                    }
                                    else
                                        BuildMenuStolen();

                                }
                                else
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
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
                    _submenuRecover.AddItem(recoverVehicle);
                }
            }

            /// <summary>
            /// Recover a "stolen" vehicle (vehicle that vanished).
            /// </summary>
            /// <param name="menu"></param>
            private void CreateMenuStolen(UIMenu menu)
            {
                _submenuStolen = _menuPool.AddSubMenu(menu, T.GetString("StolenVehicle"), T.GetString("StolenVehicleDesc"));
                if (System.IO.File.Exists(_banner)) _submenuStolen.SetBannerType(_banner);
                BuildMenuStolen();
            }
            private void BuildMenuStolen()
            {
                _submenuStolen.Clear();

                List<string> aliveVehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), false);
                if (aliveVehicleList.Count > 0)
                {
                    foreach (string vehID in aliveVehicleList)
                    {
                        int cost = _insurance.GetVehicleInsuranceCost(vehID, InsuranceManager.Multiplier.Stolen);
                        UIMenuItem recoverVehicle = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false) + " (" + cost + "$)", T.GetString("NotifyDeliverVehicle"));
                        _submenuStolen.AddItem(recoverVehicle);

                        _submenuStolen.OnItemSelect += (sender, item, index) =>
                        {
                            if (item == recoverVehicle)
                            {
                                if (SE.Player.AddCashToPlayer(-1 * cost))
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                    _insurance.RecoverVehicle(vehID);

                                    // Remove the vehicle from the world to avoid vehicle duplication
                                    foreach (Vehicle veh in World.GetAllVehicles())
                                    {
                                        if (Tools.GetVehicleIdentifier(veh) == vehID)
                                            veh.Delete();
                                    }

                                    UI.Notify(T.GetString("NotifyDeliverVehicle"));
                                    recoverVehicle.Enabled = false;

                                    // Rebuild Bring menu
                                    if (OpenedFromiFruit) BuildMenuBring();
                                }
                                else
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                    UI.Notify(T.GetString("NotifyNoMoney"));
                                }
                            }
                        };
                    }

                    if (_submenuStolen.MenuItems.Count > 0) _submenuStolen.CurrentSelection = 0;
                    _submenuStolen.UpdateScaleform();
                }
                else
                {
                    UIMenuItem recoverVehicle = new UIMenuItem(T.GetString("Empty"), T.GetString("StolenVehicleItemEmptyDesc"));
                    recoverVehicle.Enabled = false;
                    _submenuStolen.AddItem(recoverVehicle);
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
                _submenuBring.Clear();

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
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                    _observer.BringVehicleToPlayer(veh, cost, InsuranceManager.BringVehicleInstant);
                                    bringVehicle.Enabled = false;
                                    UI.Notify(T.GetString("NotifyBringVehicle"));
                                }
                                else
                                {
                                    if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
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


            private void CreateMenuPlate(UIMenu menu)
            {
                _submenuPlate = _menuPool.AddSubMenu(menu, T.GetString("PlateChange"), T.GetString("PlateChangeDesc"));
                if (System.IO.File.Exists(_banner)) _submenuPlate.SetBannerType(_banner);
                BuildMenuPlate();

                _submenuPlate.OnMenuClose += OnMenuClose;
            }
            private void BuildMenuPlate()
            {
                _submenuPlate.Clear();

                List<string> vehicleList = _insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), false);
                vehicleList.AddRange(_insurance.GetInsuredVehicles(SE.Player.GetCurrentCharacterName(true), true));

                if (vehicleList.Count > 0)
                {
                    foreach (string vehID in vehicleList)
                    {
                        UIMenuItem changePlate = new UIMenuItem(_insurance.GetVehicleFriendlyName(vehID, false));
                        _submenuPlate.AddItem(changePlate);

                        _submenuPlate.OnItemSelect += (sender, item, index) =>
                        {
                            if (item == changePlate)
                            {
                                if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.Okay);
                                string oldPlate = _insurance.GetVehicleLicensePlate(vehID);
                                string newPlate = Game.GetUserInput(oldPlate, 7);   // 7 = 8 caractères

                                if (newPlate != oldPlate && newPlate != "")
                                {
                                    if (SE.Player.AddCashToPlayer(1000))
                                    {
                                        string newVehID = _insurance.ChangeVehicleLicensePlate(vehID, newPlate);

                                        // Refresh item text
                                        item.Text = _insurance.GetVehicleFriendlyName(newVehID, false);

                                        for (int i = InsuranceObserver.InsuredVehList.Count - 1; i >= 0; i--)
                                        {
                                            if (Tools.GetVehicleIdentifier(InsuranceObserver.InsuredVehList[i]) == vehID)
                                            {
                                                // Update the plate on the in game's vehicles
                                                InsuranceObserver.InsuredVehList[i].NumberPlate = newPlate;

                                                // Remove the previous vehicle identifiers from the list
                                                InsuranceObserver.InsuredVehList.RemoveAt(i);
                                            }
                                        }

                                        UI.Notify(T.GetString("NotifyPlateChanged") + oldPlate + " => " + newPlate);
                                        item.Enabled = false;
                                    }
                                    else
                                    {
                                        if (OpenedFromiFruit) MMISound.Play(MMISound.SoundFamily.NoMoney);
                                        UI.Notify(T.GetString("NotifyNoMoney"));
                                    }
                                }
                            }
                        };
                    }
                }
                else
                {
                    UIMenuItem changePlate = new UIMenuItem(T.GetString("Empty"), T.GetString("PlateChangeItemEmptyDesc"));
                    changePlate.Enabled = false;
                    _submenuBring.AddItem(changePlate);
                }
            }
            */
        }
}
