using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects
{
    public class Wind : MonoBehaviour
    {
        [SerializeField] private Vector3 _direction = Vector3.forward;
        [SerializeField] private float _power;

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                var value = _power * Time.deltaTime * _direction;
                rigidbody.AddForce(value);
            }
        }
    }
}

