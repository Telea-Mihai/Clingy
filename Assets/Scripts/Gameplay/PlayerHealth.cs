using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthRegenRate = 5f;
    public float regenDelay = 3f;
    [Header("UI References")]
    public Slider healthBar;
    public Image healthFill;
    public Image damageOverlay;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip healSound;
    public AudioClip deathSound;
    private bool isDead = false;
    private float lastDamageTime;
    private GameManager gameManager;
    void Start()
    {
        currentHealth = maxHealth;
        gameManager = FindFirstObjectByType<GameManager>();
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 0f;
            damageOverlay.color = overlayColor;
        }
    }
    void Update()
    {
        if (currentHealth < maxHealth && Time.time - lastDamageTime > regenDelay && !isDead)
        {
            float regenAmount = healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            UpdateHealthUI();
            if (Random.Range(0f, 1f) < 0.01f && healSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(healSound, 0.3f);
            }
        }
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = Mathf.Lerp(overlayColor.a, 0f, Time.deltaTime * 2f);
            damageOverlay.color = overlayColor;
        }
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        lastDamageTime = Time.time;
        if (audioSource != null && hurtSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hurtSound);
        }
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = Mathf.Clamp01(damage / 50f);
            damageOverlay.color = overlayColor;
        }
        UpdateHealthUI();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(float healAmount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }
    }
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        if (healthFill != null)
        {
            float healthPercent = currentHealth / maxHealth;
            if (healthPercent > 0.6f)
                healthFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthFill.color = Color.yellow;
            else
                healthFill.color = Color.red;
        }
    }
    void Die()
    {
        isDead = true;
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;
        GunController gunController = GetComponent<GunController>();
        if (gunController != null)
            gunController.enabled = false;
        if (gameManager != null)
        {
            gameManager.PlayerDied();
        }
        Debug.Log("Player died!");
    }
    public bool IsDead()
    {
        return isDead;
    }
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}
