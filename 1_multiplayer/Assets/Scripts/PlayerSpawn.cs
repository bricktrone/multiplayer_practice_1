using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    
    public override void OnStartClient()
    {
        if (IsOwner)
        {
            SetRandomPositionServerRpc(OwnerId);
            FindFirstObjectByType<PlayerCombat>().AttackingPlayer = GetComponent<PlayerStats>();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetRandomPositionServerRpc(int playerId)
    {

        Vector2 newPosition = new Vector2(Random.Range(-9f, 9f), Random.Range(-9f, 9f));
        
        if (ServerManager.Clients.TryGetValue(playerId, out NetworkConnection connection))
        {
            GetRandomPositionClientRpc(connection, newPosition);
        }
    }

    [TargetRpc]
    private void GetRandomPositionClientRpc(NetworkConnection target, Vector2 value)
    {
        if (!IsOwner) return;
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;
        Vector3 pos = transform.position;
        pos.y = 1;
        pos.x = value.x;
        pos.z = value.y;
        transform.position = pos;
        controller.enabled = true;
    }
}
