using GTA.Math;

namespace MMI_SP
{
    internal class EntityPosition
    {
        public Vector3 position;
        public float heading;

        public EntityPosition(Vector3 pos, float h)
        {
            position = pos;
            heading = h;
        }
    }

}
