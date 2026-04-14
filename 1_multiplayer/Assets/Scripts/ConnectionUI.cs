using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _loadCanvas;
    [SerializeField] private GameObject _gameCanvas;

    // Сохраняем ник локально до появления сетевого объекта игрока.
    public static string PlayerNickname { get; private set; } = "Player";

    public void StartAsHost()
    {
        SaveNickname();
        HideCanvas();
        // Хост одновременно является сервером и клиентом.
        NetworkManager.Singleton.StartHost();
    }

    public void StartAsClient()
    {
        SaveNickname();
        HideCanvas();
        // Клиент только подключается к уже запущенному хосту/серверу.
        NetworkManager.Singleton.StartClient();
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
