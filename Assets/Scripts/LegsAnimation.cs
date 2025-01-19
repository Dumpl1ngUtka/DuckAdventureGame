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
        [SerializeField] private float _stepDistance;
        [SerializeField] private float _stepHeight;
        [SerializeField] private float _maxDistanceToGround = 1;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _animationSpeed = 15;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _legSpacing;
        private Vector3 _leftLegPosition;
        private Vector3 _rightLegPosition;
        private Vector3 _goalLeftLegPosition;
        private Vector3 _goalRightLegPosition;
        private float _leftLegLerp = 1;
        private float _rightLegLerp = 1;
        private float _leftLegRayOffset;
        private float _rightLegRayOffset;
        private bool _feetOnTheGround => (_leftLegLerp >= 1f && _rightLegLerp >= 1f);

        private void Awake()
        {
            _leftLegPosition = _leftLeg.position;
            _rightLegPosition = _rightLeg.position;
            //_leftLegPosition = GetLegPosition(_leftLeg, _leftLegTarget);
            //_rightLegPosition = GetLegPosition(_rightLeg, _rightLegTarget);
        }

        private void Update()
        {
            //Debug.Log(_leftLeg.localPosition + " : " + _rightLeg.localPosition);//z
            //SetRayOffset();
            ////_leftLeg.position = _leftLegPosition;

            //if ((_leftLegPosition - _leftLeg.position).magnitude > 0.01f && _leftLegLerp >= 1f && _rightLegLerp >= 1f)
            //{
            //    _leftLegLerp = 0f;
            //}
            //if (_leftLegLerp < 1f)
            //{
            //    MoveLeg(_leftLeg, _leftLegPosition, _leftLegLerp);
            //    _leftLegLerp += _animationSpeed * Time.deltaTime;
            //}
            //else
            //{
            //    _leftLegPosition = GetLegPositionL(_leftLeg);
            //}

            //if ((_rightLegPosition - _rightLeg.position).magnitude > 0.01f && _leftLegLerp >= 1f &&_rightLegLerp >= 1f )
            //{
            //    _rightLegLerp = 0f;
            //}
            //if (_rightLegLerp < 1f)
            //{
            //    MoveLeg(_rightLeg, _rightLegPosition, _rightLegLerp);
            //    _rightLegLerp += _animationSpeed * Time.deltaTime;
            //}
            //else
            //{
            //    _rightLegPosition = GetLegPositionR(_rightLeg);
            //}

            if (true)
            {
                var leftLegOffset = _leftLeg.localPosition.z;
                var rightLegOffset = _rightLeg.localPosition.z;

                var moveDot = Vector3.Dot(transform.forward, _rigidbody.velocity);
                var moveDotSign = Mathf.Sign(moveDot);

                //UpdateAnimationSpeed(_rigidbody.velocity.magnitude);

                bool isNeedToStep = (moveDot >= 0 && leftLegOffset < 0 && rightLegOffset < 0) ||
                                    (moveDot <= 0 && leftLegOffset > 0 && rightLegOffset > 0);

                if (isNeedToStep)
                {
                    if (Mathf.Abs(leftLegOffset) > Mathf.Abs(rightLegOffset))
                    {
                        _goalLeftLegPosition = transform.position + _stepDistance * moveDotSign * Vector3.forward + Vector3.left * 0.5f;
                        _leftLegLerp = 0f;
                    }
                    else
                    {
                        _goalRightLegPosition = transform.position + _stepDistance * moveDotSign * Vector3.forward - Vector3.left * 0.5f;
                        _rightLegLerp = 0f;
                    }
                }
            }
            //else
            //{
            //    if (_rightLegLerp < 1f)
            //    {
            //        _rightLegPosition = LegLerp(_rightLegPosition, _goalRightLegPosition, _rightLegLerp);
            //        _rightLegLerp += _animationSpeed * Time.deltaTime;
            //    }

            //    if (_leftLegLerp < 1f)
            //    {
            //        _leftLegPosition = LegLerp(_leftLegPosition, _goalLeftLegPosition, _leftLegLerp);
            //        _leftLegLerp += _animationSpeed * Time.deltaTime;
            //    }
            //}

            _rightLeg.position = _goalLeftLegPosition;
            _leftLeg.position = _goalRightLegPosition;

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

        //private void SetRayOffset()
        //{
        //    _leftLegRayOffset = _rigidbody.velocity.magnitude;
        //    _rightLegRayOffset = -_rigidbody.velocity.magnitude;
        //}

        //private Vector3 GetLegPositionR(Transform leg)
        //{
        //    var target = transform.position + -transform.right * _legSpacing + transform.forward * _rightLegRayOffset;
        //    Vector3 newLegPosition = target;
        //    if (Physics.Raycast(target, Vector3.down, out var hit, _maxDistanceToGround, _layerMask))
        //        newLegPosition = hit.point;

        //    if ((leg.position - newLegPosition).magnitude > _stepDistance)
        //        return newLegPosition;

        //    return leg.position;
        //}

        //private Vector3 GetLegPositionL(Transform leg)
        //{
        //    var target = transform.position + transform.right * _legSpacing + transform.forward * _leftLegRayOffset;
        //    Vector3 newLegPosition = target;
        //    if (Physics.Raycast(target, Vector3.down, out var hit, _maxDistanceToGround, _layerMask))
        //        newLegPosition = hit.point;

        //    if ((leg.position - newLegPosition).magnitude > _stepDistance)
        //        return newLegPosition;

        //    return leg.position;
        //}
    }

}
