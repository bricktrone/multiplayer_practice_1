using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private PlayerCombat _playerCombat;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _hpText;
    
    // Ник должен быть виден всем клиентам, но менять его может только сервер.
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // HP тоже читает каждый клиент, но изменяется только на сервере.
    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        _nameText.text = Nickname.Value.ToString();
        _hpText.text = HP.Value.ToString();
        Nickname.OnValueChanged += OnNameChange;
        HP.OnValueChanged += OnHealthChange;
        if (IsOwner)
        {
            // Только владелец отправляет на сервер свой локально введенный ник.
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnNetworkDespawn()
    {
        Nickname.OnValueChanged -= OnNameChange;
        HP.OnValueChanged -= OnHealthChange;
    }
    
    private void OnNameChange(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        _nameText.text = Nickname.Value.ToString();
    }

    private void OnHealthChange(int prValue, int newValue)
    {
        _hpText.text = HP.Value.ToString();
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Сервер нормализует ник и записывает итоговое значение в NetworkVariable.
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
}
