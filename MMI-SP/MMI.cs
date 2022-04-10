using System;

using GTA;


/*
    Update assembly version before release!
*/
namespace MMI_SP
{
    internal class MMI : Script
    {
        public static bool IsDebug = true;
        
        private static bool _initialized = false;
        public static bool IsInitialized { get => _initialized; }


        public MMI()
        {
            // Trick to be able to wait for the game
            Tick += Initialize;
        }

        private void Initialize(object sender, EventArgs e)
        {
            // Reset log file
            Logger.ResetLogFile();
            
            Logger.Debug($"Waiting for game to be loaded...");
            while (Game.IsLoading)
            {
                Yield();
            }
            Logger.Debug("Game is loaded");


            Logger.Debug("Waiting for screen to fade...");
            while (Game.IsScreenFadingIn)
            {
                Yield();
            }
            Logger.Debug("Screen has faded");

            
            Logger.Debug("Loading configuration values...");
            Config.Initialize();
            Logger.Debug("Configuration values loaded");


            Logger.Debug("Checking prerequisites...");
            if (SelfCheck.ArePrerequisitesInstalled())
            {
                Logger.Debug("Prerequisites are installed");

                Logger.Debug("Checking for updates...");
                if (Config.CheckForUpdate)
                {
                    // Async check for updates
                    Updater.CheckForUpdate();
                }
            }
            else
            {
                Logger.Debug("Prerequisites are not installed");
            }

            _initialized = true;

            Tick -= Initialize;
        }
    }
}
