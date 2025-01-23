using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Duck
{
    public class LegsAnimation : MonoBehaviour
    {
        [SerializeField] private Transform _leftLeg;
        [SerializeField] private Transform _rightLeg;
        [SerializeField] private float _stepDistance = 0.5f;
        [SerializeField] private float _stepHeight = 0.2f;
        [SerializeField] private float _maxDistanceToGround = 1f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _animationSpeed = 5f;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _legSpacing = 0.5f;
        [SerializeField] private float _groundCheckDistance = 1.5f;
        [SerializeField] private float _animationSpeedMultiplier = 1f;

        private Vector3 _leftLegStartPos;
        private Vector3 _rightLegStartPos;
        private Vector3 _leftLegGoalPos;
        private Vector3 _rightLegGoalPos;
        private float _leftLegLerp = 1f;
        private float _rightLegLerp = 1f;
        private bool _isLeftLegStep = true;

        private void Awake()
        {
            _leftLegStartPos = _leftLeg.position;
            _rightLegStartPos = _rightLeg.position;
            _leftLegGoalPos = _leftLegStartPos;
            _rightLegGoalPos = _rightLegStartPos;
        }

        private void Update()
        {
            var velocity = _rigidbody.velocity;
            var moveDirection = velocity.normalized;

            if (velocity.magnitude > 0.1f)
            {
                _animationSpeedMultiplier = Mathf.Clamp(velocity.magnitude, 0.5f, 2f);
                HandleLegMovement(moveDirection);
            }
            else
            {
                _leftLeg.position = _leftLegStartPos;
                _rightLeg.position = _rightLegStartPos;
            }


            Debug.DrawRay(_leftLeg.position, Vector3.down * _groundCheckDistance, Color.red);
            Debug.DrawRay(_rightLeg.position, Vector3.down * _groundCheckDistance, Color.red);
            Debug.DrawRay(transform.position, moveDirection * _stepDistance, Color.green);
        }

        private void HandleLegMovement(Vector3 moveDir)
        {
            switch (_isLeftLegStep)
            {
                case true when _leftLegLerp >= 1f:
                {
                    if (IsGrounded(_leftLeg))
                    {
                        _leftLegGoalPos = transform.position + (moveDir * _stepDistance) + Vector3.left * _legSpacing;
                        _leftLegLerp = 0f;
                        _isLeftLegStep = false;
                    }

                    break;
                }
                case false when _rightLegLerp >= 1f:
                {
                    if (IsGrounded(_rightLeg))
                    {
                        _rightLegGoalPos = transform.position + (moveDir * _stepDistance) + Vector3.right * _legSpacing;
                        _rightLegLerp = 0f;
                        _isLeftLegStep = true;
                    }

                    break;
                }
            }

            if (_leftLegLerp < 1f)
            {
                _leftLegLerp += _animationSpeed * Time.deltaTime * _animationSpeedMultiplier;
                var newPos = Vector3.Lerp(_leftLegStartPos, _leftLegGoalPos, Mathf.Sin(_leftLegLerp * Mathf.PI * 0.5f));
                newPos.y += Mathf.Sin(_leftLegLerp * Mathf.PI) * _stepHeight;
                _leftLeg.position = newPos;
            }
            else
            {
                _leftLegStartPos = _leftLegGoalPos;
            }

            if (_rightLegLerp < 1f)
            {
                _rightLegLerp += _animationSpeed * Time.deltaTime * _animationSpeedMultiplier;
                var newPos = Vector3.Lerp(_rightLegStartPos, _rightLegGoalPos,
                    Mathf.Sin(_rightLegLerp * Mathf.PI * 0.5f));
                newPos.y += Mathf.Sin(_rightLegLerp * Mathf.PI) * _stepHeight;
                _rightLeg.position = newPos;
            }
            else
            {
                _rightLegStartPos = _rightLegGoalPos;
            }
        }

        private bool IsGrounded(Transform leg)
        {
            RaycastHit hit;
            return Physics.Raycast(leg.position, Vector3.down, out hit, _groundCheckDistance, _layerMask);
        }

        private Vector3 LegLerp(Vector3 oldPos, Vector3 newPos, float t)
        {
            t = Mathf.Clamp01(t);
            var currentPos = Vector3.Lerp(oldPos, newPos, 0.4f);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * _stepHeight;
            return currentPos;
        }

        private void CalculateLegPosition(Transform leg, Vector3 newPos, float lerp)
        {
            lerp = Mathf.Clamp01(lerp);
            var currentPos = Vector3.Lerp(leg.position, newPos, lerp);
            currentPos.y += Mathf.Sin(lerp * Mathf.PI) * _stepHeight;
            leg.position = currentPos;
        }

        private void UpdateAnimationSpeed(float magnitude)
        {
            _animationSpeed = magnitude;
        }
    }
}