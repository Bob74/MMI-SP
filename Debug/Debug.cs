using System.Drawing;

using GTA;

using MMI_SP.Common;

namespace MMI_SP
{
    public static class Debug
    {
        private static readonly GTA.UI.TextElement debugTextTitle = new GTA.UI.TextElement("Last Vehicle", new PointF(0.0f, 0.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehLastHandle = new GTA.UI.TextElement("Last Handle: 0", new PointF(0.0f, 10.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextCurrentHandle = new GTA.UI.TextElement("Current Handle: 0", new PointF(0.0f, 20.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehDriveable = new GTA.UI.TextElement("Driveable: False", new PointF(0.0f, 30.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehPersistent = new GTA.UI.TextElement("Persistent: False", new PointF(0.0f, 40.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehModelHash = new GTA.UI.TextElement("Modelhash: False", new PointF(0.0f, 50.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehGameplayCamera = new GTA.UI.TextElement("GameplayCamera: False", new PointF(0.0f, 60.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehInsured = new GTA.UI.TextElement("Insured: False", new PointF(0.0f, 70.0f), 0.25f);
        private static readonly GTA.UI.TextElement debugTextVehPrice = new GTA.UI.TextElement("Price: 0", new PointF(0.0f, 80.0f), 0.25f);

        public static void ShowVehicleInfo(Vehicle veh)
        {
            Vehicle current = Game.Player.Character.CurrentVehicle;
            if (veh != null)
            {
                debugTextTitle.Draw();
                debugTextVehLastHandle.Caption = "Last Handle: " + veh.Handle.ToString();
                debugTextVehLastHandle.Draw();
                if (current != null)
                {
                    debugTextCurrentHandle.Caption = "Current Handle: " + Game.Player.Character.CurrentVehicle.Handle.ToString();
                    debugTextCurrentHandle.Draw();
                }
                debugTextVehDriveable.Caption = "Driveable: " + veh.IsDriveable.ToString();
                debugTextVehDriveable.Draw();
                debugTextVehPersistent.Caption = "Persistent: " + veh.IsPersistent.ToString();
                debugTextVehPersistent.Draw();
                debugTextVehModelHash.Caption = "ModelHash: " + veh.Model.Hash.ToString();
                debugTextVehModelHash.Draw();
                debugTextVehGameplayCamera.Caption = "GameplayCamera: " + GameplayCamera.IsRendering;
                debugTextVehGameplayCamera.Draw();
                debugTextVehInsured.Caption = "Insured: " + InsuranceManager.IsVehicleInsured(Utils.Vehicle.GetVehicleIdentifier(veh)).ToString();
                debugTextVehInsured.Draw();
                debugTextVehPrice.Caption = "Price: " + InsuranceManager.GetVehicleInsuranceCost(veh).ToString();
                debugTextVehPrice.Draw();
            }
        }
    }
}
