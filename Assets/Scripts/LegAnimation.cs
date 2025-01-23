using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Duck
{
    public class LegAnimation : MonoBehaviour
    {
        [SerializeField] private LegAnimation _secondLeg;
        [SerializeField] private Transform _leg;
        [SerializeField] private float _legSpacing;
        [SerializeField] private float _maxHeight;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _stepHeight = 0.5f;
        [SerializeField] private float _animationSpeedMultiplier = 1f;
        [SerializeField] private float _stepDistance = 1f;
        [SerializeField] private AnimationCurve _stepVerticalPath;
        private float _legOffset;
        private float _additionLegSpacing;
        private Vector3 _previousLegPosition;
        private Vector3 _legPosition;
        private Vector3 _goatLegPosition;
        private float _moveLerp = 1f;
        private float _animationSpeed;

        public bool _isStepped = false;
        public bool IsOnGround => _moveLerp >= 1f;

        private void Awake()
        {
            _legPosition = _leg.position;
            _goatLegPosition = _leg.position;
        }

        private void Update()
        {
            SetOffsetBySpeed();
            SetAnimationSpeed();

            if (_secondLeg.IsOnGround && !_isStepped)
            {
                var ray = new Ray(_rigidbody.transform.position + _rigidbody.transform.right * (_legSpacing + _additionLegSpacing) + _rigidbody.transform.forward * _legOffset, Vector3.down);
                
                if (Physics.Raycast(ray, out var hit, _maxHeight, _layerMask))
                    _goatLegPosition = hit.point;
                else
                    _goatLegPosition = _rigidbody.position + _rigidbody.transform.right * _legSpacing;

                if ((_goatLegPosition - _legPosition).magnitude > _stepDistance)
                {
                    _previousLegPosition = _legPosition;
                    _moveLerp = 0f;
                }

            }
            if (_moveLerp < 1f)
            {
                _isStepped = true;
                _secondLeg._isStepped = false;
                _legPosition = LerpStep(_previousLegPosition, _goatLegPosition, _moveLerp);
                _moveLerp += Time.deltaTime * _animationSpeed;
            }
            _leg.position = _legPosition;
        }

        private void SetOffsetBySpeed()
        {
            var dot = Vector3.Dot(_rigidbody.velocity, _rigidbody.transform.forward);
            //var magnitude = _rigidbody.velocity.magnitude;
            _legOffset = dot / 5;

            var dot2 = Vector3.Dot(_rigidbody.velocity, _rigidbody.transform.right);
            //var magnitude2 = _rigidbody.velocity.magnitude;
            _additionLegSpacing = dot2 / 15;
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
            _animationSpeed = magnitude < 1? 1 : magnitude;
            _animationSpeed *= _animationSpeedMultiplier;
        }
    }
}

