using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Duck
{
    public class LegAnimation : MonoBehaviour
    {
        [SerializeField] private PhysicalMover _mover;
        [SerializeField] private LegAnimation _secondLeg;
        [SerializeField] private Rigidbody _rigidbody;
        [Header("Ray Cast Settings")]
        [SerializeField] private float _maxLegOffsetDistance = 1f;
        [SerializeField] private float _maxLegAdditionalSpacing = 1f;
        [SerializeField] private float _rayStartHieght = 0.3f; 
        [SerializeField] private float _legSpacing;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _maxHeight;
        [Header("Animation")]
        [SerializeField] private float _stepHeight = 0.5f;
        [SerializeField] private float _stepDistance = 1f;
        [SerializeField] private float _animationSpeedMultiplier = 1f;
        [SerializeField] private AnimationCurve _stepVerticalPath;
        private Vector3 _previousLegPosition;
        private Vector3 _legPosition;
        private Vector3 _goatLegPosition;
        private float _moveLerp = 1f;
        private float _animationSpeed;
        private Quaternion _previousLegRotation;
        private Quaternion _legRotation;
        private Quaternion _goatLegRotation;

        [HideInInspector] public bool IsStepped = false;
        public bool IsLegOnGround => _moveLerp >= 1f;

        private void Awake()
        {
            _legPosition = transform.position;
            _goatLegPosition = transform.position;

            _legRotation = Quaternion.LookRotation(_rigidbody.transform.up, -_rigidbody.transform.forward);
            _goatLegRotation = _legRotation;
        }

        private void Update()
        {
            SetAnimationSpeed();
            CalculateNewLegTransform(out _goatLegPosition, out _goatLegRotation);

            if (!_mover.IsOnGround)
            {
                _legPosition = _rigidbody.position + _rigidbody.transform.right * _legSpacing;
                _legRotation = Quaternion.LookRotation(_rigidbody.transform.up, -_rigidbody.transform.forward);
            }
            else if (_secondLeg.IsLegOnGround && !IsStepped)
            {
                var bodyHorizontalPos = _rigidbody.position;
                //bodyHorizontalPos.y = _legPosition.y;
                var isDisanceEnough = (bodyHorizontalPos - _legPosition).magnitude > _stepDistance;
                if (isDisanceEnough && IsLegOnGround)
                    SetParametersForAnimation();
                
                if (transform.position.y > _rigidbody.position.y)
                    SetParametersForAnimation();
            }

            if (!IsLegOnGround && _mover.IsOnGround)
            {

                _legPosition = LerpStep(_previousLegPosition, _goatLegPosition, _moveLerp);
                _legRotation = Quaternion.Lerp(_previousLegRotation, _goatLegRotation, _moveLerp);
                _moveLerp += Time.deltaTime * _animationSpeed;
            }

            transform.position = _legPosition;
            transform.rotation = _legRotation;
        }

        private void SetParametersForAnimation()
        {
            _previousLegPosition = _legPosition;
            _previousLegRotation = _legRotation;
            _moveLerp = 0f;
            IsStepped = true;
            _secondLeg.IsStepped = false;
        }
        
        private void CalculateNewLegTransform(out Vector3 newLegPosition, out Quaternion newLegRotation)
        {
            var rbTransform = _rigidbody.transform;
            var origin = rbTransform.position + rbTransform.right * (_legSpacing + GetAdditionSpacingBySpeed()) +
                         rbTransform.forward *  GetOffsetBySpeed() + rbTransform.up * _rayStartHieght;
            
            if (Physics.Raycast(origin, -rbTransform.up, out var hit, _maxHeight, _layerMask))
            {
                newLegPosition = hit.point;
                newLegRotation = Quaternion.LookRotation(hit.normal, -rbTransform.forward);
            }
            else
            {
                newLegPosition = rbTransform.position + rbTransform.right * _legSpacing;
                newLegRotation = Quaternion.LookRotation(_rigidbody.transform.up, -rbTransform.forward);
            }
        }

        private float GetOffsetBySpeed()
        {
            var dotForward = Vector3.Dot(_rigidbody.velocity, _rigidbody.transform.forward);
            return _mover.MaxSpeed == 0? 0 : dotForward / _mover.MaxSpeed * _maxLegOffsetDistance;
        }

        private float GetAdditionSpacingBySpeed()
        {
            var dotRight = Vector3.Dot(_rigidbody.velocity, _rigidbody.transform.right);
            return _mover.MaxSpeed == 0? 0 : dotRight / _mover.MaxSpeed * _maxLegAdditionalSpacing;
        }
        
        private Vector3 LerpStep(Vector3 oldPos, Vector3 newPos, float lerp)
        {
            lerp = Mathf.Clamp01(lerp);
            var currentPos = Vector3.Lerp(oldPos, newPos, lerp);
            currentPos.y += _stepVerticalPath.Evaluate(lerp) * _stepHeight;
            return currentPos;
        }

        private void SetAnimationSpeed()
        {
            var magnitude = _rigidbody.velocity.magnitude;
            _animationSpeed = magnitude < 1 ? 1 : magnitude;
            _animationSpeed *= _animationSpeedMultiplier;
        }
    }
}