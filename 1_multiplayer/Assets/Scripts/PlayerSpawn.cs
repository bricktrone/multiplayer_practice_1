using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    public NetworkVariable<float> positionX = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public override void OnNetworkSpawn()
    {
        positionX.OnValueChanged += OnPositionChange;
        if (IsOwner)
        {
            SetRandomPositionServerRpc();
            FindFirstObjectByType<PlayerCombat>().AttackingPlayer = GetComponent<PlayerStats>();
        }
    }

    public override void OnNetworkDespawn()
    {
        positionX.OnValueChanged -= OnPositionChange;
    }
    
    private void OnPositionChange(float prValue, float newValue)
    {
        Vector3 pos = transform.position;
        pos.x = newValue;
        transform.position = pos;
    }

    [ServerRpc]
    private void SetRandomPositionServerRpc()
    {
        positionX.Value = Random.Range(-9f, 10f);
    }
}
