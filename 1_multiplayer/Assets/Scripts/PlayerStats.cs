using System;
using System.Collections;
using DefaultNamespace;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private MeshRenderer _playerModel;

    private PlayerSpawn _playerSpawn;
    private GameUI _gameUI;

    private float _respawnTimeElapsed = 0;
    
    // Ник должен быть виден всем клиентам, но менять его может только сервер.
    public readonly SyncVar<string> Nickname = new SyncVar<string>();

    // HP тоже читает каждый клиент, но изменяется только на сервере.
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);
    
    private readonly SyncVar<float> _respawnTime = new SyncVar<float>(-1);

    // private void Awake()
    // {
    //     HP.Value = 100;
    //     IsAlive.Value = true;
    //     _respawnTime.Value = -1;
    // }

    private void Start()
    {
        _playerSpawn = GetComponent<PlayerSpawn>();
        _gameUI = FindFirstObjectByType<GameUI>();
    }

    public override void OnStartNetwork()
    {
        Nickname.OnChange += OnNameChange;
        HP.OnChange += OnHealthChange;
        IsAlive.OnChange += OnIsAliveChanged;
        _respawnTime.OnChange += OnRespawnTimeChange;

        _nameText.text = Nickname.Value;
        _hpText.text = HP.Value.ToString();
        Debug.Log("stats started");
    }

    public override void OnStartClient()
    {
        if (!IsOwner) return;
        Debug.Log("Stats owner started");
        // Только владелец отправляет на сервер свой локально введенный ник.
        SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
    }

    public override void OnStopNetwork()
    {
        Nickname.OnChange -= OnNameChange;
        HP.OnChange -= OnHealthChange;
        IsAlive.OnChange -= OnIsAliveChanged;
        _respawnTime.OnChange -= OnRespawnTimeChange;
    }
    
    private void OnNameChange(string previousValue, string newValue, bool asServer)
    {
        _nameText.text = newValue;
    }

    private void OnHealthChange(int prValue, int newValue, bool asServer)
    {
        _hpText.text = newValue.ToString();
        if (!IsServer) return;
        if (newValue <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnRespawnTimeChange(float oldValue, float newValue, bool asServer)
    {
        if (!IsOwner) return;
        _gameUI.SetRespawnSliderValue(newValue / 3);
    }
    
    private void OnIsAliveChanged(bool prev, bool next, bool asServer)
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

        _playerSpawn.SetRandomPositionServerRpc(OwnerId);
        
        HP.Value = 100;
        IsAlive.Value = true;

        //_playerSpawn.RespawnPlayer();
    }
    

    [ServerRpc]
    private void SubmitNicknameServerRpc(string nickname)
    {
        Debug.Log("nickname from ui got");
        // Сервер нормализует ник и записывает итоговое значение в NetworkVariable.
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
}
