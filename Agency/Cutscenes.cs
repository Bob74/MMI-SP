using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Native;
using GTA.Math;


namespace MMI_SP.Agency
{
    public static class Cutscenes
    {
        /// <summary>
        /// Cutscene of the player entering the Agency.
        /// </summary>
        public static void EnteringAgency()
        {
            Vector3 agencyPosition = new Vector3(-825.7242f, -261.2752f, 37.0000f);
            Vector3 entranceCameraPos = new Vector3(-826.7672f, -255.3226f, 40.54334f);
            Vector3 entranceCameraTarget = new Vector3(-825.814f, -265.1871f, 37.62714f);
            Vector3 entrancePlayerPos = new Vector3(-822.528f, -260.00f, 35.79341f);
            Vector3 playerTarget = agencyPosition;
            playerTarget.Z = 40.0f;
            float playerHeading = 130.3831f;
            int walkDuration = 2500;

            // Creates the camera
            Camera cam = World.CreateCamera(entranceCameraPos, new Vector3(0.0f, 0.0f, 0.0f), GameplayCamera.FieldOfView);
            cam.PointAt(entranceCameraTarget);

            // Character walks
            Game.Player.Character.Weapons.Select(WeaponHash.Unarmed, true);
            Game.Player.Character.Position = entrancePlayerPos;
            Game.Player.Character.Heading = playerHeading;
            Game.Player.Character.Task.LookAt(playerTarget, walkDuration);
            Function.Call(Hash.SIMULATE_PLAYER_INPUT_GAIT, Game.Player, 1.0f, walkDuration, 1.0f, 1, 0);

            // See through the camera
            World.RenderingCamera = cam;

            // Wait until the character has walked to the doors
            SE.UI.WaitAndhideUI(walkDuration - 1000);

            // Hide the view
            Game.FadeScreenOut(1000);
            SE.UI.WaitAndhideUI(1000);

            // Destroys the camera
            World.RenderingCamera = null;
            cam.IsActive = false;
            cam.Destroy();
        }
        /// <summary>
        /// Cutscene of the player leaving the Agency.
        /// </summary>
        public static void LeavingAgency()
        {
            float playerHeading = 305.54f;
            int walkDuration = 2000;

            // Character walks
            Game.Player.Character.Heading = playerHeading;
            GameplayCamera.RelativeHeading = 0.0f;
            GameplayCamera.RelativePitch = 0.0f;

            Function.Call(Hash.SIMULATE_PLAYER_INPUT_GAIT, Game.Player, 1.0f, walkDuration, 1.0f, 1, 0);

            // Show view
            Game.FadeScreenIn(1000);
        }
    }
}
