using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;

using GTA;
using GTA.Native;
using GTA.Math;
using System.Drawing;
using System.Linq;

namespace MMI_SP.Common
{

    internal static class Utils
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetCurrentMethod(int offset = 0)
        {
            var methodInfo = new StackTrace().GetFrame(1 + offset).GetMethod();
            var className = methodInfo.ReflectedType.Name;
            
            return $"{ className }.{ methodInfo.Name }";
        }

        internal static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        internal static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }

        internal static class Player
        {
            /// <summary>
            /// Return the current Ped type.
            /// 0 = Michael
            /// 1 = Franklin
            /// 2 = Trevor
            /// </summary>
            /// <returns>Character's ID.</returns>
            public static int GetCurrentCharacterID()
            {
                switch ((uint)Game.Player.Character.Model.Hash)
                {
                    case (uint)PedHash.Michael:
                        return 0;
                    case (uint)PedHash.Franklin:
                        return 1;
                    case (uint)PedHash.Trevor:
                        return 2;
                    default: return -1;
                }
            }

            /// <summary>
            /// Return the character's name.
            /// </summary>
            /// <param name="full">Return first name and last name.</param>
            /// <returns>Character's name</returns>
            public static string GetCurrentCharacterName(bool full = false)
            {
                string lastname = "";

                switch (GetCurrentCharacterID())
                {
                    case 0:
                        if (full)
                            lastname = " De Santa";
                        return "Michael" + lastname;
                    case 1:
                        if (full)
                            lastname = " Clinton";
                        return "Franklin" + lastname;
                    case 2:
                        if (full)
                            lastname = " Philips";
                        return "Trevor" + lastname;
                    default:
                        return "Unknown";
                }
            }

            /// <summary>
            /// Add cash to the current player. Use negative value to remove cash.
            /// </summary>
            /// <param name="value"></param>
            /// <returns>Return false if the player doesn't have enought money.</returns>
            internal static bool AddCashToPlayer(int value)
            {
                if (value == 0) return true;

                int newValue = Game.Player.Money + value;

                if (newValue >= 0)
                {
                    Game.Player.Money += value;
                    return true;
                }
                return false;
            }
        }

        internal static class Vehicle
        {
            /// <summary>
            /// Return the length of the vehicle.
            /// </summary>
            /// <param name="veh"></param>
            /// <returns></returns>
            internal static float GetVehicleLength(GTA.Vehicle veh)
            {
                Vector3 pos1 = veh.Model.Dimensions.rearBottomLeft;
                Vector3 pos2 = veh.Model.Dimensions.frontTopRight;
                return Math.Abs(pos1.Y) + Math.Abs(pos2.Y);
            }

            /// <summary>
            /// Return the unique identifier of the vehicle.
            /// </summary>
            /// <param name="veh"></param>
            /// <returns></returns>
            internal static string GetVehicleIdentifier(GTA.Vehicle veh)
            {
                string vehIdentifier = Utils.Player.GetCurrentCharacterName() + veh.Model.Hash.ToString() + veh.Mods.LicensePlate;
                vehIdentifier = vehIdentifier.Replace(" ", "_");
                return vehIdentifier;
            }

            internal static EntityPosition GetVehicleSpawnLocation(Vector3 position)
            {
                for (int index = 0; index < 22; ++index)
                {
                    OutputArgument outUnk = new OutputArgument();
                    OutputArgument outPosition = new OutputArgument();
                    OutputArgument outHeading = new OutputArgument();

                    Function.Call(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_WITH_HEADING, position.X, position.Y, position.Z, index, outPosition, outHeading, outUnk, 9, 3.0, 2.5);
                    Vector3 newPos = outPosition.GetResult<Vector3>();
                    float newHeading = outHeading.GetResult<float>();

                    if (!Function.Call<bool>(Hash.IS_POINT_OBSCURED_BY_A_MISSION_ENTITY, newPos.X, newPos.Y, newPos.Z, 5.0f, 5.0f, 5.0f, 0))
                    {
                        return new EntityPosition(newPos, newHeading);
                    }
                }

                return new EntityPosition(position, 0f);
            }

            /// <summary>
            /// Check if the vehicle is the player "official" vehicle (the one with the colored blip).
            /// </summary>
            /// <param name="veh">Vehicle to check</param>
            /// <returns>True if the vehicle is an official player vehicle</returns>
            internal static bool IsPlayerOfficialVehicle(GTA.Vehicle veh)
            {
                // Michael
                if ((VehicleHash)veh.Model.Hash == VehicleHash.Tailgater && veh.Mods.LicensePlate == "5MDS003 ")
                    return true;

                // Franklin
                if (((VehicleHash)veh.Model.Hash == VehicleHash.Buffalo2 && veh.Mods.LicensePlate == " FC1988 ") ||
                    ((VehicleHash)veh.Model.Hash == VehicleHash.Bagger && veh.Mods.LicensePlate == "  FC88  "))
                    return true;

                // Trevor
                if ((VehicleHash)veh.Model.Hash == VehicleHash.Bodhi2 && veh.Mods.LicensePlate == "BETTY 32")
                    return true;

                return false;
            }

            /// <summary>
            /// Generate a random number plate.
            /// GTA V number plate format: 00AAA000
            /// </summary>
            /// <returns>Random plate number</returns>
            internal static string GetRandomNumberPlate()
            {
                string final = "";

                Random rnd = new Random();

                int num = rnd.Next(0, 99);
                final += num.ToString("00");
                rnd = new Random(Game.GameTime);

                final += (char)rnd.Next(65, 90);
                final += (char)rnd.Next(65, 90);
                final += (char)rnd.Next(65, 90);

                num = rnd.Next(0, 999);
                final += num.ToString("000");

                return final;
            }

            /// <summary>
            /// Check if the plate number is valide. A valid plate number has a maximum of 8 characters containing only digits, letters from A to Z and white spaces.
            /// </summary>
            /// <param name="plateNumber">The license plate number to check.</param>
            /// <returns>True if the number is valid</returns>
            public static bool IsValidPlateNumber(string plateNumber)
            {
                if (plateNumber.Length <= 8)
                {
                    if (plateNumber.Contains(" ")) plateNumber = plateNumber.Replace(" ", "");
                    return !plateNumber.Any(ch => !Char.IsLetterOrDigit(ch));
                }
                else return false;
            }

            /// <summary>
            /// Returns a comprehensive name for the vehicle.
            /// </summary>
            /// <param name="veh">Vehicle</param>
            /// <param name="showClassName">Set to true to show the class name</param>
            /// <returns>[Model of the vehicle] - [Plate's number] ([Class name of the vehicle])</returns>
            public static string GetVehicleFriendlyName(GTA.Vehicle veh, bool showClassName = true)
            {
                VehicleClass modelClass = GTA.Vehicle.GetModelClass(veh.Model.Hash);
                string modelClassName = Game.GetLocalizedString(GTA.Vehicle.GetClassDisplayName(modelClass));
                string modelName = Game.GetLocalizedString(GTA.Vehicle.GetModelDisplayName(veh.Model.Hash));

                if (showClassName)
                {
                    return modelName + " - " + veh.Mods.LicensePlate + " (" + modelClassName + ")";
                }
                else
                {
                    return modelName + " - " + veh.Mods.LicensePlate;
                }
            }

            /// <summary>
            /// Number of livery2 available for the vehicle.
            /// Livery2 know usage is the roof of the TORNADO5 (Benny's custom)
            /// </summary>
            /// <param name="veh">Vehicle</param>
            /// <returns>Number of livery</returns>
            public static int GetVehicleLivery2Count(GTA.Vehicle veh)
            {
                return Function.Call<int>((Hash)0x5ECB40269053C0D4, veh);
            }

            /// <summary>
            /// Get the current index of livery2 for the vehicle.
            /// Livery2 know usage is the roof of the TORNADO5 (Benny's custom)
            /// </summary>
            /// <param name="veh">Vehicle</param>
            /// <returns>Current livery</returns>
            public static int GetVehicleLivery2(GTA.Vehicle veh)
            {
                return Function.Call<int>((Hash)0x60190048C0764A26, veh);
            }

            /// <summary>
            /// Set the current index of livery2 for the vehicle.
            /// Livery2 known usage is the roof of the TORNADO5 (Benny's custom)
            /// </summary>
            /// <param name="veh">Vehicle</param>
            /// <param name="liveryNumber">Livery ID to set to the vehicle</param>
            public static void SetVehicleLivery2(GTA.Vehicle veh, int liveryNumber)
            {
                Function.Call((Hash)0xA6D3A8750DC73270, veh, liveryNumber);
            }
        }

        internal static class Phone
        {
            /// <summary>
            /// Return the name of the sound set of the current character's phone.
            /// </summary>
            /// <returns>Name of the sound set</returns>
            internal static string GetPhoneSoundSet()
            {
                switch ((uint)Game.Player.Character.Model.Hash)
                {
                    case (uint)PedHash.Michael:
                        return "Phone_SoundSet_Michael";
                    case (uint)PedHash.Franklin:
                        return "Phone_SoundSet_Franklin";
                    case (uint)PedHash.Trevor:
                        return "Phone_SoundSet_Trevor";
                    default:
                        return "Phone_SoundSet_Default";
                }
            }
        }

        internal static class Screen
        {
            /// <summary>
            /// Wait the duration time and hide the ui.
            /// </summary>
            /// <param name="duration"></param>
            internal static void WaitAndhideUI(int duration)
            {
                int timer = Game.GameTime + duration;
                do
                {
                    Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
                    Script.Yield();
                } while (timer >= Game.GameTime);
            }
        }

        internal static class UI
        {
            public static void DrawTexture(string fileName, float x, float y, Color color)
            {
                // Get the texture properties
                Image sprite = Image.FromFile(fileName);

                // Draw
                GTA.UI.CustomSprite a = new GTA.UI.CustomSprite(fileName, new Size(sprite.Width, sprite.Height), new PointF(x, y), color, 0.0f);
                a.Draw();
            }

        }

    }
}