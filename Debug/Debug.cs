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
                SE.UI.DrawText("Insured: " + InsuranceManager.IsVehicleInsured(Utils.GetVehicleIdentifier(veh)).ToString(), 0, false, x, y, 0.4f, 255, 255, 255, 255);
            }
        }
    }
}
