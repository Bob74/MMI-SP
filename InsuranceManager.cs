using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Drawing;

using GTA;
using GTA.Native;
using GTA.Math;


namespace MMI_SP
{
    public class InsuranceManager
    {
        public delegate void VehicleIsNowInsured(InsuranceManager sender, Vehicle veh);
        public delegate void VehicleNoLongerInsured(InsuranceManager sender, string vehIdentifier);
        public delegate void VehicleRecovered(InsuranceManager sender, Vehicle veh, Blip blip);

        // InsuranceManager instance
        private static InsuranceManager _instance;
        public static InsuranceManager Instance { get => _instance; }

        public enum Multiplier { Insurance, Recover, Stolen };
        public enum SpawnNode { Vehicle, Helicopter, Plane, Boat };

        // Vehicle spawn nodes
        private readonly static List<EntityPosition> _spawnListVehicle = new List<EntityPosition> {
            new EntityPosition(new Vector3(-225.2716f, -1182.783f, 22.49698f), 2.3600f),
            new EntityPosition(new Vector3(-229.9406f, -1182.361f, 22.49209f), 6.1440f),
            new EntityPosition(new Vector3(-234.6615f, -1182.197f, 22.48984f), 355.5509f),
            new EntityPosition(new Vector3(-244.1168f, -1179.623f, 22.5177f), 308.1156f),
            new EntityPosition(new Vector3(-243.4413f, -1173.07f, 22.53329f), 271.4005f),
            new EntityPosition(new Vector3(-243.5148f, -1166.511f, 22.56954f), 242.3607f),
            new EntityPosition(new Vector3(-237.2427f, -1162.784f, 22.47172f), 183.7536f),
            new EntityPosition(new Vector3(-232.8058f, -1162.548f, 22.44885f), 182.2262f),
            new EntityPosition(new Vector3(-228.4865f, -1162.615f, 22.45386f), 181.4573f),
            new EntityPosition(new Vector3(-150.4142f, -1166.01f, 24.73805f), 177.0276f),
            new EntityPosition(new Vector3(-143.6111f, -1163.825f, 24.76486f), 160.3781f),
            new EntityPosition(new Vector3(-136.2873f, -1183.153f, 24.7363f), 78.20843f),
            new EntityPosition(new Vector3(-136.9411f, -1177.181f, 24.72224f), 102.267f),
            new EntityPosition(new Vector3(-246.5937f, -1150.561f, 22.62461f), 269.4836f),
            new EntityPosition(new Vector3(-238.7069f, -1150.786f, 22.62887f), 269.3971f),
            new EntityPosition(new Vector3(-232.8114f, -1150.434f, 22.54277f), 272.1211f),
            new EntityPosition(new Vector3(-211.5235f, -1150.303f, 22.55123f), 268.1985f),
            new EntityPosition(new Vector3(-198.5835f, -1150.331f, 22.54078f), 269.7671f)};

        private readonly static List<EntityPosition> _spawnListVehicleLong = new List<EntityPosition> {
            new EntityPosition(new Vector3(-157.9389f, -1162.761f, 24.11157f), 0.6600574f),
            new EntityPosition(new Vector3(-236.0531f, -1149.395f, 23.04231f), 269.1866f),
            new EntityPosition(new Vector3(-174.2821f, -1149.661f, 23.17635f), 269.3501f),
            new EntityPosition(new Vector3(-200.4261f, -1182.882f, 23.1067f), 90.51575f)};

        private readonly static List<EntityPosition> _spawnListHeli = new List<EntityPosition> {
            new EntityPosition(new Vector3(-746.6312f, -1432.797f, 4.71605f), 231.0658f),
            new EntityPosition(new Vector3(-763.4095f, -1453.074f, 4.722716f), 234.3286f),
            new EntityPosition(new Vector3(-746.3437f, -1469.839f, 4.718675f), 322.4937f),
            new EntityPosition(new Vector3(-721.1809f, -1473.602f, 4.717093f), 49.87125f),
            new EntityPosition(new Vector3(-700.242f, -1447.846f, 4.71675f), 53.22678f),
            new EntityPosition(new Vector3(-723.8517f, -1442.887f, 4.716637f), 139.5879f)};

        private readonly static List<EntityPosition> _spawnListPlane = new List<EntityPosition> {
            new EntityPosition(new Vector3(1638.067f, 3234.868f, 40.11113f), 103.8905f),
            new EntityPosition(new Vector3(1558.921f, 3155.603f, 40.23004f), 134.3105f),
            new EntityPosition(new Vector3(1430.566f, 3111.669f, 40.23326f), 103.7299f),
            new EntityPosition(new Vector3(2071.546f, 4786.328f, 40.79108f), 115.6482f)};

        private readonly static List<EntityPosition> _spawnListBoat = new List<EntityPosition> {
            new EntityPosition(new Vector3(-989.812f, -1395.955f, 0.3117422f), 197.2292f),
            new EntityPosition(new Vector3(-998.601f, -1400.204f, -0.01398028f), 200.5666f),
            new EntityPosition(new Vector3(-982.6636f, -1392.835f, -0.1012118f), 200.7435f),
            new EntityPosition(new Vector3(-973.8835f, -1389.073f, -0.07558359f), 196.9216f),
            new EntityPosition(new Vector3(-965.5593f, -1385.88f, 0.1165133f), 197.4063f),
            new EntityPosition(new Vector3(-955.8226f, -1383.237f, -0.06844078f), 199.7236f),
            new EntityPosition(new Vector3(-930.0321f, -1374.57f, 0.024976f), 196.3813f),
            new EntityPosition(new Vector3(-911.7566f, -1368.049f, 0.01486713f), 205.7693f),
            new EntityPosition(new Vector3(-845.9328f, -1362.563f, -0.1105901f), 287.3574f),
            new EntityPosition(new Vector3(-858.0338f, -1328.371f, -0.05082685f), 290.1963f),
            new EntityPosition(new Vector3(-897.3489f, -1336.532f, 0.0469994f), 115.3039f),
            new EntityPosition(new Vector3(-915.5237f, -1343.745f, 0.1156863f), 111.007f),
            new EntityPosition(new Vector3(-948.0477f, -1355.498f, 0.124268f), 106.5388f),
            new EntityPosition(new Vector3(-836.3871f, -1389.433f, 0.03498344f), 290.8258f),
            new EntityPosition(new Vector3(-785.4754f, -1440.365f, 0.07404654f), 322.9755f),
            new EntityPosition(new Vector3(-772.8375f, -1424.96f, 0.07902782f), 317.0082f),
            new EntityPosition(new Vector3(-774.7501f, -1385.55f, 0.09457491f), 50.25464f),
            new EntityPosition(new Vector3(-753.9382f, -1362.394f, -0.04268733f), 50.26961f),
            new EntityPosition(new Vector3(-724.7479f, -1327.435f, 0.02641343f), 52.78964f),
            new EntityPosition(new Vector3(-855.8542f, -1485.853f, 0.0313217f), 108.474f)};

        private readonly static List<EntityPosition> _spawnListMilitary = new List<EntityPosition> {
            new EntityPosition(new Vector3(-1594.426f, 3185.479f, 30.40495f), 147.6925f),
            new EntityPosition(new Vector3(-1603.479f, 3203.978f, 30.41406f), 171.5964f),
            new EntityPosition(new Vector3(-1615.621f, 3169.568f, 29.66991f), 223.9812f),
            new EntityPosition(new Vector3(-1580.501f, 3156.202f, 30.64534f), 154.0106f),
            new EntityPosition(new Vector3(-1565.606f, 3131.228f, 32.23048f), 142.0033f),
            new EntityPosition(new Vector3(-1630.897f, 2980.762f, 32.45866f), 251.8481f),
            new EntityPosition(new Vector3(-1565.328f, 3020.8f, 32.43408f), 121.0561f),
            new EntityPosition(new Vector3(-1668.656f, 3081.12f, 30.85717f), 231.5131f)};

        // Database file
        private readonly static string _dbFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\MMI\\db.xml";
        private XElement _dbFile; // Avoid loading the file for every request

        /// <summary>
        /// Raised when a vehicle is insured.
        /// Out: vehicle
        /// </summary>
        public event VehicleIsNowInsured Insured;
        protected virtual void Raise_VehicleIsNowInsured(InsuranceManager sender, Vehicle veh) { Insured?.Invoke(this, veh); }

        /// <summary>
        /// Raised when a vehicle is no longer insured.
        /// Out: Vehicle
        /// </summary>
        public event VehicleNoLongerInsured Canceled;
        protected virtual void Raise_VehicleNoLongerInsured(InsuranceManager sender, string vehID) { Canceled?.Invoke(this, vehID); }

        /// <summary>
        /// Raised when a vehicle has been recovered.
        /// out: Vehicle + Blip
        /// </summary>
        public event VehicleRecovered Recovered;
        protected virtual void Raise_VehicleHasBeenRecovered(InsuranceManager sender, Vehicle veh, Blip blip) { Recovered?.Invoke(this, veh, blip); }



        public InsuranceManager()
        {
            _instance = this;

            if (!File.Exists(_dbFilePath))
                CreateDBFile();
            try
            {
                _dbFile = XElement.Load(_dbFilePath);
            }
            catch (Exception e)
            {
                Logger.Info("Error: InsuranceManager - Cannot load database file. " + e.Message);
            }
        }

        /// <summary>
        /// Creates the database file.
        /// </summary>
        private static void CreateDBFile()
        {
            FileInfo file = new FileInfo(_dbFilePath);
            
            if (!file.Directory.Exists)
                Directory.CreateDirectory(file.Directory.FullName);

            XDocument doc =
                new XDocument(
                    new XDeclaration("1.0", Encoding.UTF8.HeaderName, String.Empty),
                    new XComment("Xml Document")
                );
            XElement main = new XElement("MMI");
            doc.Add(main);
            doc.Save(file.FullName);
        }
        /// <summary>
        /// Saves the current database to a file.
        /// </summary>
        private void SaveDBFile()
        {
            if (!File.Exists(_dbFilePath))
                CreateDBFile();
            _dbFile.Save(_dbFilePath);
        }


        /// <summary>
        /// Add a vehicle to the database
        /// </summary>
        /// <param name="veh"></param>
        private void AddVehicleToDB(Vehicle veh)
        {
            XElement section;
            if (_dbFile.Element("Vehicles") == null)
            {
                section = new XElement("Vehicles");
                _dbFile.Add(section);
            }
            else
                section = _dbFile.Element("Vehicles");

            section.Add(GenerateVehicleSection(veh));
            SaveDBFile();

            Insured(this, veh);
        }

        /// <summary>
        /// Removes a vehicle from the database.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        private void RemoveVehicleFromDB(string vehIdentifier)
        {
            if (_dbFile.Element("Vehicles") != null)
                if (_dbFile.Element("Vehicles").Element(vehIdentifier) != null)
                    _dbFile.Element("Vehicles").Element(vehIdentifier).Remove();
                else
                    Logger.Info("Error: RemoveVehicleFromDB - Cannot find the section " + vehIdentifier);
            else
                Logger.Info("Error: RemoveVehicleFromDB - Cannot find the section Vehicles");

            SaveDBFile();

            Raise_VehicleNoLongerInsured(this, vehIdentifier);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////// INTERNAL METHODS ///////////////////////////////////////////////////////


        internal string ChangeVehicleLicensePlate(string vehIdentifier, string newPlate)
        {
            if (_dbFile.Element("Vehicles") != null)
            {
                XElement vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                if (vehicleSection != null)
                {
                    vehicleSection = vehicleSection.Element("Plate");
                    if (vehicleSection != null)
                    {
                        if (vehicleSection.Element("NumberPlate") != null)
                        {
                            vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                            vehicleSection = vehicleSection.Element("General");
                            if (vehicleSection != null)
                            {
                                // Removes the license plate number. ie: keep only Franklin-12343456545
                                string modelHash = vehicleSection.Element("Model").Value;
                                if (vehIdentifier.IndexOf(modelHash) > 0)
                                {
                                    string newVehID = vehIdentifier.Remove(vehIdentifier.IndexOf(modelHash) + modelHash.Length);
                                    newVehID += newPlate.Replace(" ", "_");

                                    XElement newSection = new XElement(newVehID);
                                    vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                                    newSection.Add(vehicleSection.Elements());

                                    newSection.Element("Plate").Element("NumberPlate").SetValue(newPlate);

                                    vehicleSection.Remove();
                                    _dbFile.Element("Vehicles").Add(newSection);

                                    SaveDBFile();

                                    return newVehID;
                                }
                                else
                                    Logger.Info("Error: ChangeVehicleLicensePlate - Unable to find the modelHash for the vehicle " + vehIdentifier + ".");
                            }
                            else
                                Logger.Info("Error: ChangeVehicleLicensePlate - General section is missing for the vehicle " + vehIdentifier + ".");
                        }
                        else
                            Logger.Info("Error: ChangeVehicleLicensePlate - NumberPlate section is missing for the vehicle " + vehIdentifier + ".");
                    }
                    else
                        Logger.Info("Error: ChangeVehicleLicensePlate - Plate section is missing for the vehicle " + vehIdentifier + ".");
                }
                else
                    Logger.Info("Error: ChangeVehicleLicensePlate - The vehicle identifier cannot be found: " + vehIdentifier);
            }

            return "";
        }

        /// <summary>
        /// Return the license plate of the vehicle.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <returns></returns>
        internal string GetVehicleLicensePlate(string vehIdentifier)
        {
            if (_dbFile.Element("Vehicles") != null)
            {
                XElement vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                if (vehicleSection != null)
                {
                    vehicleSection = vehicleSection.Element("Plate");
                    if (vehicleSection != null)
                    {
                        if (vehicleSection.Element("NumberPlate") != null)
                            return vehicleSection.Element("NumberPlate").Value;
                    }
                }
                else
                    Logger.Info("Error: GetVehicleLicensePlate - The vehicle identifier cannot be found: " + vehIdentifier);
            }

            return "";
        }
        /// <summary>
        /// Return the vehicle friendly name from the insurance database.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <param name="showClassName"></param>
        /// <returns></returns>
        internal string GetVehicleFriendlyName(string vehIdentifier, bool showClassName = true)
        {
            if (_dbFile.Element("Vehicles") != null)
            {
                XElement vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                if (vehicleSection != null)
                {
                    try
                    {
                        string friendlyname = "";

                        int modelHash = Int32.Parse(vehicleSection.Element("General").Element("Model").Value);

                        int modelClass = Function.Call<int>(Hash.GET_VEHICLE_CLASS_FROM_NAME, modelHash);
                        string modelClassName = Game.GetGXTEntry("VEH_CLASS_" + modelClass.ToString());

                        string numberPlate = vehicleSection.Element("Plate").Element("NumberPlate").Value;

                        string model = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, modelHash);
                        string modelName = Game.GetGXTEntry(model);

                        if (showClassName)
                            friendlyname = modelName + " - " + numberPlate + " (" + modelClassName + ")";
                        else
                            friendlyname = modelName + " - " + numberPlate;

                        return friendlyname;
                    }
                    catch (Exception e)
                    {
                        Logger.Info("Error: GetVehicleFriendlyName - Cannot convert model hash to int: " + e.Message);
                    }
                }
                else
                    Logger.Info("Error: GetVehicleFriendlyName - The vehicle identifier cannot be found: " + vehIdentifier);
            }

            return "Unknown";
        }

        /// <summary>
        /// Return the model name from the insurance database.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <returns></returns>
        internal string GetVehicleModelName(string vehIdentifier)
        {
            if (_dbFile.Element("Vehicles") != null)
            {
                XElement vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                if (vehicleSection != null)
                {
                    try
                    {
                        int modelHash = Int32.Parse(vehicleSection.Element("General").Element("Model").Value);
                        string model = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, modelHash);
                        string modelName = Game.GetGXTEntry(model);

                        return modelName;
                    }
                    catch (Exception e)
                    {
                        Logger.Info("Error: GetVehicleFriendlyName - Cannot convert model hash to int: " + e.Message);
                    }
                }
                else
                    Logger.Info("Error: GetVehicleFriendlyName - The vehicle identifier cannot be found: " + vehIdentifier);
            }

            return "Unknown";
        }

        internal int GetVehicleInsuranceCost(string vehIdentifier, Multiplier mode)
        {
            if (_dbFile.Element("Vehicles") != null)
            {
                XElement vehicleSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                if (vehicleSection != null)
                {
                    if (vehicleSection.Element("General") != null)
                    {
                        int cost = Int32.Parse(vehicleSection.Element("General").Element("Cost").Value);
                        float multiplier = 1.0f;

                        if (mode == Multiplier.Insurance) multiplier = Config.InsuranceMult;
                        else if (mode == Multiplier.Recover) multiplier = Config.RecoverMult;
                        else if (mode == Multiplier.Stolen) multiplier = Config.StolenMult;

                        return (int)(cost * multiplier);
                    }
                        
                    else
                        Logger.Info("Error: GetVehicleInsuranceCost - General section is missing for the vehicle " + vehIdentifier + ".");
                }
                else
                    Logger.Info("Error: GetVehicleInsuranceCost - Vehicle " + vehIdentifier + " not found in database.");

            }
            else
                Logger.Info("Error: GetVehicleInsuranceCost - No vehicles found in database file.");

            return 0;
        }

        /// <summary>
        /// Insure the vehicle.
        /// </summary>
        /// <param name="veh"></param>
        internal void InsureVehicle(Vehicle veh)
        {
            AddVehicleToDB(veh);
        }
        /// <summary>
        /// Remove the vehicle from the database.
        /// </summary>
        /// <param name="veh"></param>
        internal void CancelVehicle(string vehIdentifier)
        {
            // Remove the persistence of the vehicle and eventual Blip
            foreach (Vehicle veh in World.GetAllVehicles())
            {
                if (Tools.GetVehicleIdentifier(veh) == vehIdentifier)
                {
                    if (veh.CurrentBlip != null) veh.CurrentBlip.Remove();
                    veh.IsPersistent = false;
                }
            }

            RemoveVehicleFromDB(vehIdentifier);
        }

        /// <summary>
        /// Recover the requested vehicle.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        internal void RecoverVehicle(string vehIdentifier)
        {
            // Creates the vehicle
            Vehicle veh = CreateVehicleFromDB(vehIdentifier);

            if (veh != null)
                if (veh.Exists())
                {
                    // Make sure the vehicle is persistent
                    veh.IsPersistent = true;
                    
                    // Add a Blip to the vehicle
                    Blip vehBlip = AddVehicleBlip(veh);

                    // Update DB status
                    SetVehicleStatusToDB(Tools.GetVehicleIdentifier(veh), "Alive");

                    Raise_VehicleHasBeenRecovered(this, veh, vehBlip);
                }
                else
                {
                    Logger.Info("Error : RecoverVehicle - The vehicle doesn't exist.");
                }
            else
                Logger.Info("Error: RecoverVehicle - The vehicle value is null.");
        }

        /// <summary>
        /// Return the owner's name.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <returns></returns>
        internal string GetVehicleOwner(string vehIdentifier)
        {
            string owner = "";

            if (_dbFile.Element("Vehicles") != null)
                if (_dbFile.Element("Vehicles").Element(vehIdentifier) != null)
                    if (_dbFile.Element("Vehicles").Element(vehIdentifier).Element("General") != null)
                        if (_dbFile.Element("Vehicles").Element(vehIdentifier).Element("General").Element("Owner") != null)
                            owner = _dbFile.Element("Vehicles").Element(vehIdentifier).Element("General").Element("Owner").Value;

            return owner;
        }
        /// <summary>
        /// Return True if the vehicle exist in the database.
        /// </summary>
        /// <param name="vehIdentifier">Use GetVehicleIdentifier(Vehicle) to get the identifier.</param>
        /// <returns></returns>
        internal bool IsVehicleInDB(Vehicle veh)
        {
            return IsVehicleInDB(Tools.GetVehicleIdentifier(veh));
        }
        internal bool IsVehicleInDB(string vehIdentifier)
        {
            bool output = false;

            if (_dbFile.Element("Vehicles") != null)
                if (_dbFile.Element("Vehicles").Element(vehIdentifier) != null)
                    output = true;

            return output;
        }
        /// <summary>
        /// Get the vehicle in database.
        /// </summary>
        /// <param name="dead"></param>
        /// <returns></returns>
        internal List<string> GetInsuredVehicles(string characterName, bool dead)
        {
            List<string> list = new List<string>();
            if (_dbFile.Element("Vehicles") != null)
            {
                XElement section = _dbFile.Element("Vehicles");
                foreach (XElement elem in section.Elements())
                {
                    if (elem.Element("General").Element("Owner").Value == characterName)
                    {
                        if (dead)
                        {
                            if (elem.Element("General").Element("Status").Value == "Dead")
                                list.Add(elem.Name.ToString());
                        }
                        else
                        {
                            if (elem.Element("General").Element("Status").Value == "Alive")
                                list.Add(elem.Name.ToString());
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Status is "Alive" or "Dead" depending of the state of vehicle.isDead
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <param name="status"></param>
        internal void SetVehicleStatusToDB(string vehIdentifier, string status)
        {
            if (_dbFile.Element("Vehicles") != null)
                if (_dbFile.Element("Vehicles").Element(vehIdentifier) != null)
                    if (_dbFile.Element("Vehicles").Element(vehIdentifier).Element("General") != null)
                    {
                        XElement vehSection = _dbFile.Element("Vehicles").Element(vehIdentifier).Element("General");
                        vehSection.Element("Status").SetValue(status);
                        SaveDBFile();
                    }
        }




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////// STATIC METHODS ////////////////////////////////////////////////////////
        /// <summary>
        /// Return the insurance cost of a vehicle.
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static int GetVehicleInsuranceCost(Vehicle veh, Multiplier mode = Multiplier.Insurance)
        {
            float multiplier = 1.0f;
            if (mode == Multiplier.Insurance) multiplier = Config.InsuranceMult;
            else if (mode == Multiplier.Recover) multiplier = Config.RecoverMult;
            else if (mode == Multiplier.Stolen) multiplier = Config.StolenMult;

            int price = 0;
            price += Price.GetVehicleModelPrice(veh);
            price += Price.GetVehicleSizePrice(veh);
            price += Price.GetVehicleModsPrice(veh);
            return (int)(price * multiplier);
        }

        /// <summary>
        /// Static version of IsVehicleInDB
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <returns></returns>
        public static bool IsVehicleInsured(Vehicle veh)
        {
            return IsVehicleInsured(Tools.GetVehicleIdentifier(veh));
        }
        public static bool IsVehicleInsured(string vehIdentifier)
        {
            bool output = false;

            if (File.Exists(_dbFilePath))
            {
                XElement xdoc = XElement.Load(_dbFilePath);
                if (xdoc.Element("Vehicles") != null)
                    if (xdoc.Element("Vehicles").Element(vehIdentifier) != null)
                        output = true;
            }

            return output;
        }

        /// <summary>
        /// Return if the specified vehicle is insurable.
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static bool IsVehicleInsurable(Vehicle veh)
        {
            if (veh.IsAlive)
                if (!SE.Vehicle.IsPlayerOfficialVehicle(veh) && !veh.Model.IsTrain)
                    return true;
                else
                    return false;
            else
                return false;
        }

        public static Blip AddVehicleBlip(Vehicle veh)
        {
            // Add a Blip
            BlipSprite sprite = BlipSprite.PersonalVehicleCar;
            if (veh.Model.IsBike || veh.Model.IsBicycle) sprite = BlipSprite.PersonalVehicleBike;
            else if ((veh.Model.Hash == Game.GenerateHash("RHINO")) || veh.Model.Hash == Game.GenerateHash("KHANJALI")) sprite = BlipSprite.Tank;
            else if (veh.Model.IsHelicopter) sprite = BlipSprite.Helicopter;
            else if (veh.Model.IsPlane) sprite = BlipSprite.ArmsTraffickingAir;
            else if (veh.Model.IsBoat) sprite = BlipSprite.Speedboat;
            else if (veh.ClassType == VehicleClass.Military) sprite = BlipSprite.GunCar;

            Blip vehBlip = Function.Call<Blip>(Hash.ADD_BLIP_FOR_ENTITY, veh);
            vehBlip.Sprite = sprite;
            vehBlip.Color = BlipColor.White;
            vehBlip.Name = GetVehicleFriendlyName(veh);
            vehBlip.IsFlashing = true;

            if (sprite == BlipSprite.ArmsTraffickingAir ||
                sprite == BlipSprite.Tank ||
                sprite == BlipSprite.Speedboat ||
                sprite == BlipSprite.GunCar)
                vehBlip.Rotation = (int)veh.Rotation.Z;

            return vehBlip;
        }

        public static string GetVehicleFriendlyName(Vehicle veh, bool showClassName = true)
        {
           if (veh != null)
           {
                if (veh.Exists())
                {
                    string friendlyname;

                    int modelClass = Function.Call<int>(Hash.GET_VEHICLE_CLASS_FROM_NAME, veh.Model.Hash);
                    string modelClassName = Game.GetGXTEntry("VEH_CLASS_" + modelClass.ToString());

                    string model = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, veh.Model.Hash);
                    string modelName = Game.GetGXTEntry(model);

                    if (showClassName)
                        friendlyname = modelName + " - " + veh.NumberPlate + " (" + modelClassName + ")";
                    else
                        friendlyname = modelName + " - " + veh.NumberPlate;

                    return friendlyname;
                }
            }
            return "Unknown";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////// VEHICLE CREATION METHODS ///////////////////////////////////////////////////

        
        internal static EntityPosition GetVehicleRecoverNode(Vehicle veh)
        {
            List<EntityPosition> templist = new List<EntityPosition>();
            if (veh.Model.IsHelicopter || veh.Model.IsCargobob)
                templist.AddRange(_spawnListHeli);
            else if (veh.Model.IsPlane)
                templist.AddRange(_spawnListPlane);
            else if (veh.Model.IsBoat)
                templist.AddRange(_spawnListBoat);
            else if (veh.ClassType == VehicleClass.Military)
                templist.AddRange(_spawnListMilitary);
            else
            {
                Vector3 vehDimension = veh.Model.GetDimensions();
                if (vehDimension.Y > 7.4f)
                    templist.AddRange(_spawnListVehicleLong);
                else
                    templist.AddRange(_spawnListVehicle);
            }

            Random rnd = new Random();
            while (templist.Count > 0)
            {
                int n = rnd.Next(0, templist.Count - 1);
                EntityPosition spawn = templist[n];

                if (!Function.Call<bool>(Hash.IS_POINT_OBSCURED_BY_A_MISSION_ENTITY, spawn.Position.X, spawn.Position.Y, spawn.Position.Z, 5.0f, 5.0f, 5.0f, 0))
                    return spawn;
                else
                    templist.Remove(spawn);
            }

            // If no suitable spot has been found, we clear a random one
            EntityPosition clearSpawn = _spawnListVehicle[rnd.Next(0, _spawnListVehicle.Count - 1)];

            Function.Call(Hash.CLEAR_AREA_OF_VEHICLES, clearSpawn.Position.X, clearSpawn.Position.Y, clearSpawn.Position.Z, 1.0f, false, false, false, false, false);
            Vehicle missionEntity = World.GetClosestVehicle(clearSpawn.Position, 1.0f);

            // If it was a recovered car, we remove the Blip
            InsuranceObserver.RemoveRecoverBlip(missionEntity);
            missionEntity.IsPersistent = false;
            missionEntity.Delete();

            return clearSpawn;
        }

        /// <summary>
        /// Writes the vehicle stats in the database to be able to spawn it again.
        /// </summary>
        /// <param name="veh"></param>
        private XElement GenerateVehicleSection(Vehicle veh)
        {
            string vehIdentifier = Tools.GetVehicleIdentifier(veh);

            XElement vehSection = new XElement(vehIdentifier);

            // General
            XElement generalSection = new XElement("General");
            generalSection.Add(new XElement("Owner", SE.Player.GetCurrentCharacterName(true)));
            generalSection.Add(new XElement("Status", "Alive"));
            generalSection.Add(new XElement("Model", veh.Model.Hash.ToString()));
            generalSection.Add(new XElement("Cost", GetVehicleInsuranceCost(veh).ToString()));
            vehSection.Add(generalSection);

            // Plate
            XElement plateSection = new XElement("Plate");
            plateSection.Add(new XElement("NumberPlate", veh.NumberPlate));
            plateSection.Add(new XElement("NumberPlateType", (int)veh.NumberPlateType));
            vehSection.Add(plateSection);

            // Wheels
            XElement wheelsSection = new XElement("Wheels");
            wheelsSection.Add(new XElement("WheelType", veh.WheelType));
            vehSection.Add(wheelsSection);

            // Mods
            XElement modsSection = new XElement("Mods");

            if (Function.Call<int>(Hash.GET_NUM_MOD_KITS, veh) != 0)
            {
                foreach (VehicleMod mod in Enum.GetValues(typeof(VehicleMod)))
                {
                    XElement modElem = new XElement("Mod", new XAttribute("Name", mod));
                    modElem.SetValue(veh.GetMod(mod));

                    modsSection.Add(modElem);
                }
                foreach (VehicleToggleMod mod in Enum.GetValues(typeof(VehicleToggleMod)))
                {
                    XElement modElem = new XElement("ToggleMod", new XAttribute("Name", mod));
                    if (veh.IsToggleModOn(mod))
                        modElem.SetValue(true);
                    else
                        modElem.SetValue(false);

                    modsSection.Add(modElem);
                }
            }
            modsSection.Add(new XElement("FrontTiresCustom", Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, veh, 23)));
            modsSection.Add(new XElement("RearTiresCustom", Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, veh, 24)));
            modsSection.Add(new XElement("WindowTint", (int)veh.WindowTint));
            vehSection.Add(modsSection);

            // Tires
            XElement tiresSection = new XElement("Tires");
            try
            {
                tiresSection.Add(new XElement("TireSmokeColor", ColorTranslator.ToHtml(veh.TireSmokeColor)));
            }
            catch (Exception e)
            {
                tiresSection.Add(new XElement("TireSmokeColor", ColorTranslator.ToHtml(Color.White)));
                Logger.Info("Warning: GenerateVehicleSection - TireSmokeColor is wrong: " + e.Message);
            }
            tiresSection.Add(new XElement("CanTiresBurst", veh.CanTiresBurst));
            vehSection.Add(tiresSection);

            // Neons
            XElement neonsSection = new XElement("Neons");
            try
            {
                neonsSection.Add(new XElement("NeonLightsColor", ColorTranslator.ToHtml(veh.NeonLightsColor)));
            }
            catch (Exception e)
            {
                neonsSection.Add(new XElement("NeonLightsColor", ColorTranslator.ToHtml(Color.White)));
                Logger.Info("Warning: GenerateVehicleSection - NeonLightsColor is wrong: " + e.Message);
            }

            for (int i = 0; i < 4; i++)
                if (veh.IsNeonLightsOn((VehicleNeonLight)i))
                    neonsSection.Add(new XElement("VehicleNeonLight", i));

            vehSection.Add(neonsSection);

            // Colors
            XElement colorsSection = new XElement("Colors");
            colorsSection.Add(new XElement("IsPrimaryColorCustom", veh.IsPrimaryColorCustom));
            colorsSection.Add(new XElement("IsSecondaryColorCustom", veh.IsSecondaryColorCustom));
            colorsSection.Add(new XElement("PrimaryColor", veh.PrimaryColor));
            colorsSection.Add(new XElement("SecondaryColor", veh.SecondaryColor));
            colorsSection.Add(new XElement("PearlescentColor", veh.PearlescentColor));
            colorsSection.Add(new XElement("RimColor", veh.RimColor));
            colorsSection.Add(new XElement("ColorCombination", veh.ColorCombination));
            colorsSection.Add(new XElement("CustomPrimaryColor", ColorTranslator.ToHtml(veh.CustomPrimaryColor)));
            colorsSection.Add(new XElement("CustomSecondaryColor", ColorTranslator.ToHtml(veh.CustomSecondaryColor)));
            colorsSection.Add(new XElement("DashboardColor", veh.DashboardColor));
            colorsSection.Add(new XElement("TrimColor", veh.TrimColor));
            vehSection.Add(colorsSection);

            // Convertible
            if (veh.IsConvertible)
            {
                XElement convertibleSection = new XElement("Convertible");
                convertibleSection.Add(new XElement("ConvertibleRoofState", (veh.RoofState)));
                vehSection.Add(convertibleSection);
            }

            // Extra
            XElement extraSection = new XElement("Extra");
            for (int i = 1; i < 15; i++)
                if (veh.IsExtraOn(i))
                    extraSection.Add(new XElement("ID", i));
            vehSection.Add(extraSection);

            // Livery
            XElement liverySection = new XElement("Livery");
            liverySection.Add(new XElement("ID", veh.Livery));
            vehSection.Add(liverySection);

            if (SE.Vehicle.GetVehicleLivery2(veh) > 0)
            {
                XElement livery2Section = new XElement("Livery2");
                livery2Section.Add(new XElement("ID", SE.Vehicle.GetVehicleLivery2(veh)));
                vehSection.Add(livery2Section);
            }

            return vehSection;
        }

        /// <summary>
        /// Updates the vehicle in the DB.
        /// </summary>
        /// <param name="veh"></param>
        public void UpdateVehicleToDB(Vehicle veh)
        {
            string vehIdentifier = Tools.GetVehicleIdentifier(veh);

            if (_dbFile.Element("Vehicles") != null)
            {
                XElement vehSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                if (vehSection != null)
                {
                    XElement currentSection;

                    // General
                    currentSection = vehSection.Element("General");
                    if (currentSection != null)
                        currentSection.Element("Cost").SetValue(GetVehicleInsuranceCost(veh).ToString());

                    // Plate
                    currentSection = vehSection.Element("Plate");
                    if (currentSection != null)
                        if (currentSection.Element("NumberPlateType") != null)
                            currentSection.Element("NumberPlateType").SetValue((int)veh.NumberPlateType);
                        else
                            Logger.Info("Error: UpdateVehicleToDB - NumberPlateType not found.");

                    // Wheels
                    currentSection = vehSection.Element("Wheels");
                    if (currentSection != null)
                        if (currentSection.Element("WheelType") != null)
                            currentSection.Element("WheelType").SetValue(veh.WheelType);
                        else
                            Logger.Info("Error: UpdateVehicleToDB - WheelType not found.");

                    // Mods
                    currentSection = vehSection.Element("Mods");
                    if (currentSection != null)
                    {
                        currentSection.RemoveAll();

                        foreach (VehicleMod mod in Enum.GetValues(typeof(VehicleMod)))
                        {
                            XElement modElem = new XElement("Mod", new XAttribute("Name", mod));
                            modElem.SetValue(veh.GetMod(mod));

                            currentSection.Add(modElem);
                        }
                        foreach (VehicleToggleMod mod in Enum.GetValues(typeof(VehicleToggleMod)))
                        {
                            XElement modElem = new XElement("ToggleMod", new XAttribute("Name", mod));
                            if (veh.IsToggleModOn(mod))
                                modElem.SetValue(true);
                            else
                                modElem.SetValue(false);

                            currentSection.Add(modElem);
                        }
                        currentSection.Add(new XElement("FrontTiresCustom", Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, veh, 23)));
                        currentSection.Add(new XElement("RearTiresCustom", Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, veh, 24)));
                        currentSection.Add(new XElement("WindowTint", (int)veh.WindowTint));
                    }

                    // Tires
                    currentSection = vehSection.Element("Tires");
                    if (currentSection != null)
                    {
                        if (currentSection.Element("TireSmokeColor") != null)
                        {
                            try
                            {
                                currentSection.Element("TireSmokeColor").SetValue(ColorTranslator.ToHtml(veh.TireSmokeColor));
                            }
                            catch (Exception e)
                            {
                                currentSection.Element("TireSmokeColor").SetValue(ColorTranslator.ToHtml(Color.White));
                                Logger.Info("Warning: GenerateVehicleSection - TireSmokeColor is wrong: " + e.Message);
                            }
                        }
                        else
                            Logger.Info("Error: UpdateVehicleToDB - TireSmokeColor not found.");

                        if (currentSection.Element("CanTiresBurst") != null)
                            currentSection.Element("CanTiresBurst").SetValue(veh.CanTiresBurst);
                        else
                            Logger.Info("Error: UpdateVehicleToDB - CanTiresBurst not found.");
                    }

                    // Neons
                    currentSection = vehSection.Element("Neons");
                    if (currentSection != null)
                    {
                        currentSection.RemoveAll();
                        try
                        {
                            currentSection.Add(new XElement("NeonLightsColor", ColorTranslator.ToHtml(veh.NeonLightsColor)));
                        }
                        catch (Exception e)
                        {
                            currentSection.Add(new XElement("NeonLightsColor", ColorTranslator.ToHtml(Color.White)));
                            Logger.Info("Warning: GenerateVehicleSection - NeonLightsColor is wrong: " + e.Message);
                        }
                        
                        for (int i = 0; i < 4; i++)
                            if (veh.IsNeonLightsOn((VehicleNeonLight)i))
                                currentSection.Add(new XElement("VehicleNeonLight", i));
                    }

                    // Colors
                    currentSection = vehSection.Element("Colors");
                    if (currentSection != null)
                    {
                        if (currentSection.Element("IsPrimaryColorCustom") != null) currentSection.Element("IsPrimaryColorCustom").SetValue(veh.IsPrimaryColorCustom);
                        else Logger.Info("Error: UpdateVehicleToDB - IsPrimaryColorCustom not found.");

                        if (currentSection.Element("IsSecondaryColorCustom") != null) currentSection.Element("IsSecondaryColorCustom").SetValue(veh.IsSecondaryColorCustom);
                        else Logger.Info("Error: UpdateVehicleToDB - IsSecondaryColorCustom not found.");

                        if (currentSection.Element("PrimaryColor") != null) currentSection.Element("PrimaryColor").SetValue(veh.PrimaryColor);
                        else Logger.Info("Error: UpdateVehicleToDB - PrimaryColor not found.");

                        if (currentSection.Element("SecondaryColor") != null) currentSection.Element("SecondaryColor").SetValue(veh.SecondaryColor);
                        else Logger.Info("Error: UpdateVehicleToDB - SecondaryColor not found.");

                        if (currentSection.Element("PearlescentColor") != null) currentSection.Element("PearlescentColor").SetValue(veh.PearlescentColor);
                        else Logger.Info("Error: UpdateVehicleToDB - PearlescentColor not found.");

                        if (currentSection.Element("RimColor") != null) currentSection.Element("RimColor").SetValue(veh.RimColor);
                        else Logger.Info("Error: UpdateVehicleToDB - RimColor not found.");

                        if (currentSection.Element("ColorCombination") != null) currentSection.Element("ColorCombination").SetValue(veh.ColorCombination);
                        else Logger.Info("Error: UpdateVehicleToDB - ColorCombination not found.");

                        if (currentSection.Element("CustomPrimaryColor") != null) currentSection.Element("CustomPrimaryColor").SetValue(ColorTranslator.ToHtml(veh.CustomPrimaryColor));
                        else Logger.Info("Error: UpdateVehicleToDB - CustomPrimaryColor not found.");

                        if (currentSection.Element("CustomSecondaryColor") != null) currentSection.Element("CustomSecondaryColor").SetValue(ColorTranslator.ToHtml(veh.CustomSecondaryColor));
                        else Logger.Info("Error: UpdateVehicleToDB - CustomSecondaryColor not found.");

                        if (currentSection.Element("DashboardColor") != null) currentSection.Element("DashboardColor").SetValue(veh.DashboardColor);
                        else Logger.Info("Error: UpdateVehicleToDB - DashboardColor not found.");

                        if (currentSection.Element("TrimColor") != null) currentSection.Element("TrimColor").SetValue(veh.TrimColor);
                        else Logger.Info("Error: UpdateVehicleToDB - TrimColor not found.");

                    }

                    // Convertible
                    if (veh.IsConvertible)
                    {
                        currentSection = vehSection.Element("Convertible");

                        if (currentSection != null)
                            if (currentSection.Element("ConvertibleRoofState") != null)
                                currentSection.Element("ConvertibleRoofState").SetValue(veh.RoofState);
                            else
                                Logger.Info("Error: UpdateVehicleToDB - NeonLightsColor not found.");
                    }

                    // Extra
                    currentSection = vehSection.Element("Extra");
                    if (currentSection != null)
                    {
                        currentSection.RemoveAll();
                        for (int i = 1; i < 15; i++)
                            if (veh.IsExtraOn(i))
                                currentSection.Add(new XElement("ID", i));
                    }

                    // Livery
                    currentSection = vehSection.Element("Livery");
                    if (currentSection != null)
                        if (currentSection.Element("ID") != null)
                            currentSection.Element("ID").SetValue(veh.Livery);
                        else
                            Logger.Info("Error: UpdateVehicleToDB - Livery ID not found.");

                    if (SE.Vehicle.GetVehicleLivery2(veh) > 0)
                    {
                        currentSection = vehSection.Element("Livery2");
                        if (currentSection != null)
                            if (currentSection.Element("ID") != null)
                                currentSection.Element("ID").SetValue(SE.Vehicle.GetVehicleLivery2(veh));
                            else
                                Logger.Info("Error: UpdateVehicleToDB - Livery2 ID not found.");
                    }

                    // Saving file
                    SaveDBFile();
                }
                else
                {
                    Logger.Info("Error: UpdateVehicleToDB - Unable to find the current vehicle section in DB: " + vehIdentifier);
                }
            }
            else
            {
                Logger.Info("Error: UpdateVehicleToDB - The \"vehicles\" section doesn't exist in the DB file!");
            }
        }

        /// <summary>
        /// Creates an insured vehicle according to its data in the DB file.
        /// </summary>
        /// <param name="vehIdentifier"></param>
        /// <returns></returns>
        public Vehicle CreateVehicleFromDB(string vehIdentifier)
        {
            try
            {
                if (_dbFile.Element("Vehicles") != null)
                    if (_dbFile.Element("Vehicles").Element(vehIdentifier) != null)
                    {
                        XElement vehSection = _dbFile.Element("Vehicles").Element(vehIdentifier);
                        int modelHandle = Int32.Parse(vehSection.Element("General").Element("Model").Value);
                        Model model = new Model(modelHandle);

                        Vehicle veh = World.CreateVehicle(modelHandle, new Vector3(0f, 0f, 0f), 0f);
                        veh.IsPersistent = true;
                        Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, veh, true);
                        EntityPosition node = GetVehicleRecoverNode(veh);
                        veh.Position = node.Position;
                        veh.Heading = node.Heading;

                        // Plate
                        if (vehSection.Element("Plate") != null)
                        {
                            veh.NumberPlate = vehSection.Element("Plate").Element("NumberPlate").Value;
                            veh.NumberPlateType = (NumberPlateType)Int32.Parse(vehSection.Element("Plate").Element("NumberPlateType").Value);
                        }

                        // Wheels
                        if (vehSection.Element("Wheels") != null)
                        {
                            veh.WheelType = (VehicleWheelType)Enum.Parse(typeof(VehicleWheelType), vehSection.Element("Wheels").Element("WheelType").Value);
                        }

                        // Mods
                        if (vehSection.Element("Mods") != null)
                        {
                            veh.InstallModKit();

                            if (vehSection.Element("Mods").Elements("Mod") != null)
                            {
                                foreach (XElement mod in vehSection.Element("Mods").Elements("Mod"))
                                {
                                    bool variation = false;

                                    VehicleMod modType = (VehicleMod)Enum.Parse(typeof(VehicleMod), mod.Attribute("Name").Value);
                                    int modIndex = Int32.Parse(mod.Value);
                                    if (modType == VehicleMod.FrontWheels)
                                        variation = bool.Parse(vehSection.Element("Mods").Element("FrontTiresCustom").Value);
                                    else if (modType == VehicleMod.BackWheels)
                                        variation = bool.Parse(vehSection.Element("Mods").Element("RearTiresCustom").Value);

                                    veh.SetMod(modType, modIndex, variation);
                                }
                                veh.WindowTint = (VehicleWindowTint)Int32.Parse(vehSection.Element("Mods").Element("WindowTint").Value);
                            }
                            if (vehSection.Element("Mods").Elements("ToggleMod") != null)
                            {
                                foreach (XElement toggleMod in vehSection.Element("Mods").Elements("ToggleMod"))
                                {
                                    VehicleToggleMod modType = (VehicleToggleMod)Enum.Parse(typeof(VehicleToggleMod), toggleMod.Attribute("Name").Value);
                                    bool enabled = bool.Parse(toggleMod.Value);
                                    veh.ToggleMod(modType, enabled);
                                }
                            }
                        }
 
                        // Tires
                        if (vehSection.Element("Tires") != null)
                        {
                            veh.TireSmokeColor = ColorTranslator.FromHtml(vehSection.Element("Tires").Element("TireSmokeColor").Value);
                            veh.CanTiresBurst = bool.Parse(vehSection.Element("Tires").Element("CanTiresBurst").Value);
                        }

                        // Neons
                        if (vehSection.Element("Neons") != null)
                        {
                            if (vehSection.Element("Neons").Element("NeonLightsColor") != null)
                                veh.NeonLightsColor = ColorTranslator.FromHtml(vehSection.Element("Neons").Element("NeonLightsColor").Value);
                            else
                                Logger.Info("Error: CreateVehicleFromDB - Cannot find element NeonLightsColor");

                            foreach (XElement neon in vehSection.Element("Neons").Elements("VehicleNeonLight"))
                                veh.SetNeonLightsOn((VehicleNeonLight)Int32.Parse(neon.Value), true);
                        }

                        // Colors
                        XElement colorSection = vehSection.Element("Colors");
                        if (colorSection != null)
                        {
                            bool IsPrimaryColorCustom = bool.Parse(colorSection.Element("IsPrimaryColorCustom").Value);
                            bool IsSecondaryColorCustom = bool.Parse(colorSection.Element("IsSecondaryColorCustom").Value);
                            veh.ClearCustomPrimaryColor();
                            veh.ClearCustomSecondaryColor();

                            if (IsPrimaryColorCustom)
                                veh.CustomPrimaryColor = ColorTranslator.FromHtml(colorSection.Element("CustomPrimaryColor").Value);
                            if (IsSecondaryColorCustom)
                                veh.CustomSecondaryColor = ColorTranslator.FromHtml(colorSection.Element("CustomSecondaryColor").Value);

                            veh.PrimaryColor = (VehicleColor)Enum.Parse(typeof(VehicleColor), colorSection.Element("PrimaryColor").Value);
                            veh.SecondaryColor = (VehicleColor)Enum.Parse(typeof(VehicleColor), colorSection.Element("SecondaryColor").Value);
                            veh.PearlescentColor = (VehicleColor)Enum.Parse(typeof(VehicleColor), colorSection.Element("PearlescentColor").Value);
                            veh.RimColor = (VehicleColor)Enum.Parse(typeof(VehicleColor), colorSection.Element("RimColor").Value);
                            veh.DashboardColor = (VehicleColor)Enum.Parse(typeof(VehicleColor), colorSection.Element("DashboardColor").Value);
                            veh.TrimColor = (VehicleColor)Enum.Parse(typeof(VehicleColor), colorSection.Element("TrimColor").Value);
                        }

                        // Convertible
                        if (vehSection.Element("Convertible") != null)
                        {
                            if (veh.IsConvertible)
                                veh.RoofState = (VehicleRoofState)Enum.Parse(typeof(VehicleRoofState), vehSection.Element("Convertible").Element("ConvertibleRoofState").Value);
                        }

                        // Extra
                        if (vehSection.Element("Extra") != null)
                        {
                            // Remove All extras
                            for (int i = 1; i < 15; i++)
                                veh.ToggleExtra(i, false);

                            // Add the rights extras
                            foreach (XElement extra in vehSection.Element("Extra").Elements("ID"))
                                veh.ToggleExtra(Int32.Parse(extra.Value), true);
                        }

                        // Livery
                        if (vehSection.Element("Livery") != null)
                        {
                            veh.Livery = Int32.Parse(vehSection.Element("Livery").Element("ID").Value);
                        }
                        if (vehSection.Element("Livery2") != null)
                        {
                            SE.Vehicle.SetVehicleLivery2(veh, Int32.Parse(vehSection.Element("Livery2").Element("ID").Value));
                        }

                        return veh;
                    }
            }
            catch (Exception e)
            {
                Logger.Info("Error: CreateVehicleFromDB - " + e.Message);
            }
            return null;
        }
    }
}
