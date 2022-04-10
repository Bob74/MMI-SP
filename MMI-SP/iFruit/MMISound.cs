using System;
using System.IO;
using System.Media;
using System.Collections.Generic;

namespace MMI_SP.iFruit
{
    static class MMISound
    {
        private static int _volume = 25;
        public static int Volume { get => _volume; set => _volume = value; }

        private static Random _rnd = new Random();
        public enum SoundFamily { Hello, Okay, Bye, NoMoney };

        private static List<UnmanagedMemoryStream> _helloList = new List<UnmanagedMemoryStream> {
            Properties.Resources.Start_HelloThisIsMMI,
            Properties.Resources.Start_MMIExpectUnexpected,
            Properties.Resources.Start_MMIHereToHelp,
            Properties.Resources.Start_MMIHowCanHelp,
            Properties.Resources.Start_MMIHowCanIBeService,
            Properties.Resources.Start_MMIPeaceOfMind,
            Properties.Resources.Start_MMITrust,
            Properties.Resources.Start_WhatCanIDo,
            Properties.Resources.Start_WhatCanIHelpYouWith};

        private static List<UnmanagedMemoryStream> _byeList = new List<UnmanagedMemoryStream> {
            Properties.Resources.End_ByeNow,
            Properties.Resources.End_DriveSafe,
            Properties.Resources.End_NiceDay,
            Properties.Resources.End_NiveDay2,
            Properties.Resources.End_SoLong,
            Properties.Resources.End_StaySafe};

        private static List<UnmanagedMemoryStream> _okayList = new List<UnmanagedMemoryStream> {
            Properties.Resources.Mid_ICanDoThat,
            Properties.Resources.Mid_ILookIntoit,
            Properties.Resources.Mid_IWillDoMyBest,
            Properties.Resources.Mid_Okay,
            Properties.Resources.Mid_Sure,
            Properties.Resources.Mid_WeCanDoThat,
            Properties.Resources.Mid_WeCanHandleThat};

        private static List<UnmanagedMemoryStream> _noMoneyList = new List<UnmanagedMemoryStream> { Properties.Resources.NoMoney };


        public static void Play(SoundFamily family)
        {
            List<UnmanagedMemoryStream> list = new List<UnmanagedMemoryStream>();
            if (family == SoundFamily.Hello)
                list.AddRange(_helloList);
            else if (family == SoundFamily.Okay)
                list.AddRange(_okayList);
            else if (family == SoundFamily.Bye)
                list.AddRange(_byeList);
            else if (family == SoundFamily.NoMoney)
                list.AddRange(_noMoneyList);

            int index = _rnd.Next(0, list.Count - 1);

            try
            {
                Stream stream = list[index];
                stream.Position = 0;

                WaveStream wvStream = new WaveStream(stream);
                if (_volume < 0) _volume = 0;
                if (_volume > 100) _volume = 100;

                wvStream.Volume = _volume;

                SoundPlayer player = new SoundPlayer(wvStream);
                player.Play();
            }
            catch (Exception e)
            {
                Logger.Log("Error: MMISound.Play - " + family.ToString() + " n°" + index.ToString() + ". " + e.Message);
            }

        }
        
    }
}
