using UnityEngine;

namespace Chapter6.Fluid
{
    public struct FluidParticle
    {
        public Vector2 position;
        public Vector2 acceleration;
        public Vector2 velocity;
        public float density;
        public float pressure;
    }
}
