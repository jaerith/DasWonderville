using UnityEngine;
using UnityEngine.UI;

public class HealthBarDisplay : MonoBehaviour
{
    public Image fillImage;

    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateFill();
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        UpdateFill();
    }

    public void ApplyDamage(float amount)
    {
        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        SetHealth(currentHealth + amount);
    }

    private void UpdateFill()
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = maxHealth > 0f ? currentHealth / maxHealth : 0f;
    }
}
