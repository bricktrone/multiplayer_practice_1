using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class PlayerShooting : NetworkBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _cooldown = 0.4f;
        [SerializeField] private int _maxAmmo = 10;

        private float _lastShotTime; 

        private readonly SyncVar<int> _currentAmmo = new SyncVar<int>();

        // IsAlive и связь с PlayerNetwork настраивается студентом
        private PlayerStats _playerStats;
        private GameUI _gameUI;

        private InputSystem_Actions _input;


        public override void OnStartNetwork()
        {
            _currentAmmo.OnChange += OnAmmoChange;
            
            _gameUI = FindFirstObjectByType<GameUI>();
            _playerStats = GetComponent<PlayerStats>();
        }
        
        public override void OnStartClient()
        {
            if (!Owner.IsLocalClient) return;
            SetMaxAmmoServerRpc();
        }


        public override void OnStopNetwork()
        {
            _currentAmmo.OnChange -= OnAmmoChange;
        }

        private void OnAmmoChange(int oldValue, int newValue, bool asServer)
        {
            if (!IsOwner) return;
            _gameUI.ChangeBulletCount(newValue);
        }

        private void Awake()
        {
            _currentAmmo.Value = 0;
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.Player.Jump.performed += OnShoot;
        }

        private void OnDisable()
        {
            _input.Player.Jump.performed -= OnShoot;
            _input.Disable();
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }

        [ServerRpc]
        private void SetMaxAmmoServerRpc()
        {
            _currentAmmo.Value = _maxAmmo;
        }

        [ServerRpc]
        private void ShootServerRpc(Vector3 pos, Vector3 dir)
        {
            // 1. Жив ли игрок?
            if (!_playerStats.IsAlive.Value) return;

            // 2. Есть ли патроны?
            if (_currentAmmo.Value <= 0) return;

            // 3. Прошёл ли кулдаун?
            if (Time.time < _lastShotTime + _cooldown) return;

            _lastShotTime = Time.time;
            _currentAmmo.Value--;

            GameObject projectileObject = Instantiate(_projectilePrefab, pos + dir * 1.2f,
                Quaternion.LookRotation(dir));
            ServerManager.Spawn(projectileObject, Owner);
        }
    }
}