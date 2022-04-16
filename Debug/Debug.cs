using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using GTA;
using GTA.Native;
using GTA.Math;

using MMI_SP.Common;

namespace MMI_SP
{
    public static class Debug
    {

        public static int GetPhoneHandle()
        {
            var model = (uint)Game.Player.Character.Model.Hash;
            switch (model)
            {
                case (uint)PedHash.Michael:
                    return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_ifruit");
                case (uint)PedHash.Franklin:
                    return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_badger");
                case (uint)PedHash.Trevor:
                    return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_facade");
                default: return Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "cellphone_ifruit");
            }
        }

        public static void DisplayCallUI(int handle, string contactName = "Test contact", string picName = "CELL_300")
        {
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "SET_DATA_SLOT");
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 3);

            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "STRING");
            Function.Call(Hash._0x761B77454205A61D, contactName, -1);       //UI::_ADD_TEXT_COMPONENT_APP_TITLE
            Function.Call(Hash._END_TEXT_COMPONENT);

            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "CELL_2000");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, picName);
            Function.Call(Hash._END_TEXT_COMPONENT);

            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "STRING");
            Function.Call(Hash._0x761B77454205A61D, "DIALING...", -1);      //UI::_ADD_TEXT_COMPONENT_APP_TITLE
            Function.Call(Hash._END_TEXT_COMPONENT);

            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "DISPLAY_VIEW");
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 4);
            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);
        }


        public static void ShowVehicleInfo(Vehicle veh, float x = 0.825f, float y = 0.65f)
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
                SE.UI.DrawText("Assuré: " + InsuranceManager.IsVehicleInsured(Utils.GetVehicleIdentifier(veh)).ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
            }
        }
    }
}
