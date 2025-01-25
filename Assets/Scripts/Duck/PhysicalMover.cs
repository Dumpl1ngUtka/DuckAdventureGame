using UnityEngine;
using UnityEngine.Serialization;

namespace Duck
{
    public class PhysicalMover : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        private Quaternion _targetRotation;
        private Vector3 _moveVector;
        private float _maxSpeed;
        private float _currentMaxSpeed = 0f;
        private Vector3 _goalVelocity;
        private float _speed = 0f;
        private float _rotationFraction = 0f;
        private Parameters _parameters;
        
        private bool _isNeedToMove => Direction != Vector3.zero;
        
        public float MaxSpeed => _maxSpeed;
        public bool IsOnGround
        {
            get; private set;
        }
        
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
        #endregion

        public void Init(Duck duck)
        {
            _parameters = duck.Parameters;
        }

        private void Update()
        {
            if (_directionTimer > 0f)
                _directionTimer -= Time.deltaTime;
            else
                Direction = Vector3.zero;

            SetTargetRotation();
            SetSpeed();
        }
        
        private void SetSpeed()
        {
            if (!_isNeedToMove)
                _currentMaxSpeed = 0f;

            var delta = Vector3.Angle(_rigidbody.transform.forward, Direction);
            _currentMaxSpeed = delta < _parameters.StopRotationDegree ? _maxSpeed : Mathf.Lerp(_maxSpeed, 0, (delta - _parameters.StopRotationDegree) / 90);

            _speed = Mathf.MoveTowards(_speed, _currentMaxSpeed, _parameters.AccelerationPower * Time.deltaTime);
        }

        public void SetMoveDirection(Vector3 directoion)
        {
            _maxSpeed = Mathf.Lerp(_parameters.MaxSpeedMinRange, _parameters.MaxSpeedMaxRange, directoion.magnitude / _parameters.MaxSpeedDistance);
            Direction = directoion.normalized;
        }

        private void FixedUpdate()
        {
            IsOnGround = CheckGround(out var hit);
            
            if (!IsOnGround) return;
            
            Floating(hit);
            HorizontalMove();
            RotationStabilization();
        }

        private void HorizontalMove()
        {
            var move = Direction;

            var velocityDot = Vector3.Dot(move, _rigidbody.velocity);
            var acceleration = _parameters.AccelerationPower * _parameters.AccelerationFromDot.Evaluate(velocityDot);

            var velocity = move * _speed;
            _goalVelocity = Vector3.MoveTowards(_goalVelocity, velocity, acceleration * Time.fixedDeltaTime);

            var neededAccel = (_goalVelocity - _rigidbody.velocity) / Time.fixedDeltaTime;
            neededAccel = Vector3.ClampMagnitude(neededAccel, _parameters.MaxAcceleration);
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
            var deltaX = hit.distance - _parameters.DistanceToFloor;
            var springForce = (deltaX * _parameters.SpringStrength) - (relVel * _parameters.SpringDamper);
            _rigidbody.AddForce(rayDirection * springForce);

            if (hitbody)
                hitbody.AddForceAtPosition(rayDirection * -springForce, hit.point);
        }

        private void RotationStabilization()
        {
            var toGoal = _targetRotation * Quaternion.Inverse(transform.rotation);

            toGoal.ToAngleAxis(out var rotDegrees, out var rotAxis);
            rotAxis.Normalize();

            if (rotDegrees > 180f)
                rotDegrees -= 360f;
            
            var rotRadians = rotDegrees * Mathf.Deg2Rad;
            
            _rigidbody.AddTorque((rotAxis * (rotRadians * _parameters.RotationSpringStrength)) -
                                 (_rigidbody.angularVelocity * _parameters.RotationSpringDamper));
        }

        private bool CheckGround(out RaycastHit hit)
        {
            return Physics.Raycast(transform.position, -Vector3.up, out hit, _parameters.DistanceToFloor * 2, _parameters.GroundLayers);
        }
        
        private void SetTargetRotation()
        {
            if (Direction == Vector3.zero) return;
            _targetRotation = Quaternion.Lerp(_targetRotation, Quaternion.LookRotation(Direction), Time.deltaTime * _parameters.RotationSpeed);
        }
    }
}