using FishNet.Object;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    public PlayerStats AttackingPlayer;
    public void TryAttack(int damage, PlayerStats target)
    {
        if (target == null) return;
        DealDamageServerRpc(target.OwnerId, AttackingPlayer.OwnerId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DealDamageServerRpc(int targetId, int attackerId, int damage)
    {
        if (!NetworkManager.ServerManager.Objects.Spawned.TryGetValue(targetId, out NetworkObject targetObject))
            return;
        if (!NetworkManager.ServerManager.Objects.Spawned.TryGetValue(attackerId, out NetworkObject attackingObject))
            return;
        
        PlayerStats targetPlayer = targetObject.GetComponent<PlayerStats>();
        PlayerStats attackingPlayer = attackingObject.GetComponent<PlayerStats>();
        // Запрещаем урон самому себе и удары по некорректной цели.
        if (targetPlayer == null || targetPlayer == attackingPlayer)
            return;

        // Итоговое значение HP ограничиваем снизу нулем.
        int newHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
        targetPlayer.HP.Value = newHp;
    }
}
