using GTA;
using System.Collections.Generic;

namespace MMI_SP
{
    static class DialogueManager
    {
        internal enum SpeechType { OfficeHi, OfficeBye, OfficeNiceCar, OfficeSomething, OfficeNaughty, OfficeNaughtyBye, DriverBye };

        /// <summary>
        /// Holds a speech.
        /// </summary>
        internal class Speech
        {
            internal string Name;
            internal string Voice;
            internal SpeechModifier Modifier;

            public Speech(string speechName, string voiceName, SpeechModifier speechmodifier)
            {
                Name = speechName;
                Voice = voiceName;
                Modifier = speechmodifier;
            }
        }


        internal static List<Speech> GetSpeechList(SpeechType type)
        {
            List<Speech> list = new List<Speech>();

            switch (type)
            {
                case SpeechType.OfficeHi:
                    list.AddRange(OfficeHiCollection);
                    break;
                case SpeechType.OfficeNiceCar:
                    list.AddRange(OfficeNiceCarCollection);
                    break;
                case SpeechType.OfficeSomething:
                    list.AddRange(OfficeSomethingCollection);
                    break;
                case SpeechType.OfficeBye:
                    list.AddRange(OfficeByeCollection);
                    break;
                case SpeechType.OfficeNaughty:
                    list.AddRange(OfficeNaughtyCollection);
                    break;
                case SpeechType.OfficeNaughtyBye:
                    list.AddRange(OfficeNaughtyByeCollection);
                    break;
                case SpeechType.DriverBye:
                    list.AddRange(DriverByeCollection);
                    break;
            }
            return list;
        }


        // Office secretary
        private static readonly List<Speech> OfficeHiCollection = new List<Speech> {
            new Speech("GENERIC_HI", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Force),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_01_WHITE_MINI_01", SpeechModifier.Force),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_01_WHITE_MINI_02", SpeechModifier.Force),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_02_WHITE_FULL_01", SpeechModifier.Force),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_02_WHITE_MINI_01", SpeechModifier.Force),
            new Speech("GENERIC_HI", "A_F_M_BUSINESS_02_WHITE_MINI_01", SpeechModifier.Force),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BUSINESS_02_WHITE_MINI_01", SpeechModifier.Force),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Force),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_01_WHITE_MINI_01", SpeechModifier.Force),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_02_WHITE_FULL_01", SpeechModifier.Force),
        };
        private static readonly List<Speech> OfficeNiceCarCollection = new List<Speech> {
            new Speech("NICE_CAR", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Standard),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_01", SpeechModifier.Standard),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_02", SpeechModifier.Standard),
        };
        private static readonly List<Speech> OfficeSomethingCollection = new List<Speech> {
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_01_WHITE_MINI_02", SpeechModifier.Standard),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_02_WHITE_MINI_01", SpeechModifier.Standard),
            new Speech("PED_RANT_01", "A_F_M_BUSINESS_02_WHITE_MINI_01", SpeechModifier.Standard),
            new Speech("CHALLENGE_ACCEPTED_GENERIC", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Standard),
            new Speech("CHAT_RESP", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Standard),
            new Speech("GENERIC_WHATEVER", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Standard),

            new Speech("NICE_CAR", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Standard),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_01", SpeechModifier.Standard),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_02", SpeechModifier.Standard),
        };
        private static readonly List<Speech> OfficeByeCollection = new List<Speech> {
            new Speech("GENERIC_BYE", "A_F_M_BUSINESS_02_WHITE_MINI_01", SpeechModifier.Force),
            new Speech("GOODBYE_ACROSS_STREET", "A_F_M_BUSINESS_02_WHITE_MINI_01", SpeechModifier.Force),
        };

        private static readonly List<Speech> OfficeNaughtyCollection = new List<Speech> {
            new Speech("CHALLENGE_THREATEN", "A_F_M_BEACH_01_WHITE_FULL_01", SpeechModifier.Force),
            new Speech("GENERIC_HI", "S_F_Y_HOOKER_01_WHITE_FULL_01", SpeechModifier.Force),
            new Speech("HOOKER_OFFER_SERVICE", "S_F_Y_HOOKER_01_WHITE_FULL_01", SpeechModifier.Force),
        };
        private static readonly List<Speech> OfficeNaughtyByeCollection = new List<Speech> {
            new Speech("SEX_FINISHED", "S_F_Y_HOOKER_01_WHITE_FULL_01", SpeechModifier.Force),
        };

        // Driver
        private static readonly List<Speech> DriverByeCollection = new List<Speech> {
            new Speech("GENERIC_BYE", "S_M_M_AUTOSHOP_01_WHITE_01", SpeechModifier.Force),
            new Speech("GENERIC_BYE", "S_M_M_GENERICMECHANIC_01_BLACK_MINI_01", SpeechModifier.Force),
        };
    }
}
