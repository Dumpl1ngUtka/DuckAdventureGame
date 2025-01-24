using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Duck
{
    public class Duck : MonoBehaviour
    {
        [SerializeField] private Parameters _parameters;
        [SerializeField] private PhysicalMover _mover;
        [SerializeField] private PlayerController _input;

        public Parameters Parameters => _parameters;

        private void Awake()
        {
            _mover.Init(this);
        }

        private void OnEnable()
        {
            _input.LeftButtonDown += SetMovePosition;
        }

        private void SetMovePosition(Vector3 position)
        {
            var direction = position - transform.position;
            direction.y = 0;
            _mover.SetMoveDirection(direction);
        }

        private void OnDisable()
        {
            
        }
    }
}

