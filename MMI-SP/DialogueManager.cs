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
            internal string Param;
            internal int Index;

            public Speech(string speechName, string voiceName, string speechParam, int i = 0)
            {
                Name = speechName;
                Voice = voiceName;
                Param = speechParam;
                Index = i;
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
                case SpeechType.DriverBye:
                    list.AddRange(DriverByeCollection);
                    break;
            }
            return list;
        }


        // Office secretary
        private static List<Speech> OfficeHiCollection = new List<Speech> {
            new Speech("GENERIC_HI", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_01_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_01_WHITE_MINI_02", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_02_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HI", "A_F_M_BEVHILLS_02_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HI", "A_F_M_BUSINESS_02_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BUSINESS_02_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_01_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_02_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
        };
        private static List<Speech> OfficeNiceCarCollection = new List<Speech> {
            new Speech("NICE_CAR", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_02", "SPEECH_PARAMS_STANDARD"),
        };
        private static List<Speech> OfficeSomethingCollection = new List<Speech> {
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_01_WHITE_MINI_02", "SPEECH_PARAMS_STANDARD"),
            new Speech("GENERIC_HOWS_IT_GOING", "A_F_M_BEVHILLS_02_WHITE_MINI_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("PED_RANT_01", "A_F_M_BUSINESS_02_WHITE_MINI_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("CHALLENGE_ACCEPTED_GENERIC", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("CHAT_RESP", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("GENERIC_WHATEVER", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),

            new Speech("NICE_CAR", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_01", "SPEECH_PARAMS_STANDARD"),
            new Speech("NICE_CAR", "A_F_M_BEVHILLS_02_WHITE_FULL_02", "SPEECH_PARAMS_STANDARD"),
        };
        private static List<Speech> OfficeByeCollection = new List<Speech> {
            new Speech("GENERIC_BYE", "A_F_M_BUSINESS_02_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GOODBYE_ACROSS_STREET", "A_F_M_BUSINESS_02_WHITE_MINI_01", "SPEECH_PARAMS_FORCE"),
        };

        private static List<Speech> OfficeNaughtyCollection = new List<Speech> {
            new Speech("CHALLENGE_THREATEN", "A_F_M_BEACH_01_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_HI", "S_F_Y_HOOKER_01_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
            new Speech("HOOKER_OFFER_SERVICE", "S_F_Y_HOOKER_01_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
        };
        private static List<Speech> OfficeNaughtyByeCollection = new List<Speech> {
            new Speech("SEX_FINISHED", "S_F_Y_HOOKER_01_WHITE_FULL_01", "SPEECH_PARAMS_FORCE"),
        };

        // Driver
        private static List<Speech> DriverByeCollection = new List<Speech> {
            new Speech("GENERIC_BYE", "S_M_M_AUTOSHOP_01_WHITE_01", "SPEECH_PARAMS_FORCE"),
            new Speech("GENERIC_BYE", "S_M_M_GENERICMECHANIC_01_BLACK_MINI_01", "SPEECH_PARAMS_FORCE"),
        };
    }
}
