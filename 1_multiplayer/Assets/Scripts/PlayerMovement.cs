using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _gravity = -9.81f;

        private CharacterController _characterController;
        private InputSystem_Actions _input;
        private PlayerStats _playerStats;
        private float _verticalVelocity;

        private Vector2 _moveVector;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.Player.Move.performed += OnMovementStart;
            _input.Player.Move.canceled += OnMovementStop;
        }

        private void OnDisable()
        {
            _input.Player.Move.performed -= OnMovementStart;
            _input.Player.Move.canceled -= OnMovementStop;
            _input.Disable();
        }
        
        public override void OnNetworkSpawn()
        {
            _playerStats = GetComponent<PlayerStats>();
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
        }
        
        private void OnMovementStart(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            _moveVector = context.ReadValue<Vector2>();
        }
        
        private void OnMovementStop(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            _moveVector = Vector2.zero;
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            if (!_playerStats.IsAlive.Value) return;
            
            Vector3 move = new Vector3(_moveVector.x, 0f, _moveVector.y).normalized * _speed;

            _verticalVelocity += _gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _characterController.Move(move * Time.deltaTime);

            if (_characterController.isGrounded) _verticalVelocity = 0f;
        }
    }
}