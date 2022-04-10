using System;

using GTA;
using GTA.Native;
using GTA.Math;
using System.Collections.Generic;

using static MMI_SP.Agency.ItemsManager;
using static MMI_SP.DialogueManager;

namespace MMI_SP.Agency
{
    class Office
    {
        internal OfficeItemsCollection itemsCollection;

        private Vector3 officeCameraPos = new Vector3(116.0f, -620.50f, 206.35f);
        private Camera officeCamera;

        private Vector3 npcPos = new Vector3(114.35f, -619.3748f, 204.50f);
        private Vector3 npcRot = new Vector3(0.0f, 0.0f, -120.0f);
        private string npcModel = "a_f_y_business_01";
        private Ped npc;

        private Weather officeWeather;

        public Office()
        {
            // Building item set
            itemsCollection = new OfficeItemsCollection(GetItemsCollection());
            BuildOffice();
        }
        public Office(OfficeItemsCollection collection)
        {
            // Building item set
            itemsCollection = new OfficeItemsCollection(collection);
            BuildOffice();
        }
        private void BuildOffice()
        {
            // Store current weather and change to a non-raining weather (rain drops pass through the roof)
            officeWeather = World.Weather;
            World.Weather = Weather.Clouds;

            // Creates the camera
            officeCamera = World.CreateCamera(officeCameraPos, new Vector3(0.0f, 0.0f, 0.0f), GameplayCamera.FieldOfView);

            // Creates the NPC
            npc = CreateNpc();
            if (itemsCollection.Type == CollectionType.Night) Function.Call(Hash.SET_PED_COMPONENT_VARIATION, npc, 2, 0, 2, 0);
            SetNpcAI();

            // Creates the props
            foreach (OfficeItem item in itemsCollection)
            {
                if (npc != null)
                {
                    Prop prop = item.Init();
                    if (prop != null)
                    {
                        if (prop.Exists())
                            if (npc.Exists()) npc.SetNoCollision(prop, true);
                    }
                    else
                        Logger.Log("Error: BuildOffice Props - prop is null!");
                }
                else
                    Logger.Log("Error: BuildOffice Props - npc is null!");
            }
                

            // See through the camera
            if (npc != null)
            {
                if (npc.Exists())
                    officeCamera.PointAt(npc, (int)Bone.IK_Head);
            }
            else
            {
                officeCamera.PointAt(npcPos);
                Logger.Log("Error: BuildOffice Camera - npc is null!");
            }

            World.RenderingCamera = officeCamera;

            // Show view
            Game.FadeScreenIn(1000);
            SE.UI.WaitAndhideUI(1000);
        }
        internal void CleanUp()
        {
            World.RenderingCamera = null;
            officeCamera.IsActive = false;
            officeCamera.Destroy();

            World.Weather = officeWeather;
            if (npc != null)
            {
                if (npc.Exists())
                {
                    npc.Task.ClearAllImmediately();
                    npc.Delete();
                }
            }
            else
                Logger.Log("Error: CleanUp - npc is null!");

            itemsCollection.DeleteItems();
        }

        /// <summary>
        /// Return the item collection depending of the time of the day.
        /// </summary>
        /// <returns></returns>
        private OfficeItemsCollection GetItemsCollection()
        {
            Random rnd = new Random(Game.GameTime);
            List<OfficeItemsCollection> itemsCollectionPool = new List<OfficeItemsCollection>();
            OfficeItemsCollection collec;
            do
            {
                if (World.CurrentDayTime.Hours >= 2 && World.CurrentDayTime.Hours < 12)
                    itemsCollectionPool.AddRange(ItemsManager.GetItemsCollection(CollectionType.Normal));
                else if (World.CurrentDayTime.Hours >= 12 && World.CurrentDayTime.Hours < 14)
                {
                    itemsCollectionPool.AddRange(ItemsManager.GetItemsCollection(CollectionType.Midday));
                    itemsCollectionPool.AddRange(ItemsManager.GetItemsCollection(CollectionType.Normal));
                }
                else if (World.CurrentDayTime.Hours >= 14 && World.CurrentDayTime.Hours < 0)
                    itemsCollectionPool.AddRange(ItemsManager.GetItemsCollection(CollectionType.Normal));
                else
                {
                    itemsCollectionPool.AddRange(ItemsManager.GetItemsCollection(CollectionType.Normal));
                    itemsCollectionPool.AddRange(ItemsManager.GetItemsCollection(CollectionType.Night));
                }
                collec = itemsCollectionPool[rnd.Next(0, itemsCollectionPool.Count - 1)];
            } while (collec.Type == CollectionType.Empty);

            return collec;
        }

        private Ped CreateNpc()
        {
            Ped npc = World.CreatePed(npcModel, npcPos);

            if (npc != null)
            {
                if (npc.Exists())
                { 
                    npc.IsPersistent = true;

                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, npc, 0, 0, 0, 0);   // Face
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, npc, 2, 1, 0, 0);   // Hair
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, npc, 3, 1, 0, 0);   // Torso
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, npc, 4, 0, 1, 0);   // Legs
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, npc, 6, 0, 0, 0);   // Feet
                    Function.Call(Hash.SET_PED_PROP_INDEX, npc, 1, 0, 0, 0);            // Glasses
                }
            }
            else
                Logger.Log("Error: CreateNpc - npc is null!");

            return npc;
        }
        private void SetNpcAI()
        {
            if (npc != null)
            {
                if (npc.Exists())
                {
                    // Sits on the chair
                    npc.Task.PlayAnimation("amb@prop_human_seat_chair@female@arms_folded@base", "base", 1.0f, -1, AnimationFlags.Loop);

                    // Freeze position
                    npc.FreezePosition = true;

                    npc.Position = npcPos;
                    npc.Rotation = npcRot;

                    // Look at the camera
                    npc.Task.LookAt(officeCameraPos);
                }
                else
                    Logger.Log("Error: SetNpcAI - npc doesn't exist!");
            }
            else
                Logger.Log("Error: SetNpcAI - npc is null!");

        }
        internal void NpcSay(SpeechType type)
        {
            Random rnd = new Random();
            List<Speech> speeches = new List<Speech>(GetSpeechList(type));
            
            int i = rnd.Next(0, speeches.Count - 1);
            Speech speech = speeches[i];
            Function.Call(Hash._PLAY_AMBIENT_SPEECH_WITH_VOICE, npc, speech.Name, speech.Voice, speech.Param, speech.Index);
        }

    }
}
