using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // команды для linux
    // cd /mnt/c/Users/111/Documents/GitHub/multiplayer_practice_1/linux_server_build/
    // ./linux_server_build.x86_64 -batchmode -nographics
    //
    // 172.31.191.75
    
    [SerializeField] private int _requiredPlayers = 2;
    [SerializeField] private GameObject _ManagerUI;
    [SerializeField] private GameObject _WaitUI;
    [SerializeField] private TextMeshProUGUI _waitText;
    [SerializeField] private GameObject _ScoreUI;
    [SerializeField] private GameObject _playerScorePrefab;

    public readonly SyncVar<GameState> CurrentState = new SyncVar<GameState>(GameState.WaitingForPlayers);

    public readonly SyncVar<int> ConnectedPlayers = new SyncVar<int>(0);

    public override void OnStartNetwork()
    {
        // Подписываемся на изменения переменных для ВСЕХ (сервер и клиенты)
        CurrentState.OnChange += OnGameStateChanged;
        ConnectedPlayers.OnChange += OnConnectedPlayersChanged;

        // Инициализируем UI при старте объекта под актуальное состояние сети
        UpdateUI(CurrentState.Value);
        UpdateWaitText(ConnectedPlayers.Value);
    }
    
    public override void OnStopNetwork()
    {
        CurrentState.OnChange -= OnGameStateChanged;
        ConnectedPlayers.OnChange -= OnConnectedPlayersChanged;
    }
    
    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        ShowingResults
    }

    public override void OnStartServer()
    {
        base.ServerManager.OnRemoteConnectionState += OnPlayerConnectionChanged;
        // Учитываем самого хоста при старте сервера
        ConnectedPlayers.Value = base.ServerManager.Clients.Count;
    }
    
    public override void OnStopServer()
    {
        base.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionChanged;
    }
    

    private void OnPlayerConnectionChanged(
        NetworkConnection conn,
        FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (!base.IsServerInitialized) return;
        

        // Пересчитываем игроков.
        ConnectedPlayers.Value = base.ServerManager.Clients.Count;

        if (CurrentState.Value == GameState.WaitingForPlayers
            && ConnectedPlayers.Value >= _requiredPlayers)
        {
            StartMatch();
        }
    }
    
    private void UpdateUI(GameState state)
    {
        _ManagerUI.SetActive(true);
        
        // Сбрасываем состояния по умолчанию
        _WaitUI.SetActive(false);
        _ScoreUI.SetActive(false);

        switch (state)
        {
            case GameState.WaitingForPlayers:
                _WaitUI.SetActive(true);
                break;
            case GameState.InProgress:
                // Экран ожидания скрывается
                break;
            case GameState.ShowingResults:
                _ScoreUI.SetActive(true);
                break;
        }
    }
    
    private void UpdateWaitText(int count)
    {
        if (_waitText != null)
        {
            _waitText.text = $"Ожидание игроков {count}/{_requiredPlayers}";
        }
    }
    
    private void OnGameStateChanged(GameState oldValue, GameState newValue, bool asServer)
    {
        // Этот метод автоматически сработает на каждом клиенте при изменении состояния игры
        UpdateUI(newValue);
        Debug.Log($"Game state changed: {oldValue} -> {newValue}");
    }

    private void OnConnectedPlayersChanged(int oldValue, int newValue, bool asServer)
    {
        // Этот метод автоматически обновит текст у всех игроков
        UpdateWaitText(newValue);
    }

    private void StartMatch()
    {
        CurrentState.Value = GameState.InProgress;
        Debug.Log("[Server] Match started!");
    }
    
    public readonly SyncVar<float> MatchTimer = new SyncVar<float>(60f);
    private void Update()
    {
        if (!base.IsServerInitialized) return;
        if (CurrentState.Value != GameState.InProgress) return;

        MatchTimer.Value -= Time.deltaTime;

        if (MatchTimer.Value <= 0f)
        {
            EndMatch();
        }
    }

    private void EndMatch()
    {
        CurrentState.Value = GameState.ShowingResults;
        Debug.Log("[Server] Match ended! Showing results...");
        CreateScoreBoardRpc();

        // Через 5 секунд возвращаемся в лобби.
        Invoke(nameof(ResetToLobby), 5f);
    }
    
    [ObserversRpc]
    private void CreateScoreBoardRpc()
    {
        // Очищаем старый UI на всякий случай у всех
        ClearScoreBoard();

        // Каждый клиент локально собирает информацию по доступным в сети игрокам
        foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player")) // Или ваш способ поиска игроков
        {
            PlayerStats pn = playerObj.GetComponent<PlayerStats>();
            if (pn != null)
            {
                GameObject playerScroe = Instantiate(_playerScorePrefab, _ScoreUI.transform);
                playerScroe.GetComponent<TextMeshProUGUI>().text = $"{pn.Nickname.Value}: {pn.Score.Value} kills";
            }
        }
    }
    
    private void ClearScoreBoard()
    {
        for (int i = _ScoreUI.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_ScoreUI.transform.GetChild(i).gameObject);
        }
    }
    
    [ObserversRpc]
    private void ClearScoreBoardRpc()
    {
        ClearScoreBoard();
    }

    private void ResetToLobby()
    {
        // Сбросить очки всех игроков.
        foreach (var conn in base.ServerManager.Clients.Values)
        {
            foreach (var nob in conn.Objects)
            {
                PlayerStats pn = nob.GetComponent<PlayerStats>();
                if (pn != null)
                {
                    pn.HP.Value = 100;
                    pn.Score.Value = 0;
                }
            }
        }

        ClearScoreBoardRpc();

        MatchTimer.Value = 60f;
        if (ConnectedPlayers.Value >= _requiredPlayers)
            CurrentState.Value = GameState.InProgress;
        else
        {
            CurrentState.Value = GameState.WaitingForPlayers;
        }
        Debug.Log("[Server] Lobby reset. Waiting for players...");
    }
}
