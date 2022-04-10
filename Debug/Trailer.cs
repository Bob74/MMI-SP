using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Native;
using GTA.Math;


namespace MMI_SP
{
    internal static class Trailer
    {
        static Ped _michael, _franklin, _trevor, _freemode;

        // Michael Setup
        readonly static Vector3 _michaelPosition = new Vector3(-787.1056f, 185.9241f, 72.83529f);
        readonly static float _michaelHeading = 58.53875f;

        // Franklin Setup
        readonly static Vector3 _franklinPosition = new Vector3(-18.88375f, -1451.604f, 30.58212f);
        readonly static float _franklinHeading = 223.4324f;
        readonly static Vector3 _franklinCarPosition = new Vector3(-25.07652f, -1450.024f, 30.1692f);
        readonly static float _franklinCarHeading = 183.715f;

        // Trevor Setup
        readonly static Vector3 _trevorPosition = new Vector3(1984.025f, 3817.162f, 32.28379f);
        readonly static float _trevorHeading = 228.1426f;

        // Freemode Setup
        readonly static Vector3 _freemodePosition = new Vector3(-777.3974f, 282.0237f, 85.77721f);
        readonly static float _freemodeHeading = 179.5031f;
        readonly static Vector3 _freemodeTrevorPosition = new Vector3(-778.6523f, 282.0237f, 85.78682f);
        readonly static float _freemodeTrevorHeading = 179.5031f;

        internal static void CleanUp()
        {
            if (_michael != null) _michael.Delete();
            if (_franklin != null) _franklin.Delete();
            if (_trevor != null) _trevor.Delete();
            if (_freemode != null) _freemode.Delete();
        }

        internal static void Spawn(PedHash hash)
        {
            switch (hash)
            {
                case PedHash.Michael:
                    _michael = World.CreatePed(PedHash.Michael, _michaelPosition, _michaelHeading);
                    SetupMichael();
                    break;
                case PedHash.Franklin:
                    if (_franklin != null) _franklin.Delete();
                    _franklin = World.CreatePed(PedHash.Franklin, _franklinPosition, _franklinHeading);
                    SetupFranklin();
                    break;
                case PedHash.Trevor:
                    if (_trevor != null) _trevor.Delete();
                    _trevor = World.CreatePed(PedHash.Trevor, _trevorPosition, _trevorHeading);
                    break;
                case PedHash.FreemodeMale01:
                    if (_freemode != null) _freemode.Delete();
                    _freemode = World.CreatePed(PedHash.FreemodeMale01, _freemodePosition, _freemodeHeading);
                    if (_trevor != null) _trevor.Delete();
                    _trevor = World.CreatePed(PedHash.Trevor, _freemodeTrevorPosition, _freemodeTrevorHeading);
                    SetFreemodeClothes();
                    break;
            }
        }

        private static void SetupMichael()
        {
            Function.Call(Hash.SET_PED_PROP_INDEX, _michael, 1, 5, 0, 0);
        }

        private static void SetupFranklin()
        {
            Vehicle buffalo = World.CreateVehicle(VehicleHash.Buffalo2, _franklinCarPosition, _franklinCarHeading);
            buffalo.PrimaryColor = VehicleColor.PureWhite;
            buffalo.WindowTint = VehicleWindowTint.Limo;
            buffalo.DirtLevel = 0f;
            buffalo.MarkAsNoLongerNeeded();
        }

        private static void SetFreemodeClothes()
        {
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 8, 3, 0, 0);  // Accessories

            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 0, 4, 0, 0);   // Face
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 2, 2, 4, 0);   // Hair
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 3, 1, 0, 0);   // Torso
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 4, 0, 1, 0);   // Legs
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 6, 0, 0, 0);   // Feet
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, _freemode, 11, 4, 0, 0);  // Torso2

            Function.Call(Hash.SET_PED_PROP_INDEX, _freemode, 1, 3, 4, 0);
        }

        internal static void TrevorAttackFreemode()
        {
            _freemode.Health = 1;
            if (_trevor != null && _freemode != null)
                _trevor.Task.FightAgainst(_freemode);
        }

        internal static void CharacterCurse(PedHash hash, string insult = "GENERIC_CURSE_HIGH")
        {
            switch (hash)
            {
                case PedHash.Michael:
                    if (_michael != null)
                        Function.Call(Hash._PLAY_AMBIENT_SPEECH_WITH_VOICE, _michael, insult, "MICHAEL_NORMAL", "SPEECH_PARAMS_FORCE", 0);
                    break;
                case PedHash.Franklin:
                    if (_franklin != null)
                        Function.Call(Hash._PLAY_AMBIENT_SPEECH_WITH_VOICE, _franklin, insult, "FRANKLIN_NORMAL", "SPEECH_PARAMS_FORCE", 0);
                    break;
                case PedHash.Trevor:
                    if (_trevor != null)
                        Function.Call(Hash._PLAY_AMBIENT_SPEECH_WITH_VOICE, _trevor, insult, "TREVOR_NORMAL", "SPEECH_PARAMS_FORCE", 3);
                    break;
                case PedHash.FreemodeMale01:
                    if (_freemode != null)
                    {
                        _freemode.Task.PlayAnimation("mp_celebration@idles@male", "celebration_idle_m_b");
                    }

                    break;
            }
        }

        internal static void TrevorFight()
        {
            if (_freemode != null)
                _trevor.Task.FightAgainst(_freemode);
        }

    }
}
