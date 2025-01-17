using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Duck
{
    public class DuckMover : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private LayerMask _groundLayers;
        [SerializeField] private float _maxSpeedMinRange = 3f;
        [SerializeField] private float _maxSpeedMaxRange = 7f;
        [SerializeField] private float _maxSpeedDistance = 10f;
        [SerializeField] private int _stopRotationDegree = 25;
        [SerializeField] private float _maxRotationSpeed = 120f;
        private float _maxSpeed = 5f;
        private float _rotationFraction = 0f;
        private float _speed = 0f;
        private float _currentMaxSpeed = 0f;
        public float _acceleration = 1f;
        private bool _isNeedToMove => Direction != Vector3.zero;
        private bool _isNeedToRotate => _isNeedToMove || RotationDirection != Vector3.zero;
        public Rigidbody Rigidbody => _rigidbody;

        #region Direction
        private Vector3 _direction;
        private float _directionTimer;
        private Vector3 Direction
        {
            get { return _direction; }
            set 
            {
                _directionTimer = 0.5f;
                _direction = value; 
            }
        }

        private Vector3 RotationDirection;
        #endregion

        public float SpeedFraction => _speed / _maxSpeed ;
        public float RotationFraction => _rotationFraction;

        public void SetMoveDirection(Vector3 directoion)
        {
            _maxSpeed = Mathf.Lerp(_maxSpeedMinRange, _maxSpeedMaxRange, directoion.magnitude/ _maxSpeedDistance);
            Direction = directoion.normalized;
        }

        public void SetRotationDirection(Vector3 directoion)
        {
            RotationDirection = directoion;
        }


        private void FixedUpdate()
        {
            if (_isNeedToMove)
                Move();
        }

        private void Update()
        {
            if (_directionTimer > 0f)
                _directionTimer -= Time.deltaTime;
            else
                Direction = Vector3.zero;

            if (_isNeedToRotate)
                Rotate();
            SetSpeed();
        }

        private void SetSpeed()
        {
            if (!_isNeedToMove)
                _currentMaxSpeed = 0f;

            var delta = Vector3.Angle(transform.forward, Direction);
            if (delta < _stopRotationDegree)
                _currentMaxSpeed = _maxSpeed;
            else
                _currentMaxSpeed = Mathf.Lerp(_maxSpeed, 0, (delta - _stopRotationDegree) / 90);

            _speed = Mathf.MoveTowards(_speed, _currentMaxSpeed, _acceleration * Time.deltaTime);
        }

        private void Move()
        {
            if (IsOnGround())
            {
                if (_currentMaxSpeed - _rigidbody.velocity.magnitude > 0)
                    _rigidbody.AddForce(transform.forward, ForceMode.VelocityChange);
            }
        }

        private void Rotate()
        {
            var delta = Vector3.SignedAngle(transform.forward, Direction, Vector3.up);
            Debug.Log(delta);
            _rotationFraction = Mathf.Abs(delta) / 180;
            var newPivotRotation = transform.rotation.eulerAngles + new Vector3(0, delta, 0) * _maxRotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(newPivotRotation);
        }

        private bool IsOnGround()
        {
            //return true;
            return Physics.Raycast(transform.position + transform.up * 0.1f, -transform.up, 0.5f, _groundLayers);
        }
    }

}

