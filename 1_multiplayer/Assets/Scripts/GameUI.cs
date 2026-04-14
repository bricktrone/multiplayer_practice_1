using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private GameObject _respawnObjects;
        [SerializeField] private TextMeshProUGUI _bulletCountText;
        [SerializeField] private Slider _respawnSlider;

        public void ChangeBulletCount(int newCount)
        {
            _bulletCountText.text = newCount.ToString();
        }

        public void ChangeRespawnWindowVisibility(bool isVisible)
        {
            _respawnObjects.SetActive(isVisible);
        }

        public void SetRespawnSliderValue(float value)
        {
            _respawnSlider.value = value;
        }
    }
}