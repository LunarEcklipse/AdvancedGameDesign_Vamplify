using UnityEngine;
using TMPro;

public class HealthText : MonoBehaviour // TODO: Make all the UI updaters a subclass of each other
{
    private TextMeshProUGUI healthText;
    private int ConvertHealthToInt(float inHealth) // Converts the health to an integer. This number always needs to round up.
    {
        return Mathf.CeilToInt(inHealth);
    }

    public string FormatHealthText(int health, int maxHealth)
    {
        return $"Health: {health}/{maxHealth}";
    }

    private void Awake()
    {
        if (!TryGetComponent(out healthText))
        {
            healthText = gameObject.AddComponent<TextMeshProUGUI>();
            healthText.fontSize = 36;
            healthText.color = Color.white;
        }
        healthText.text = "";
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PlayerController.Instance.RefreshPlayerStats.AddListener(OnPlayerUIUpdate);
        OnPlayerUIUpdate();
    }

    private void OnDestroy()
    {
        PlayerController.Instance.RefreshPlayerStats.RemoveListener(OnPlayerUIUpdate);
    }
    private void OnPlayerUIUpdate()
    {
        healthText.text = FormatHealthText(ConvertHealthToInt(PlayerController.Instance.combatant.remainingHealth), ConvertHealthToInt(PlayerController.Instance.combatant.MaxHealth));
    }
}
