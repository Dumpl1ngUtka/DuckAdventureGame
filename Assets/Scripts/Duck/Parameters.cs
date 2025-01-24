using UnityEngine;

namespace Duck
{
    [CreateAssetMenu(menuName = "Duck/Parameters")]
    public class Parameters : ScriptableObject
    {
        [Header("Ground Ray")]
        public float DistanceToFloor = 0.6f;
        public LayerMask GroundLayers;
        [Header("Move")]
        public float MaxSpeedMinRange = 3f;
        public float MaxSpeedMaxRange = 7f;
        public float MaxSpeedDistance = 10f;
        public float AccelerationPower = 20f;
        public float MaxAcceleration = 100f;
        public AnimationCurve AccelerationFromDot;
        public int StopRotationDegree = 25;
        public float MaxRotationSpeed = 120f;
        [Header("Spring")] 
        public float SpringStrength = 2000f;
        public float SpringDamper = 100f;
        public float RotationSpringStrength = 100f;
        public float RotationSpringDamper = 20f;
    }
}
