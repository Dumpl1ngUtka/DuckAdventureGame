using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] private float _distanceToFloor;
    [SerializeField] private LayerMask _groundLayers;
    [Header("Spring")] [SerializeField] private float _springStrength;
    [SerializeField] private float _springDamper;
    private Rigidbody _rigidbody;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var isOnGround = IsOnGround(out var hit);
        if (!isOnGround) return;
        Floating(hit);
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
    
    private bool IsOnGround(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, -Vector3.up, out hit, _distanceToFloor, _groundLayers);
    }
}
