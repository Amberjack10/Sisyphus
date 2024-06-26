using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image _healthBar;
    [SerializeField] TextMeshProUGUI _healthText;
    [SerializeField] Image _expBar;
    [SerializeField] TextMeshProUGUI _shieldText;

    Player _player;
    InventoryController _inventoryController;

    void Start()
    {
        _player = GameManager.Instance.Player;
        _inventoryController = InventoryController.Instance;
        _inventoryController.nextStage += SetHealthBar;
        _player.PlayerHealthChange += UpdateHealthBar;
        _healthText.text = $"{_player.currentStat.health} / {_player.currentStat.maxHealth}";
        _player.PlayerExpChange += UpdateExpBar;

        _player.PlayerSheildChange += UpdateShield;
    }

    void SetHealthBar()
    {
        _healthText.text = $"{(int)_player.currentStat.health} / {_player.currentStat.maxHealth}";
        _healthBar.fillAmount = _player.currentStat.health / _player.currentStat.maxHealth;
    }

    void UpdateHealthBar(float maxHealth, float health)
    {
        _healthText.text = $"{(int)health} / {maxHealth}";
        _healthBar.fillAmount = health / maxHealth;
    }

    void UpdateExpBar(float exp, float maxExp)
    {
        _expBar.fillAmount = exp / maxExp;
    }

    void UpdateShield(float value)
    {
        if (value <= 0)
        {
            _shieldText.text = "";
        }
        else
        {
            _shieldText.text = value.ToString("N0");
        }
    }

    private void OnDisable()
    {
        _inventoryController.nextStage -= SetHealthBar;
        _player.PlayerHealthChange -= UpdateHealthBar;
        _player.PlayerExpChange -= UpdateExpBar;
        _player.PlayerSheildChange -= UpdateShield;
    }
}
