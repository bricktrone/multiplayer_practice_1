using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerCamera : NetworkBehaviour
    {
        [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);

        private Camera _camera;

        public override void OnStartNetwork()
        {
            if (!Owner.IsLocalClient)
            {
                enabled = false;
                return;
            }
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            _camera.transform.position = transform.position + _offset;
            _camera.transform.LookAt(transform.position);
        }
    }
}