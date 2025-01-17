using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        public Action<Vector3> LeftButtonDown;

        public void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, 100, _layerMask))
            {
                if (Input.GetMouseButton(0))
                    LeftMouseButtonDown(hit);
            }
        }   

        private void LeftMouseButtonDown(RaycastHit hit)
        {
            var hitPosition = hit.point;
            LeftButtonDown?.Invoke(hitPosition);
        }
    }
}

