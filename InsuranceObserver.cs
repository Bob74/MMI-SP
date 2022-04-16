using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using GTA;
using GTA.Native;
using GTA.Math;

using MMI_SP.Common;
using static MMI_SP.DialogueManager;

namespace MMI_SP
{
    using T = Translator;

    public class InsuranceObserver : Script
    {
        /// <summary>
        /// Raised when an insured vehicle is detected.
        /// Out: Vehicle
        /// </summary>
        public delegate void InsuredVehicleDetected(InsuranceObserver sender, Vehicle veh);
        public event InsuredVehicleDetected Detected;
        protected virtual void Raise_InsuredVehicleDetected(InsuranceObserver sender, Vehicle veh) { Detected?.Invoke(this, veh); }

        private static bool _initialized = false;
        public static bool Initialized { get => _initialized; private set => _initialized = value; }

        private static InsuranceObserver _instance;
        public static InsuranceObserver Instance { get => _instance; }

        private static List<Vehicle> _insuredVehList = new List<Vehicle>();
        public static List<Vehicle> InsuredVehList { get => _insuredVehList; set => _insuredVehList = value; }

        private static List<Vehicle> _recoveredVehList = new List<Vehicle>();
        public static List<Vehicle> RecoveredVehList { get => _recoveredVehList; set => _recoveredVehList = value; }

        private static Dictionary<string, Blip> _blipsToRemove = new Dictionary<string, Blip>();
        public static Dictionary<string, Blip> BlipsToRemove { get => _blipsToRemove; set => _blipsToRemove = value; }

        private InsuranceManager _im;
        private List<IncomingVehicle> _incomingVehicles = new List<IncomingVehicle>();
        internal List<IncomingVehicle> IncomingVehicles { get => _incomingVehicles; set => _incomingVehicles = value; }

        private Vehicle _previousVehicle = null;

        // Timers
        private int _timerInsurance = 0;
        private int _timerDetectInsuredVehicles = 0;
        private int _timerRecoveredVehicle = 0;
        private int _timerIncomingVehicle = 0;

        private int _delayDetectInsuredVehicles = 3000;
        public int DelayDetectInsuredVehicles { get => _delayDetectInsuredVehicles; private set => _delayDetectInsuredVehicles = value; }

        public InsuranceObserver()
        {
            _instance = this;

            Tick += Initialize;
        }

        void Initialize(object sender, EventArgs e)
        {
            // Waiting for the main plugin to be ready
            while (!MMI.IsInitialized)
            {
                Yield();
            }

            _im = new InsuranceManager();
            _im.Insured += OnVehicleInsured;
            _im.Recovered += OnVehicleRecovered;
            _im.Canceled += OnVehicleCanceled;

            _initialized = true;

            Tick -= Initialize;
            Tick += OnTick;
        }
        
#if DEBUG
        public const float Width = 1280f;
        public const float Height = 720f;
        public static float AspectRatio => Function.Call<float>(Hash._0xF1307EF624A80D87, 0);
        public static float ScaledWidth => Height * AspectRatio;
        public static PointF WorldToScreen(Vector3 position, bool scaleWidth = false)
        {
            float pointX, pointY;

            unsafe
            {
                if (!Function.Call<bool>(Hash._0x34E82F05DF2974F5, position.X, position.Y, position.Z, &pointX, &pointY))
                {
                    return PointF.Empty;
                }
            }

            pointX *= scaleWidth ? ScaledWidth : Width;
            pointY *= Height;

            return new PointF(pointX, pointY);
        }
#endif

        // OnTick Event
        void OnTick(object sender, EventArgs e)
        {
#if DEBUG
            Size screenRes = SE.UI.GetScreenResolution();

            foreach (Vehicle veh in World.GetAllVehicles())
            {
                if (Game.Player.Character.Position.DistanceTo(veh.Position) < 30f)
                {
                    Vector3 pos = veh.Position;
                    pos.Z += 2.0f;
                    
                    PointF screenCoo = WorldToScreen(pos);

                    SE.UI.DrawText(InsuranceManager.GetVehicleInsuranceCost(veh).ToString(), 0, true, (float)((float)screenCoo.X / screenRes.Width), (float)((float)screenCoo.Y / screenRes.Height), 1.0f, 64, 255, 64);
                }
            }

            Vehicle curVehicle = Game.Player.LastVehicle;
            if (curVehicle != null) SE.UI.DrawText("X: " + curVehicle.Model.GetDimensions().X.ToString() + " / Y: " + curVehicle.Model.GetDimensions().Y.ToString());
#endif
            // When timers end
            if (_timerInsurance <= Game.GameTime)
            {
                UpdateInsurance();
                _timerInsurance = Game.GameTime + 1000;
            }
            if (_timerRecoveredVehicle <= Game.GameTime)
            {
                UpdateRecoveredVehicles();
                _timerRecoveredVehicle = Game.GameTime + 3000;
            }
            if (_timerDetectInsuredVehicles <= Game.GameTime)
            {
                CheckForInsuredVehicles();
                _timerDetectInsuredVehicles = Game.GameTime + DelayDetectInsuredVehicles;
            }
            if (IncomingVehicles.Count > 0)
            {
                if (_timerIncomingVehicle <= Game.GameTime)
                {
                    UpdateIncomingVehicles();
                    _timerIncomingVehicle = Game.GameTime + 1000;
                }
            }

            // Display the insurance status of the vehicle the player enter
            // The player enter or leave a vehicle
            if (_previousVehicle != Game.Player.Character.CurrentVehicle)
            {
                _previousVehicle = Game.Player.Character.CurrentVehicle;
                if (_previousVehicle != null)
                {
                    // Remove Blip if necessary (recovered vehicle or bringed vehicle)
                    RemoveRecoverBlip(_previousVehicle);

                    // Insured icon
                    if (InsuranceManager.IsVehicleInsurable(_previousVehicle))
                    {
                        if (InsuranceManager.IsVehicleInsured(_previousVehicle))
                        {
                            SE.UI.DrawTexture(Config.InsuranceImage, 4500, 0.955f, 0.83f, Color.FromArgb(35, 199, 128));
                        }
                        else
                        {   
                            SE.UI.DrawTexture(Config.InsuranceImage, 4500, 0.955f, 0.83f, Color.FromArgb(190, 0, 50));
                        }
                    }
                }
            }
        }


        static string[] garages = new string[] {
            "Michael - Beverly Hills",
            "Trevor - Countryside", "Trevor - City", "Trevor - Stripclub",
            "Franklin - Aunt", "Franklin - Hills",
            "Lockup_PSY_01", "Lockup_PSY_02", "Lockup_PSY_03",
            "Lockup_CSY_01", "Lockup_CSY_02", "Lockup_CSY_03",
            "Lockup_CMS_01", "Lockup_CMS_02", "Lockup_CMS_03"
        };
        private bool IsVehicleInGarage(Vehicle veh)
        {
            bool isInGarage = false;

            if (veh != null)
            {
                foreach (string garage in garages)
                {
                    isInGarage = Function.Call<bool>(Hash.IS_VEHICLE_IN_GARAGE_AREA, garage, veh);
                    if (isInGarage)
                    {
                        break;
                    }
                }
            }

            return isInGarage;
        }

        // Dispose Event
        protected override void Dispose(bool A_0)
        {
            if (A_0)
            {
                ClearAllBlips();
                RemovePersistence();
            }
        }

        /// <summary>
        /// Remove all remaining Blips from the map.
        /// </summary>
        private void ClearAllBlips()
        {
            for (int i = BlipsToRemove.Count - 1; i >= 0; i--)
            {
                Blip toDel = BlipsToRemove.ElementAt(i).Value;
                if (toDel != null)
                    if (toDel.Exists())
                        toDel.Remove();
            }
        }
        /// <summary>
        /// Removes the Blip added by the insurance to recovered vehicles.
        /// </summary>
        /// <param name="veh"></param>
        internal static void RemoveRecoverBlip(Vehicle veh)
        {
            BlipsToRemove.TryGetValue(Utils.GetVehicleIdentifier(veh), out Blip vehicleBlip);

            if (vehicleBlip != null)
            {
                vehicleBlip.Remove();
                BlipsToRemove.Remove(Utils.GetVehicleIdentifier(veh));
            }
        }

        /// <summary>
        /// Remove persistence from all recovered vehicles.
        /// </summary>
        private void RemovePersistence()
        {
            for (int i = RecoveredVehList.Count - 1; i >= 0; i--)
                if (!Config.PersistentVehicles) RecoveredVehList.ElementAt(i).IsPersistent = false;
        }

        /// <summary>
        /// List all vehicles and keeps the insured ones.
        /// Also replace licence number plate 46EEK572 of all vehicles by a random one.
        /// </summary>
        private void CheckForInsuredVehicles()
        {
            Vehicle[] array = World.GetAllVehicles();

            foreach (Vehicle veh in array)
            {
                if (!veh.IsDead)
                {
                    if (!InsuredVehList.Contains(veh))
                    {
                        if (veh.NumberPlate == "46EEK572") veh.NumberPlate = SE.Vehicle.GetRandomNumberPlate();
                        if (_im.IsVehicleInDB(Utils.GetVehicleIdentifier(veh)))
                        {
                            InsuredVehList.Add(veh);
                            Raise_InsuredVehicleDetected(this, veh);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Check if a vehicle is dead and updates the database.
        /// Also updates the vehicle modifications in the database.
        /// </summary>
        private void UpdateInsurance()
        {
            for (int i = InsuredVehList.Count - 1; i >= 0; i--)
            {
                Vehicle currenVeh = InsuredVehList.ElementAt(i);
                if (currenVeh.Exists())
                {
                    if (currenVeh.IsDead)
                    {
                        string vehIdentifier = Utils.GetVehicleIdentifier(currenVeh);

                        SE.UI.DrawNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("NotifyVehicleDestroyedTitle"), T.GetString("NotifyVehicleDestroyedSubtitle"));

                        _im.SetVehicleStatusToDB(vehIdentifier, "Dead");
                        _im.UpdateVehicleToDB(currenVeh); // Save the last configuration of the vehicle
                        currenVeh.IsPersistent = false;
                        InsuredVehList.RemoveAt(i);
                        RemoveRecoverBlip(currenVeh);
                        break;
                    }
                    else
                    {
                        // Only update the vehicle in DB if the player is inside
                        // (A vehicle shouldn't be modified without the player driving)
                        if (Game.Player.Character.CurrentVehicle == currenVeh)
                        {
                            // Ensure we aren't in LSC or Benny's by checking if we use another camera
                            if (GameplayCamera.IsRendering)
                                _im.UpdateVehicleToDB(currenVeh);
                        }



                        if (IsVehicleInGarage(currenVeh))
                        {
                            if (Config.PersistentVehicles) Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, currenVeh, false, true);
                        }
                        else
                        {
                            if (Config.PersistentVehicles) currenVeh.IsPersistent = true;
                        }
                        



                        //if (PersistentVehicles) currenVeh.IsPersistent = true;
                    }
                }
                else
                {
                    InsuredVehList.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Check the recovered vehicles and remove persistence/blips when needed.
        /// </summary>
        private void UpdateRecoveredVehicles()
        {
            for (int i = RecoveredVehList.Count - 1; i >= 0; i--)
            {
                Vehicle recoveredVehicle = RecoveredVehList.ElementAt(i);
                if (Game.Player.LastVehicle == recoveredVehicle || !recoveredVehicle.Exists() || recoveredVehicle.IsDead)
                {
                    if (recoveredVehicle.Exists())
                    {
                        if (recoveredVehicle.IsAlive)
                            SE.UI.DrawNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("NotifyVehicleRecoveredTitle"), T.GetString("NotifyVehicleRecoveredSubtitle"));

                        // Remove persistence
                        if (!Config.PersistentVehicles) recoveredVehicle.IsPersistent = false;
                    }

                    // Remove the vehicle from the list
                    RecoveredVehList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Update the driver of the upcoming vehicles.
        /// </summary>
        private void UpdateIncomingVehicles()
        {
            for (int i = IncomingVehicles.Count - 1; i >= 0; i--)
            {
                IncomingVehicle incoming = IncomingVehicles[i];

                if (incoming.vehicle.CurrentBlip.Sprite == BlipSprite.ArmsTraffickingAir ||
                    incoming.vehicle.CurrentBlip.Sprite == BlipSprite.Tank ||
                    incoming.vehicle.CurrentBlip.Sprite == BlipSprite.Speedboat ||
                    incoming.vehicle.CurrentBlip.Sprite == BlipSprite.GunCar)
                    incoming.vehicle.CurrentBlip.Rotation = (int)incoming.vehicle.Rotation.Z;

                // If the driver destroyed the vehicle, we refund the player
                if (incoming.vehicle.IsDead)
                {
                    CannotBringVehicle(incoming, InsuranceManager.GetVehicleInsuranceCost(incoming.vehicle, InsuranceManager.Multiplier.Recover));
                    break;
                }
                
                // The driver is in the vehicle and has arrived
                if (incoming.vehicle.Model.IsHelicopter)
                {
                    if (incoming.driver.IsInVehicle(incoming.vehicle) && incoming.vehicle.Speed <= 0.5f && (incoming.vehicle.Position.Z - World.GetGroundHeight(incoming.vehicle.Position) <= 5.0f))
                    {
                        incoming.driver.Task.LeaveVehicle();
                        Function.Call(Hash.RESET_PED_LAST_VEHICLE, incoming.driver);
                        break;
                    }
                }
                else if (incoming.vehicle.Model.IsPlane)
                {
                    if (incoming.driver.IsInVehicle(incoming.vehicle) && incoming.vehicle.Speed <= 5.0f && (incoming.vehicle.Position.Z - World.GetGroundHeight(incoming.vehicle.Position) <= 10.0f))
                    {
                        incoming.driver.Task.LeaveVehicle();
                        Function.Call(Hash.RESET_PED_LAST_VEHICLE, incoming.driver);
                        break;
                    }
                }
                else if (incoming.driver.IsInVehicle(incoming.vehicle) && ((incoming.driver.Position.DistanceTo(incoming.destination) <= 5.0f && incoming.driver.Position.Z - incoming.destination.Z <= 2.0f) ||
                    (incoming.driver.Position.DistanceTo(Game.Player.Character.Position) <= 5.0f && incoming.driver.Position.Z - Game.Player.Character.Position.Z <= 2.0f)))
                {
                    incoming.driver.Task.LeaveVehicle();
                    Function.Call(Hash.RESET_PED_LAST_VEHICLE, incoming.driver);
                    break;
                }

                // The driver has left the car
                if (!incoming.driver.IsInVehicle(incoming.vehicle))
                {
                    incoming.driver.IsPersistent = false;
                    incoming.driver.MarkAsNoLongerNeeded();
                    incoming.driver.Task.WanderAround();
                    IncomingVehicles.Remove(incoming);

                    // Only say Bye if the player is close
                    if (incoming.driver.Position.DistanceTo(Game.Player.Character.Position) < 8.0f)
                    {
                        Random rnd = new Random();
                        List<Speech> speeches = new List<Speech>(GetSpeechList(SpeechType.DriverBye));

                        int n = rnd.Next(0, speeches.Count - 1);
                        Speech speech = speeches[n];
                        Function.Call(Hash._PLAY_AMBIENT_SPEECH_WITH_VOICE, incoming.driver, speech.Name, speech.Voice, speech.Param, speech.Index);
                    }

                    break;
                }

                // If the driver is not arrived after x minutes
                if (Game.GameTime - incoming.calledTime > (Config.BringVehicleTimeout * 60000))
                {
                    CannotBringVehicle(incoming);
                    break;
                }

            }
        }
        /// <summary>
        /// Moves the vehicle to the nearest player spot.
        /// </summary>
        /// <param name="veh"></param>
        /// <param name="instant"></param>
        internal void BringVehicleToPlayer(Vehicle veh, int cost, bool instant = false)
        {
            if (veh.Exists())
            {
                bool recoveredVehicle = RecoveredVehList.Contains(veh);

                if (IncomingVehicles.Count > 0)
                {
                    // If the vehicle is already incoming, we remove the driver
                    foreach (IncomingVehicle incoming in IncomingVehicles)
                    {
                        if (incoming.vehicle == veh)
                        {
                            incoming.driver.Task.ClearAllImmediately();
                            incoming.driver.IsPersistent = false;
                            incoming.driver.Delete();

                            if (!incoming.recovered)
                                RemoveRecoverBlip(incoming.vehicle);
                            else
                                incoming.vehicle.Repair();
                        }
                    }
                }

                if (instant || veh.Model.Hash == Game.GenerateHash("HYDRA"))
                {
                    if (veh.Model.IsBoat)
                        IncomingVehicle.BringBoat(veh, cost, recoveredVehicle);
                    else
                    {
                        EntityPosition pos = Utils.GetVehicleSpawnLocation(Game.Player.Character.Position);
                        veh.Position = pos.Position;
                        veh.Heading = pos.Heading;
                    }

                    // If it isn't a recovered vehicle, it doesn't have a Blip yet
                    if (!recoveredVehicle)
                    {
                        string key = Utils.GetVehicleIdentifier(veh);
                        if (BlipsToRemove.ContainsKey(key))
                        {
                            Blip oldBlip = BlipsToRemove[key];
                            if (oldBlip != null)
                                if (oldBlip.Exists()) oldBlip.Remove();
                            BlipsToRemove[key] = InsuranceManager.AddVehicleBlip(veh);
                        }
                        else
                            BlipsToRemove.Add(key, InsuranceManager.AddVehicleBlip(veh));
                    }
                }
                else
                {
                    if (veh.Model.IsCargobob || veh.Model.IsHelicopter)
                        IncomingVehicles.Add(IncomingVehicle.BringHelicopter(veh, cost, recoveredVehicle));
                    else if (veh.Model.IsPlane)
                        IncomingVehicles.Add(IncomingVehicle.BringPlane(veh, cost, recoveredVehicle));
                    else if (veh.Model.IsBoat)
                        IncomingVehicle.BringBoat(veh, cost, recoveredVehicle);
                    else
                        IncomingVehicles.Add(IncomingVehicle.BringVehicle(veh, cost, recoveredVehicle));

                    // If it isn't a recovered vehicle, it doesn't have a Blip yet
                    if (!recoveredVehicle)
                    {
                        string key = Utils.GetVehicleIdentifier(veh);
                        if (BlipsToRemove.ContainsKey(key))
                        {
                            Blip oldBlip = BlipsToRemove[key];
                            if (oldBlip != null)
                                if (oldBlip.Exists()) oldBlip.Remove();
                            BlipsToRemove[key] = InsuranceManager.AddVehicleBlip(veh);
                        }
                        else
                            BlipsToRemove.Add(key, InsuranceManager.AddVehicleBlip(veh));
                    }
                }
            }
            else
                Logger.Error("BringVehicleToPlayer - The vehicle doesn't exist!");
        }

        internal void CannotBringVehicle(IncomingVehicle incoming, int refund = 0)
        {
            SE.UI.DrawNotification("char_mp_mors_mutual", "MORS MUTUAL INSURANCE", T.GetString("BringVehicle"), T.GetString("NotifyBringVehicleCancel"));

            // Refund the player
            if (refund == 0)
                SE.Player.AddCashToPlayer(incoming.price);
            else
                SE.Player.AddCashToPlayer(refund + incoming.price);

            // Remove the driver
            incoming.driver.Delete();

            if (incoming.originalPosition.Position != Vector3.Zero)
            {
                // Remove Blip
                RemoveRecoverBlip(incoming.vehicle);

                if (!incoming.vehicle.IsDead)
                {
                    // Put the vehicle back in place
                    incoming.vehicle.Position = incoming.originalPosition.Position;
                    incoming.vehicle.Heading = incoming.originalPosition.Heading;
                    incoming.vehicle.EngineRunning = false;
                    incoming.vehicle.Repair();
                }
                IncomingVehicles.Remove(incoming);
            }
            else
            {
                if (!incoming.vehicle.IsDead)
                {
                    EntityPosition vehiclePos = InsuranceManager.GetVehicleRecoverNode(incoming.vehicle);
                    incoming.vehicle.Position = vehiclePos.Position;
                    incoming.vehicle.Heading = vehiclePos.Heading;
                    incoming.vehicle.EngineRunning = false;
                    incoming.vehicle.Repair();
                }
                else
                {
                    // Remove Blip
                    RemoveRecoverBlip(incoming.vehicle);
                }
                 IncomingVehicles.Remove(incoming);
            }
            
        }

        /// <summary>
        /// List all bringable vehicles on the map.
        /// Vehicle must be insured and the player must not be inside.
        /// </summary>
        /// <returns></returns>
        internal static List<Vehicle> GetBringableVehicles()
        {
            List<Vehicle> vehiclesToBring = new List<Vehicle>();
            vehiclesToBring.AddRange(RecoveredVehList);

            foreach (Vehicle insuredVehicle in InsuredVehList)
            {
                // Avoid including recovered vehicles
                if (!vehiclesToBring.Contains(insuredVehicle)) vehiclesToBring.Add(insuredVehicle);
            }

            // If the player is in a bringable vehicle, we don't list it
            if (Game.Player.Character.CurrentVehicle != null)
                if (vehiclesToBring.Contains(Game.Player.Character.CurrentVehicle))
                    vehiclesToBring.Remove(Game.Player.Character.CurrentVehicle);

            return vehiclesToBring;
        }




        /// <summary>
        /// When a vehicle is insured, it will automatically be added to the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="veh"></param>
        private void OnVehicleInsured(InsuranceManager sender, Vehicle veh)
        {
            if (!InsuredVehList.Contains(veh))
            {
                InsuredVehList.Add(veh);
                if (Config.PersistentVehicles) veh.IsPersistent = true;
            }
        }
        /// <summary>
        /// Removes the vehicle from the list when it isn't insured anymore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="veh"></param>
        private void OnVehicleCanceled(InsuranceManager sender, string vehID)
        {
            foreach (Vehicle veh in InsuredVehList)
            {
                if (Utils.GetVehicleIdentifier(veh) == vehID)
                {
                    InsuredVehList.Remove(veh);
                    if (Config.PersistentVehicles) veh.IsPersistent = false;
                    break;
                }
            } 
        }
        /// <summary>
        /// When a vehicle is recovered, we need to check if the player takes it.
        /// This way, we can remove Blip and set persistence.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="veh"></param>
        /// <param name="blip"></param>
        private void OnVehicleRecovered(InsuranceManager sender, Vehicle veh, Blip blip)
        {
            if (!RecoveredVehList.Contains(veh))
                RecoveredVehList.Add(veh);
            if (!BlipsToRemove.ContainsValue(blip) && !BlipsToRemove.ContainsKey(Utils.GetVehicleIdentifier(veh)))
                BlipsToRemove.Add(Utils.GetVehicleIdentifier(veh), blip);

            if (Config.PersistentVehicles) veh.IsPersistent = true;
        }


    }
}
