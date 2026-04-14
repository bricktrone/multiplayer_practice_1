using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetRandomPositionServerRpc(OwnerClientId);
            FindFirstObjectByType<PlayerCombat>().AttackingPlayer = GetComponent<PlayerStats>();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRandomPositionServerRpc(ulong playerId)
    {

        Vector2 newPosition = new Vector2(Random.Range(-9f, 9f), Random.Range(-9f, 9f));
        
        GetRandomPositionClientRpc(newPosition, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerId }
            }
        });
    }

    [ClientRpc]
    private void GetRandomPositionClientRpc(Vector2 value, ClientRpcParams clientRpcParams = default)
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
