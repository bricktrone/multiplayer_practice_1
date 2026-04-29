using System;
using FishNet.Managing;
using TMPro;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _loadCanvas;
    [SerializeField] private GameObject _gameCanvas;
    [SerializeField] private GameObject _playerPrefab;

    // Сохраняем ник локально до появления сетевого объекта игрока.
    public static string PlayerNickname { get; private set; } = "Player";

    public void StartAsHost()
    {
        _networkManager.ServerManager.StartConnection();
        StartAsClient();
    }

    public void StartAsClient()
    {
        SaveNickname();
        // Клиент только подключается к уже запущенному хосту/серверу.
        _networkManager.ClientManager.StartConnection();
        HideCanvas();
    }

    // private void OnEnable()
    // {
    //     if (_networkManager != null)
    //         _networkManager.ClientManager.OnRemoteConnectionState += OnConnectionComplete;
    // }
    //
    // private void OnDisable()
    // {
    //     if (_networkManager != null)
    //         _networkManager.ClientManager.OnRemoteConnectionState -= OnConnectionComplete;
    // }

    private void OnConnectionComplete(RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log($"connected {_networkManager.ClientManager.Connection.ClientId}");
            GameObject player = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
            _networkManager.ServerManager.Spawn(player, _networkManager.ClientManager.Connection);
        }
    }

    private void SaveNickname()
    {
        // Нормализуем ввод, чтобы сервер не получил пустую строку.
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void HideCanvas()
    {
        _gameCanvas.SetActive(true);
        _loadCanvas.SetActive(false);
    }
}
