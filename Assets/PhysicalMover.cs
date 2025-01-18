using UnityEngine;

namespace Duck
{
    public class PhysicalMover : MonoBehaviour
    {
        [SerializeField] private float _distanceToFloor;
        [SerializeField] private LayerMask _groundLayers;
        [Header("Spring")]
        [SerializeField] private float _springStrength;
        [SerializeField] private float _springDamper;        
        [SerializeField] private float _rotationSpringStrength;
        [SerializeField] private float _rotationSpringDamper;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var isOnGround = IsOnGround(out var hit);
            if (isOnGround)
            {
                Floating(hit);
            }
            RotationStabilization();
        }

        private void Floating(RaycastHit hit)
        {
            var velocity = _rigidbody.velocity;
            var rayDirection = transform.TransformDirection(-Vector3.up);

            var otherVelocity = Vector3.zero;
            var hitbody = hit.rigidbody;
            if (hitbody != null)
                otherVelocity = hitbody.velocity;

            var rayDirVelocity = Vector3.Dot(rayDirection, velocity);
            var otherDirVelocity = Vector3.Dot(rayDirection, otherVelocity);

            var relVel = rayDirVelocity - otherDirVelocity;
            var deltaX = hit.distance - _distanceToFloor;
            var springForce = (deltaX * _springStrength) - (relVel * _springDamper);
            _rigidbody.AddForce(rayDirection * springForce);

            if (hitbody != null)
                hitbody.AddForceAtPosition(rayDirection * -springForce, hit.point);
        }

        private void RotationStabilization()
        {
            var currentRotation = transform.rotation;
            var toGoal = Quaternion.Inverse(currentRotation);

            toGoal.ToAngleAxis(out var rotDegrees, out var rotAxis);
            rotAxis.Normalize();

            var rotRadians = rotDegrees * Mathf.Deg2Rad;

            _rigidbody.AddTorque((rotAxis * (rotRadians * _rotationSpringStrength)) - (_rigidbody.angularVelocity * _rotationSpringDamper));

        }

        private bool IsOnGround(out RaycastHit hit)
        {
            return Physics.Raycast(transform.position, -Vector3.up, out hit, 1.5f, _groundLayers);
        }
    }
}