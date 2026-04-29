using UnityEngine;
using System.Collections;
using FishNet.Managing;
using FishNet.Object;

namespace DefaultNamespace
{
// PickupManager — обычный MonoBehaviour, работает только на сервере
    public class PickupManager : NetworkBehaviour
    {
        [SerializeField] private GameObject _healthPickupPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _respawnDelay = 10f;

        public override void OnStartNetwork()
        {
            Debug.Log("heal manager started");
            // Менеджер активен только на сервере/хосте
            if (!ServerManager.NetworkManager.IsServerStarted) return;
            SpawnAll();
            Debug.Log("heal manager spawned");
        }

        private void SpawnAll()
        {
            foreach (Transform point in _spawnPoints)
                SpawnPickup(point.position);
        }

        public void OnPickedUp(Vector3 position)
        {
            StartCoroutine(RespawnAfterDelay(position));
        }

        private IEnumerator RespawnAfterDelay(Vector3 position)
        {
            yield return new WaitForSeconds(_respawnDelay);
            SpawnPickup(position);
        }

        private void SpawnPickup(Vector3 position)
        {
            GameObject healObject = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
            healObject.GetComponent<HealthPickup>().Init(this);
            ServerManager.Spawn(healObject);
        }
    }
}