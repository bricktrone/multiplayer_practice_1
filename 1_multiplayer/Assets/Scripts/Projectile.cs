using FishNet.Object;
using UnityEngine;

namespace DefaultNamespace
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private float _speed = 18f;
        [SerializeField] private int _damage = 20;

        private PlayerStats _shootingPlayer = null;

        public void Initialize(PlayerStats PlayeerOwn)
        {
            _shootingPlayer = PlayeerOwn;
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            PlayerStats target = other.GetComponent<PlayerStats>();
            if (target == null) return;

            // Не наносим урон самому себе
            if (target.OwnerId == OwnerId) return;

            int newHp = Mathf.Max(0, target.HP.Value - _damage);
            if (newHp == 0) _shootingPlayer.Score.Value += 1;
            target.HP.Value = newHp;
            
            ServerManager.Despawn(gameObject);
        }
    }
}