using System;
using System.Collections;
using DefaultNamespace;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private MeshRenderer _playerModel;

    private PlayerSpawn _playerSpawn;
    private GameUI _gameUI;

    private float _respawnTimeElapsed = 0;
    
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
    
    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> _respawnTime = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    private void Start()
    {
        _playerSpawn = GetComponent<PlayerSpawn>();
        _gameUI = FindFirstObjectByType<GameUI>();
    }

    public override void OnNetworkSpawn()
    {
        _nameText.text = Nickname.Value.ToString();
        _hpText.text = HP.Value.ToString();
        Nickname.OnValueChanged += OnNameChange;
        IsAlive.OnValueChanged += OnIsAliveChanged;
        HP.OnValueChanged += OnHealthChange;
        _respawnTime.OnValueChanged += OnRespawnTimeChange;
        if (IsOwner)
        {
            // Только владелец отправляет на сервер свой локально введенный ник.
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnNetworkDespawn()
    {
        Nickname.OnValueChanged -= OnNameChange;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
        HP.OnValueChanged -= OnHealthChange;
        _respawnTime.OnValueChanged -= OnRespawnTimeChange;
    }
    
    private void OnNameChange(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        _nameText.text = Nickname.Value.ToString();
    }

    private void OnHealthChange(int prValue, int newValue)
    {
        _hpText.text = HP.Value.ToString();
        if (!IsServer) return;
        if (newValue <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnRespawnTimeChange(float oldValue, float newValue)
    {
        if (!IsOwner) return;
        _gameUI.SetRespawnSliderValue(newValue / 3);
    }
    
    private void OnIsAliveChanged(bool prev, bool next)
    {
        _playerModel.enabled = next;
        if (IsOwner) _gameUI.ChangeRespawnWindowVisibility(!next);
        // Показываем/скрываем модель на всех клиентах
        // Студент реализует самостоятельно
    }
    
    private IEnumerator RespawnRoutine()
    {
        
        _respawnTimeElapsed = 0;
        while (_respawnTimeElapsed < 3f)
        {
            _respawnTimeElapsed += Time.deltaTime;
            _respawnTime.Value = _respawnTimeElapsed;
            yield return null;
        }

        _playerSpawn.SetRandomPositionServerRpc(OwnerClientId);
        
        HP.Value = 100;
        IsAlive.Value = true;

        //_playerSpawn.RespawnPlayer();
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Сервер нормализует ник и записывает итоговое значение в NetworkVariable.
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
}
