using System;
using iFruitAddon2;

using GTA;

namespace MMI_SP.iFruit
{
    using T = Translator;

    class iFruitMMI : Script
    {
        private CustomiFruit _iFruit;
        private MenuMMI _menuiFruit = null;
        private MenuConfig _menuConfig = null;

        public iFruitMMI()
        {
            _iFruit = new CustomiFruit();
            
            Tick += Initialize;
        }

        void Initialize(object sender, EventArgs e)
        {
            // Waiting for Insurance Observer to be ready
            while (!InsuranceObserver.Initialized)
            {
                Yield();
            }

            _menuiFruit = new MenuMMI();
            _menuConfig = new MenuConfig();

            Wait(2000);

            iFruitContact contactMMI = new iFruitContact("Mors Mutual Insurance")
            {
                DialTimeout = 4000, Active = true, Icon = ContactIcon.MP_MorsMutual
            };
            contactMMI.Answered += ContactAnsweredMMI;
             
            iFruitContact contactConf = new iFruitContact(T.GetString("ConfigMenuContact"))
            {
                DialTimeout = 0, Active = true, Icon = ContactIcon.MP_FmContact
            };
            contactConf.Answered += ContactAnsweredConfig;


            _iFruit.Contacts.Add(contactMMI);
            _iFruit.Contacts.Add(contactConf);

            Tick -= Initialize;
            Tick += OnTick;
        }

        // OnTick Event
        void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (_menuiFruit != null) _menuiFruit.MenuPoolProcessMenus();
                if (_menuConfig != null) _menuConfig.MenuPoolProcessMenus();
            }
            catch (DivideByZeroException)
            {
                // Happen when there are no items left in a menu
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            _iFruit.Update();
        }

        // Dispose Event
        protected override void Dispose(bool A_0)
        {
            if (A_0)
            {
                _iFruit.Contacts.ForEach(x => x.EndCall());
            }
        }

        internal void MenuClosed(object sender)
        {
            MMISound.Play(MMISound.SoundFamily.Bye);
            _menuiFruit.Mainmenu.OnMenuClose -= MenuClosed;
        }


        private void ContactAnsweredMMI(iFruitContact contact)
        {
            try
            {
                _menuiFruit.Reset(true);
                _menuiFruit.Show();
                _menuiFruit.Mainmenu.OnMenuClose += MenuClosed;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                UI.Notify("MMI-SP: Error with module NativeUI!");
            }

            MMISound.Play(MMISound.SoundFamily.Hello);
            _iFruit.Close(2000);
        }


        private void ContactAnsweredConfig(iFruitContact contact)
        {
            try
            {
                _menuConfig.Show();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                UI.Notify("MMI-SP: Error with module NativeUI!");
            }
            _iFruit.Close();
        }
    }
}
