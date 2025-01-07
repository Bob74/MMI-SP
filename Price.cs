
using GTA;
using GTA.Math;

namespace MMI_SP
{
    static class Price
    {
        private static int GetVehicleClassPrice(Vehicle veh)
        {
            switch (veh.ClassType)
            {
                case VehicleClass.Boats:
                    return 5000;
                case VehicleClass.Commercial:
                    return 5000;
                case VehicleClass.Cycles:
                    return 500;
                case VehicleClass.Emergency:
                    return 10000;
                case VehicleClass.Helicopters:
                    return 10000;
                case VehicleClass.Industrial:
                    return 6000;
                case VehicleClass.Military:
                    return 25000;
                case VehicleClass.Planes:
                    return 5000;
                case VehicleClass.Service:
                    return 5000;
                case VehicleClass.Utility:
                    return 5000;
                case VehicleClass.Compacts:
                    return 5000;
                case VehicleClass.Sedans:
                    return 6000;
                case VehicleClass.SUVs:
                    return 8000;
                case VehicleClass.Coupes:
                    return 6500;
                case VehicleClass.Motorcycles:
                    return 6000;
                case VehicleClass.Muscle:
                    return 7500;
                case VehicleClass.SportsClassics:
                    return 18000;
                case VehicleClass.Sports:
                    return 9500;
                case VehicleClass.Super:
                    return 12000;
                case VehicleClass.OffRoad:
                    return 8500;
                case VehicleClass.Vans:
                    return 5500;
                default:
                    return 8000;
            }
        }

        public static int GetVehicleModelPrice(Vehicle veh)
        {
            int classPrice = GetVehicleClassPrice(veh);

            // Vehicle Model
            if (veh.Model.Hash == Game.GenerateHash("DUSTER"))
                classPrice += -4000;
            else if (veh.Model.Hash == Game.GenerateHash("RHINO") || veh.Model.Hash == Game.GenerateHash("KHANJALI"))
                classPrice += 25000;
            else if (veh.Model.Hash == Game.GenerateHash("HYDRA") || veh.Model.Hash == Game.GenerateHash("LAZER"))
                classPrice += 20000;
            else if (veh.Model.Hash == Game.GenerateHash("BUZZARD"))    // Armed buzzard
                classPrice += 8000;
            else if (veh.Model.Hash == Game.GenerateHash("ANNIHILATOR"))
                classPrice += 15000;
            else if (veh.Model.Hash == Game.GenerateHash("DUMP"))
                classPrice += 15000;
            else if (veh.Model.Hash == Game.GenerateHash("REBEL"))
                classPrice += -2000;
            else if (veh.Model.Hash == Game.GenerateHash("SURFER2"))
                classPrice += -2000;
            else if (veh.Model.Hash == Game.GenerateHash("TORNADO3") || veh.Model.Hash == Game.GenerateHash("TORNADO4"))
                classPrice += -13000;
            else if (veh.Model.Hash == Game.GenerateHash("TORNADO6"))
                classPrice += -11000;
            else if (veh.Model.Hash == Game.GenerateHash("PEYOTE"))
                classPrice += -6000;
            else if (veh.Model.Hash == Game.GenerateHash("TRACTOR"))
                classPrice += -4200;
            else if (veh.Model.Hash == Game.GenerateHash("VOODOO2"))
                classPrice += -4000;
            else if (veh.Model.Hash == Game.GenerateHash("RUINER2"))    // Ruiner 2000
                classPrice += 13000;
            else if (veh.Model.Hash == Game.GenerateHash("RUINER3"))    // Wreck
                classPrice += -6000;
            else if (veh.Model.Hash == Game.GenerateHash("DELUXO"))
                classPrice += 13000;
            else if (veh.Model.Hash == Game.GenerateHash("EMPEROR2"))
                classPrice += -3000;
            else if (veh.Model.Hash == Game.GenerateHash("BFINJECTION"))
                classPrice += -4000;
            else if (veh.Model.Hash == Game.GenerateHash("JOURNEY"))
                classPrice += -4000;
            else if (veh.Model.Hash == Game.GenerateHash("RATBIKE"))
                classPrice += -3000;
            else if (veh.Model.Hash == Game.GenerateHash("RATLOADER"))
                classPrice += -3000;

            if (classPrice < 0) return 0;
            return classPrice;
        }

        public static int GetVehicleSizePrice(Vehicle veh)
        {
            int value = 0;
            Vector3 rearBottomLeft = veh.Model.Dimensions.rearBottomLeft;
            Vector3 frontTopRight = veh.Model.Dimensions.frontTopRight;

            float diagonal = rearBottomLeft.DistanceTo(frontTopRight);

            if (veh.Model.IsPlane)
            {
                // Vehicle Length
                if (diagonal < 10f)
                    value += 1000;
                else if (diagonal < 20f)
                    value += 6000;
                else if (diagonal < 30f)
                    value += 10000;
                else if (diagonal < 50f)
                    value += 13000;
                else if (diagonal < 70f)
                    value += 20000;
                else
                    value += 30000;
            }
            else if (veh.Model.IsBoat)
            {
                // Vehicle Length
                if (diagonal < 5f)
                    value += -4000;
                else if (diagonal < 10f)
                    value += 3000;
                else
                    value += 10000;
            }
            else if (veh.Model.IsBike || veh.Model.IsBicycle)
            {
                return 0;
            }
            else if (veh.Model.IsQuadBike)
            {
                return 0;
            }
            else if (veh.Model.IsHelicopter)
            {
                return 0;
            }
            else if (veh.Model.IsCargobob)
            {
                return 5000;
            }
            else
            {
                // Vehicle Length
                if (diagonal < 3f)
                    value += -4000;
                else if (diagonal < 5f)
                    value += 0;
                else if (diagonal < 8f)
                    value += 700;
                else if (diagonal < 10f)
                    value += 1500;
                else if (diagonal < 15f)
                    value += 3000;
                else
                    value += 10000;

                // Vehicle Width (overprice Dump and Tanks)
                if (diagonal < 1.5f)
                    value += -2500;
                else if (diagonal < 3.5f)
                    value += 2000;
                else if (diagonal < 7f)
                    value += 6000;
                else
                    value += 10000;
            }

            return value;
        }

        public static int GetVehicleModsPrice(Vehicle veh)
        {
            int value = 0;

            if (veh.Mods[VehicleToggleModType.Turbo].IsInstalled)
                value += 2000;
            if (veh.Mods[VehicleToggleModType.TireSmoke].IsInstalled)
                value += 485;
            if (veh.Mods[VehicleToggleModType.XenonHeadlights].IsInstalled)
                value += 960;
            if (veh.Mods.WheelType == VehicleWheelType.HighEnd)
                value += 200;
            if (veh.Mods.WheelType == VehicleWheelType.Sport)
                value += 120;
            if (veh.Mods.WheelType == VehicleWheelType.Tuner)
                value += 100;
            if (veh.Mods[VehicleModType.Armor].Index > -1)
                value += 500;
            if (veh.Mods[VehicleModType.Brakes].Index > -1)
                value += 500;
            if (veh.Mods[VehicleModType.Engine].Index > -1)
                value += 720;
            if (veh.Mods[VehicleModType.Transmission].Index > -1)
                value += 630;
            if (veh.Mods.WindowTint == VehicleWindowTint.LightSmoke)
                value += 170;
            if (veh.Mods.WindowTint == VehicleWindowTint.DarkSmoke)
                value += 275;
            if (veh.Mods.WindowTint == VehicleWindowTint.Limo)
                value += 300;
            if (veh.Mods.WindowTint == VehicleWindowTint.PureBlack)
                value += 355;
            if (veh.Mods.WindowTint == VehicleWindowTint.Green)
                value += 355;
            if (veh.Mods.IsPrimaryColorCustom)
                value += 700;
            if (veh.Mods.IsSecondaryColorCustom)
                value += 500;
            if (veh.IsConvertible)
                value += 525;
            if (!veh.CanTiresBurst)
                value += 810;
            if (veh.Mods.Livery > -1)
                value += 500;

            return value;
        }

    }
}
