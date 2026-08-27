using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthBarDisplay : MonoBehaviour
{
    private bool firstUpdate = true;

    public Image fillImage;

    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private float currentHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    [SerializeField] private UnityEvent onHealthDepleted;

    private void Awake()
    {
        currentHealth = maxHealth;

        Debug.Log("HealthBarDisplay Awake: currentHealth = " + currentHealth + ", maxHealth = " + maxHealth);

        UpdateFill();
    }

    public float GetHealth()
    {
        Debug.Log("Getting health bar: currentHealth = " + currentHealth + ", maxHealth = " + maxHealth);

        return currentHealth;
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);

        Debug.Log("Updating health bar: currentHealth = " + currentHealth + ", maxHealth = " + maxHealth);

        UpdateFill();

        if (currentHealth <= 0f)
        {
            onHealthDepleted?.Invoke();
        }
    }

    public void ApplyDamage(float amount)
    {
        if (firstUpdate)
        {
            firstUpdate = false;
            currentHealth = MaxHealth;
            Debug.Log("Updating health bar: firstUpdate => currentHealth = " + currentHealth + ", maxHealth = " + maxHealth);
        }

        var newHealthValue = currentHealth - amount;

        Debug.Log("Setting currentHealth to: " + newHealthValue);

        SetHealth(newHealthValue);
    }

    /*
    public void Heal(float amount)
    {
        SetHealth(currentHealth + amount);
    }
    */

    private void UpdateFill()
    {
        if (fillImage == null)
            return;

        float fillAmount = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        Debug.Log("Setting fillAmount to: " + fillAmount);

        fillImage.fillAmount = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        Debug.Log("fillAmount set to: " + fillImage.fillAmount);
    }
}
