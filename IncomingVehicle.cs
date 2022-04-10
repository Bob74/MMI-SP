using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;
using GTA.Native;

namespace MMI_SP
{
    class IncomingVehicle
    {
        public Vehicle vehicle;
        public Ped driver;
        public Vector3 destination;
        public int calledTime;
        public int price;
        public bool recovered;
        public object[] additional;
        public bool hasReachedDestination = false;
        public EntityPosition originalPosition = new EntityPosition(Vector3.Zero, 0f);

        private static readonly List<PedHash> _drivers = new List<PedHash> { PedHash.Car3Guy2, PedHash.Xmech01SMY, PedHash.Autoshop01SMM, PedHash.Autoshop02SMM };
        public static List<PedHash> Drivers => _drivers;

        public IncomingVehicle(Vehicle veh, Ped ped, Vector3 dest, int cost, bool isRecovered, object[] add = null)
        {
            vehicle = veh;
            driver = ped;
            destination = dest;
            calledTime = Game.GameTime;
            price = cost;
            recovered = isRecovered;
            additional = add;
        }
        public IncomingVehicle(Vehicle veh, Ped ped, Vector3 dest, int cost, bool isRecovered, Vector3 originPos, float originHeading, object[] add = null)
        {
            vehicle = veh;
            driver = ped;
            destination = dest;
            calledTime = Game.GameTime;
            price = cost;
            recovered = isRecovered;
            originalPosition.Position = originPos;
            originalPosition.Heading = originHeading;
            additional = add;
        }

        public static IncomingVehicle BringHelicopter(Vehicle veh, int cost, bool recoveredVehicle)
        {
            float zOffset = 80f;
            Vector3 startPosition = Game.Player.Character.Position;
            do {
                startPosition = startPosition.Around(InsuranceObserver.BringVehicleRadius);
            } while (Game.Player.Character.Position.DistanceTo(startPosition) < (int)(InsuranceObserver.BringVehicleRadius * 0.95));

            startPosition.Z += World.GetGroundHeight(startPosition) + zOffset;
            veh.Position = startPosition;
            veh.Heading = (startPosition - Game.Player.Character.Position).ToHeading();
            veh.PreviouslyOwnedByPlayer = true;
            veh.EngineRunning = true;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, veh);

            Vector3 destination = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0f, 1.5f, -1f));

            Ped driver = CreateDriver(veh);
            Function.Call(Hash.TASK_HELI_MISSION, driver, veh, 0, Game.Player.Character, destination.X, destination.Y, destination.Z, 20, 30f, 15f, (destination - veh.Position).ToHeading(), -1, -1, -1, 32);

            return new IncomingVehicle(veh, driver, destination, cost, recoveredVehicle);
        }

        public static IncomingVehicle BringPlane(Vehicle veh, int cost, bool recoveredVehicle)
        {
            float zOffset = 80f;
            Vector3 startPosition = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0f, -5 * InsuranceObserver.BringVehicleRadius, 0f));
            if ((World.GetGroundHeight(startPosition) + zOffset) < Game.Player.Character.Position.Z)
                startPosition.Z = Game.Player.Character.Position.Z + zOffset;
            else
                startPosition.Z = World.GetGroundHeight(startPosition) + 200f;

            veh.Position = startPosition;
            veh.Heading = Game.Player.Character.Heading;
            veh.PreviouslyOwnedByPlayer = true;
            veh.EngineRunning = true;

            veh.ApplyForceRelative(new Vector3(0f, 20f, 0f));
            
            Vector3 runwayStartPoint = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0f, -110f, 0f));
            Vector3 runwayEndPoint = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0f, -40.0f, 0f));

            Ped driver = CreateDriver(veh);
            Function.Call(Hash.TASK_PLANE_LAND, driver, veh, runwayStartPoint.X, runwayStartPoint.Y, runwayStartPoint.Z, runwayEndPoint.X, runwayEndPoint.Y, runwayEndPoint.Z);

            return new IncomingVehicle(veh, driver, runwayEndPoint, cost, recoveredVehicle);
        }

        public static void BringBoat(Vehicle veh, int cost, bool recoveredVehicle)
        {
            Vector3 coords = Game.Player.Character.Position;
            Vector3 nodePos;
            float nodeHeading;

            OutputArgument pos = new OutputArgument();
            OutputArgument heading = new OutputArgument();
            Function.Call(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, coords.X, coords.Y, coords.Z, pos, heading, 3, 3.0f, 0);
            nodePos = pos.GetResult<Vector3>();
            nodeHeading = heading.GetResult<float>();

            veh.Position = nodePos;
            veh.Heading = nodeHeading;
            veh.PreviouslyOwnedByPlayer = true;
        }

        public static IncomingVehicle BringVehicle(Vehicle veh, int cost, bool recoveredVehicle)
        {
            Vector3 startPosition = Game.Player.Character.Position;
            do {
                startPosition = startPosition.Around(InsuranceObserver.BringVehicleRadius);
            } while (Game.Player.Character.Position.DistanceTo(startPosition) < (int)(InsuranceObserver.BringVehicleRadius * 0.8));

            EntityPosition vehPos = Tools.GetVehicleSpawnLocation(startPosition);
            veh.Position = vehPos.Position;
            veh.Heading = vehPos.Heading;
            veh.PreviouslyOwnedByPlayer = true;
            veh.EngineRunning = true;

            Vector3 destination = Tools.GetVehicleSpawnLocation(Game.Player.Character.Position).Position;

            Ped driver = CreateDriver(veh);
            driver.Task.DriveTo(veh, destination, 0f, 10.0f, (int)DrivingStyle.IgnoreLights);

            return new IncomingVehicle(veh, driver, destination, cost, recoveredVehicle);
        }

        private static Ped CreateDriver(Vehicle vehicle)
        {
            Vector3 npcPos = vehicle.Position;
            npcPos.X += 5f;

            PedHash driverModel = Drivers[new Random().Next(0, Drivers.Count - 1)];

            Ped driver = World.CreatePed(driverModel, npcPos);

            // Stop the NPC from fleeing
            driver.BlockPermanentEvents = true;
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, driver, 0, 0);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, driver, 17, 1);

            driver.IsPersistent = true;
            driver.SetIntoVehicle(vehicle, VehicleSeat.Driver);

            return driver;
        }
    }
}
