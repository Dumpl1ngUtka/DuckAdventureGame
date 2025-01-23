using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Duck
{
    public class Duck : MonoBehaviour
    {
        [SerializeField] private PhysicalMover _mover;
        [SerializeField] private PlayerController _input;

        private void OnEnable()
        {
            _input.LeftButtonDown += SetMovePosition;
        }

        private void SetMovePosition(Vector3 position)
        {
            _mover.SetMoveDirection(position - transform.position);
        }

        private void OnDisable()
        {
            
        }
    }
}

