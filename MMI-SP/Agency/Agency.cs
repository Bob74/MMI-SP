using System;

using GTA;
using GTA.Native;
using GTA.Math;

namespace MMI_SP.Agency
{
    using T = Translator;

    class Agency : Script
    {
        // Position of the MMI Agency
        private Blip _agencyBlip;
        private static Vector3 _position = new Vector3(-825.7242f, -261.2752f, 37.0000f);
        public static Vector3 Position { get => _position; }

        internal Vector3 GetPosition() { return _position; }
        
        // Menus
        private MenuMMI _menuMMI = null;
        internal MenuMMI GetMenu() { return _menuMMI; }

        // Agency office setup
        private Office _office;

        private bool _isPlayerInCutscene = false;
        private static Vector3 _officePlayerPos = new Vector3(120.0f, -620.50f, 206.35f);
        public static Vector3 OfficePlayerPos { get => _officePlayerPos; }

        private TimeSpan _officeLastCreation = new TimeSpan(0);
        private ItemsManager.OfficeItemsCollection _officeLastItemsCollection = new ItemsManager.OfficeItemsCollection();

        // Timers
        private int _timerRandomSpeech = 0;


        public Agency()
        {
            Tick += Initialize;
        }

        void Initialize(object sender, EventArgs e)
        {
            while (!InsuranceObserver.Initialized)
                Yield();

            _agencyBlip = CreateBlip();
            CreateMenuMMI();

            if (Game.Player.Character.Position.DistanceTo(OfficePlayerPos) <= 2.0f)
                ErrorCancelAgency(false);

            Tick -= Initialize;
            Tick += OnTick;
        }

        // OnTick Event
        void OnTick(object sender, EventArgs e)
        {
            if (_isPlayerInCutscene)
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);

            if (_office != null)
            {
                if (_timerRandomSpeech <= Game.GameTime && _timerRandomSpeech != 0)
                {
                    _office.NpcSay(DialogueManager.SpeechType.OfficeSomething);
                    _timerRandomSpeech = Game.GameTime + new Random(Game.GameTime).Next(10000, 20000); // Next random speech in 5 to 10s
                }
                else if (_timerRandomSpeech == 0)
                {
                    _timerRandomSpeech = Game.GameTime + new Random(Game.GameTime).Next(10000, 20000); // Next random speech in 5 to 10s
                }
                
            }
            else
                _timerRandomSpeech = 0;

            if (_menuMMI != null) _menuMMI.MenuPoolProcessMenus();
            DisplayAgencyThisFrame();
        }

        // Dispose Event
        protected override void Dispose(bool A_0)
        {
            if (A_0)
            {
                if (_office != null)
                {
                    _office.CleanUp();
                    _office = null;
                }
                if (_agencyBlip.Exists())
                    _agencyBlip.Remove();
            }
        }

        /// <summary>
        /// Display and handle the agency main entrance.
        /// </summary>
        private void DisplayAgencyThisFrame()
        {
            if (Game.Player.Character.Position.DistanceTo(_position) < 4.0)
            {
                if (!Game.Player.Character.IsInVehicle())
                    if (Game.Player.WantedLevel > 0)
                        SE.UI.DisplayHelpTextThisFrame(T.GetString("AgencyEntryWanted"));
                    else
                    {
                        SE.UI.DisplayHelpTextThisFrame(T.GetString("AgencyEntry"));
                        if (Game.IsControlJustReleased(1, Control.Context))
                        {
                            try
                            {
                                EnterAgency();
                            }
                            catch (Exception e)
                            {
                                Logger.Log("Error: DisplayAgencyThisFrame - " + e.Message);
                                UI.Notify("MMI-SP: Error while creating the office.");

                                ErrorCancelAgency();

                                _menuMMI.Reset();
                                _menuMMI.Show();
                            }
                        }
                    }
            }
        }

        /// <summary>
        /// Creates the MMI blip on the map.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private Blip CreateBlip()
        {
            Blip blip = World.CreateBlip(_position);
            blip.Sprite = BlipSprite.Michael;
            blip.Color = (BlipColor)6;
            blip.Name = "Mors Mutual Insurance";
            blip.IsShortRange = true;

            return blip;
        }

        private void CreateMenuMMI()
        {
            _menuMMI = new MenuMMI();
            _menuMMI.GetMainmenu().OnMenuClose += (sender) =>
            {
                if (_office.itemsCollection.Type == ItemsManager.CollectionType.Night)
                    _office.NpcSay(DialogueManager.SpeechType.OfficeNaughtyBye);
                else
                    _office.NpcSay(DialogueManager.SpeechType.OfficeBye);
                ExitAgency();
            };
        }

        private void EnterAgency()
        {
#if DEBUG
            Logger.Log("DEBUG: Reset the menu");
#endif
            try
            {
                // Reset the menu
                _menuMMI.Reset();
            }
            catch (Exception e)
            {
                Logger.Log("Error: EnterAgency - " + e.Message);
                UI.Notify("MMI-SP: Error with module NativeUI!");

                ErrorCancelAgency();
                return;
            }
#if DEBUG
            Logger.Log("DEBUG: Entering cutscene");
#endif
            // Entering cutscene
            _isPlayerInCutscene = true;
#if DEBUG
            Logger.Log("Start entering cutscene and Fade screen out");
#endif
            // Start entering cutscene and Fade screen out
            Cutscenes.EnteringAgency();
#if DEBUG
            Logger.Log("Teleport the player in the office");
#endif
            // Teleport the player in the office
            Game.Player.Character.Position = OfficePlayerPos;
            Game.Player.Character.FreezePosition = true;
#if DEBUG
            Logger.Log("Force load office");
#endif
            // Force load office
            Function.Call(Hash.LOAD_SCENE, OfficePlayerPos.X, OfficePlayerPos.Y, OfficePlayerPos.Z);
#if DEBUG
            Logger.Log("Wait until everything is loaded");
#endif
            // Wait until everything is loaded
            SE.UI.WaitAndhideUI(1000);
#if DEBUG
            Logger.Log("Open menu");
#endif
            try
            {
                // Open menu
                _menuMMI.Show();
            }
            catch (Exception e)
            {
                Logger.Log("Error: EnterAgency - " + e.Message);
                UI.Notify("MMI-SP: Error with module NativeUI!");

                ErrorCancelAgency();
                return;
            }
#if DEBUG
            Logger.Log("Office creation");
#endif
            // Office creation
            if (_officeLastCreation.Days == World.CurrentDayTime.Days && _officeLastCreation.Hours == World.CurrentDayTime.Hours && _officeLastItemsCollection.Count > 0)
            {
#if DEBUG
                Logger.Log("Office creation with known items");
#endif
                _office = new Office(_officeLastItemsCollection);
            }
            else
            {
#if DEBUG
                Logger.Log("Office creation with new items");
#endif
                _office = new Office();
                _officeLastCreation = World.CurrentDayTime;
                if (_officeLastItemsCollection != null) _officeLastItemsCollection.DeleteItems();
                _officeLastItemsCollection = new ItemsManager.OfficeItemsCollection(_office.itemsCollection);
            }
            if (_office.itemsCollection.Type == ItemsManager.CollectionType.Night)
                _office.NpcSay(DialogueManager.SpeechType.OfficeNaughty);
            else
                _office.NpcSay(DialogueManager.SpeechType.OfficeHi);
#if DEBUG
            Logger.Log("DEBUG: _office.itemsCollection:");
            Logger.Log("DEBUG: type=" + _office.itemsCollection.Type.ToString());
            Logger.Log("DEBUG: count=" + _office.itemsCollection.Count.ToString());
#endif
        }
        private void ExitAgency()
        {
            Game.FadeScreenOut(1000);
            SE.UI.WaitAndhideUI(1000);

            // Removing office
            _office.CleanUp();
            _office = null;

            // Teleport the player to the entrance
            Game.Player.Character.FreezePosition = false;
            Game.Player.Character.Position = _position;
            
            // Force load spawn point
            Function.Call(Hash.LOAD_SCENE, _position.X, _position.Y, _position.Z);
            // Wait until everything is loaded
            Wait(1000);

            // Start leaving cutscene and Fade screen in
            Cutscenes.LeavingAgency();
            _isPlayerInCutscene = false;
        }

        private void ErrorCancelAgency(bool menu = true)
        {
            _isPlayerInCutscene = false;
            Game.Player.Character.Position = _position;
            Game.Player.Character.FreezePosition = false;
            Game.FadeScreenIn(1000);

            World.RenderingCamera = null;

            if (menu)
            {
                _menuMMI.Reset();
                _menuMMI.Show();
            }
        }
    }
}
