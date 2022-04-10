using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;

using GTA;
using GTA.Native;
using GTA.Math;

namespace MMI_SP
{

    internal static class Tools
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetCurrentMethod(int offset = 0)
        {
            var methodInfo = new StackTrace().GetFrame(1 + offset).GetMethod();
            var clasName = methodInfo.ReflectedType.Name;
            
            return $"{ clasName }.{ methodInfo.Name }";
        }

        internal static void ShowVehicleInfo(Vehicle veh, float x = 0.825f, float y = 0.65f)
        {
            Vehicle current = Game.Player.Character.CurrentVehicle;
            if (veh != null)
            {
                SE.UI.DrawText("Last Vehicle", 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                SE.UI.DrawText("Last Handle: " + veh.Handle.ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                if (current != null)
                {
                    SE.UI.DrawText("Current Handle: " + Game.Player.Character.CurrentVehicle.Handle.ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
                    y += 0.025f;
                }
                SE.UI.DrawText("Driveable: " + veh.IsDriveable.ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                SE.UI.DrawText("Persistent: " + veh.IsPersistent.ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                SE.UI.DrawText("MissionEntity: " + Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, veh), 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                SE.UI.DrawText("ModelHash: " + veh.Model.Hash.ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                SE.UI.DrawText("GameplayCamera: " + GameplayCamera.IsRendering, 0, false, x, y, 0.4f, 255, 255, 255, 255);
                y += 0.025f;
                SE.UI.DrawText("Insured: " + InsuranceManager.IsVehicleInsured(GetVehicleIdentifier(veh)).ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
            }
        }

        /// <summary>
        /// Allow notifications even if SHVDN-Extender is not installed (self check of initialization).
        /// </summary>
        /// <param name="picture"></param>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        /// <param name="message"></param>
        internal static void ShowNotification(string picture, string title, string subtitle, string message)
        {
            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, picture, false);
            while (!Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, picture))
            {
                Script.Yield();
            }

            Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, message);
            Function.Call(Hash._SET_NOTIFICATION_MESSAGE, picture, picture, false, 4, title, subtitle);
            Function.Call(Hash._DRAW_NOTIFICATION, false, true);
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

        internal enum VisualCVersion
        {
            Visual_2017,
            Visual_2015,
            Visual_2013,
            Visual_2012,
            Visual_2010,
            Visual_2008,
            Visual_2005,
        }

        private static readonly Dictionary<VisualCVersion, Version> visualCVersion = new Dictionary<VisualCVersion, Version>
        {
            { VisualCVersion.Visual_2017, new Version("14.1.0.0")},
            { VisualCVersion.Visual_2015, new Version("14.0.0.0")},
            { VisualCVersion.Visual_2013, new Version("12.0.0.0")},
            { VisualCVersion.Visual_2012, new Version("11.0.0.0")},
            { VisualCVersion.Visual_2010, new Version("10.0.0.0")},
            { VisualCVersion.Visual_2008, new Version("9.0.0.0")},
            { VisualCVersion.Visual_2005, new Version("8.0.0.0")}
        };

        internal static bool IsVisualCVersionHigherOrEqual(VisualCVersion visualC)
        {
            Version targetVersion = visualCVersion[visualC];

            string[] filters = { "msvcp*.dll", "msvcr*.dll" };
            List<string> files = new List<string>();

            foreach (string filter in filters)
                files.AddRange(Directory.GetFiles(Environment.SystemDirectory, filter));

            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(file);

                if (new Version(fileVersion.ProductVersion).CompareTo(targetVersion) >= 0)
                    return true;
            }

            return false;
        }


        internal static Version GetNETFrameworkVersion()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Version") != null)
                {
                    return new Version((string)ndpKey.GetValue("Version"));
                }
                else
                {
                    if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    {
                        int releaseKey = (int)ndpKey.GetValue("Release");

                        if (releaseKey >= 461308)
                        {
                            return new Version("4.7.1.0");
                        }
                        else if (releaseKey >= 460798)
                        {
                            return new Version("4.7.0.0");
                        }
                        else
                        {
                            return new Version("4.0.0.0");
                        }
                    }
                    else
                    {
                        return new Version("0.0.0.0");
                    }
                }
            }
        }

        /// <summary>
        /// Return the unique identifier of the vehicle.
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        internal static string GetVehicleIdentifier(Vehicle veh)
        {
            string vehIdentifier = SE.Player.GetCurrentCharacterName() + veh.Model.Hash.ToString() + veh.NumberPlate;
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
                    return new EntityPosition(newPos, newHeading);

            }

            return new EntityPosition(position, 0f);
        }

    }
}