using UnityEngine;

namespace Duck
{
    public class PhysicalMover : MonoBehaviour
    {
        [SerializeField] private float _distanceToFloor;
        [SerializeField] private LayerMask _groundLayers;
        [SerializeField] private float _maxSpeedMinRange = 3f;
        [SerializeField] private float _maxSpeedMaxRange = 7f;
        [SerializeField] private float _maxSpeedDistance = 10f;
        [Header("Spring")] [SerializeField] private float _springStrength;
        [SerializeField] private float _springDamper;
        [SerializeField] private float _rotationSpringStrength;
        [SerializeField] private float _rotationSpringDamper;
        [SerializeField] private Vector3 _targetRotation;
        [SerializeField] private Vector3 _moveVector;
        [Header("Move")] [SerializeField] private float _maxSpeed;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _maxAcceleration;
        [SerializeField] private AnimationCurve _accelerationFromDot;
        [SerializeField] private int _stopRotationDegree = 25;
        [SerializeField] private float _maxRotationSpeed = 120f;
        private Rigidbody _rigidbody;
        private float _currentMaxSpeed = 0f;
        private Vector3 _goalVelocity;
        private float _speed = 0f;
        private float _rotationFraction = 0f;
        
        private bool _isNeedToMove => Direction != Vector3.zero;
        private bool _isNeedToRotate => _isNeedToMove || RotationDirection != Vector3.zero;
        #region Direction

        private Vector3 _direction;
        private float _directionTimer;

        private Vector3 Direction
        {
            get => _direction;
            set
            {
                _directionTimer = 0.5f;
                _direction = value;
            }
        }

        private Vector3 RotationDirection;

        #endregion

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (_directionTimer > 0f)
                _directionTimer -= Time.deltaTime;
            else
                Direction = Vector3.zero;

            Rotate();
            SetSpeed();
        }
        
        private void SetSpeed()
        {
            if (!_isNeedToMove)
                _currentMaxSpeed = 0f;

            var delta = Vector3.Angle(transform.forward, Direction);
            _currentMaxSpeed = delta < _stopRotationDegree ? _maxSpeed : Mathf.Lerp(_maxSpeed, 0, (delta - _stopRotationDegree) / 90);

            _speed = Mathf.MoveTowards(_speed, _currentMaxSpeed, _acceleration * Time.deltaTime);
        }

        public void SetMoveDirection(Vector3 directoion)
        {
            _maxSpeed = Mathf.Lerp(_maxSpeedMinRange, _maxSpeedMaxRange, directoion.magnitude / _maxSpeedDistance);
            Direction = directoion.normalized;
        }

        private void FixedUpdate()
        {
            var isOnGround = IsOnGround(out var hit);
            if (!isOnGround) return;
            Floating(hit);
            HorizontalMove();
            RotationStabilization();
        }

        private void HorizontalMove()
        {
            var move = Direction;

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
            if (hitbody)
                otherVelocity = hitbody.velocity;

            var rayDirVelocity = Vector3.Dot(rayDirection, velocity);
            var otherDirVelocity = Vector3.Dot(rayDirection, otherVelocity);

            var relVel = rayDirVelocity - otherDirVelocity;
            var deltaX = hit.distance - _distanceToFloor;
            var springForce = (deltaX * _springStrength) - (relVel * _springDamper);
            _rigidbody.AddForce(rayDirection * springForce);

            if (hitbody)
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

            _rigidbody.AddTorque((rotAxis * (rotRadians * _rotationSpringStrength)) -
                                 (_rigidbody.angularVelocity * _rotationSpringDamper));
        }

        private bool IsOnGround(out RaycastHit hit)
        {
            return Physics.Raycast(transform.position, -Vector3.up, out hit, _distanceToFloor * 2, _groundLayers);
        }
        
        private void Rotate()
        {
            var delta = Vector3.SignedAngle(transform.forward, Direction, Vector3.up);
            _rotationFraction = Mathf.Abs(delta) / 180;
            var newPivotRotation = _targetRotation + new Vector3(0, delta, 0) * (_maxRotationSpeed * Time.deltaTime);
            if (newPivotRotation.y >= 180)
                newPivotRotation.y -= 360;
            else if (newPivotRotation.y < -180)
                newPivotRotation.y += 360;
            _targetRotation = newPivotRotation;
        }
    }
}