using GTA.Math;

namespace MMI_SP
{
    internal class EntityPosition
    {
        public Vector3 Position;
        public float Heading;

        public EntityPosition(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }
    }
}
