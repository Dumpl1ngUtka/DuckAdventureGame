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
        [SerializeField] private Vector3 _targetRotation;
        [SerializeField] private Vector3 _moveVector;
        [Header("Move")]
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _maxAcceleration;
        [SerializeField] private AnimationCurve _accelerationFromDot;
        private Rigidbody _rigidbody;
        private Vector3 _goalVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var moveVec = Vector3.zero;
            if (Input.GetKey("w"))
                moveVec += Vector3.forward;
            if (Input.GetKey("s"))
                moveVec -= Vector3.forward;
            if (Input.GetKey("a"))
                moveVec -= Vector3.right;
            if (Input.GetKey("d"))
                moveVec += Vector3.right;
            _moveVector = moveVec;
        }

        private void FixedUpdate()
        {
            var isOnGround = IsOnGround(out var hit);
            if (isOnGround)
            {
                Floating(hit);
                HorizontalMove();
                RotationStabilization();
            }
        }

        private void HorizontalMove()
        {
            var move = _moveVector;

            var velocityDot = Vector3.Dot(move, _rigidbody.velocity);
            var acceleration = _acceleration * _accelerationFromDot.Evaluate(velocityDot);

            var velocity = move * _maxSpeed;
            _goalVelocity = Vector3.MoveTowards(_goalVelocity, velocity, acceleration * Time.fixedDeltaTime);

            var neededAccel = (_goalVelocity - _rigidbody.velocity) / Time.fixedDeltaTime;
            neededAccel = Vector3.ClampMagnitude(neededAccel, _maxAcceleration);
            _rigidbody.AddForce(neededAccel * _rigidbody.mass);
        }

        private void Floating(RaycastHit hit)
        {
            var velocity = _rigidbody.velocity;
            var rayDirection = -Vector3.up;

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
            var targetRot = Quaternion.Euler(_targetRotation);
            var toGoal = targetRot * Quaternion.Inverse(currentRotation);

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