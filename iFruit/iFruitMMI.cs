using System;
using iFruitAddon2;

using GTA;

namespace MMI_SP.iFruit
{
    using T = Translator;

    class iFruitMMI : Script
    {
        private readonly CustomiFruit _iFruit;
        private MenuMMI _menuiFruit = null;
        private MenuConfig _menuConfig = null;

        public iFruitMMI()
        {
            _iFruit = new CustomiFruit();
            
            Tick += Initialize;
            Aborted += OnAborted;
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
                _menuiFruit?.MenuPoolProcessMenus();
                _menuConfig?.MenuPoolProcessMenus();
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

        // Aborted Event
        void OnAborted(object sender, EventArgs e)
        {
            if (_iFruit?.Contacts.Count > 0)
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
                GTA.UI.Notification.Show("MMI-SP: Error with module NativeUI!");
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
                GTA.UI.Notification.Show("MMI-SP: Error with module NativeUI!");
            }
            _iFruit.Close();
        }
    }
}
