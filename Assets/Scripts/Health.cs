
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    void Reset() => currentHealth = maxHealth;

    public void ApplyDamage(float amount, Transform from = null)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        Debug.Log($"{name} took {amount} damage. HP={currentHealth}/{maxHealth}");
        if (currentHealth <= 0f) Die();
    }

    public void ApplyHeal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        Debug.Log($"{name} healed {amount}. HP={currentHealth}/{maxHealth}");
    }

    void Die()
    {
        Debug.Log($"{name} died.");
        // default behaviour: disable
        gameObject.SetActive(false);
    }
}