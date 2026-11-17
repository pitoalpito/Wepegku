
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health UI")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("Invincibility Frames")]
    public float iframeDuration = 1f;

    [Header("Visual Feedback")]
    public SpriteRenderer[] playerSprites;
    public float blinkInterval = 0.1f;

    public bool IsInvulnerable { get; private set; }
    public bool IsDead { get; private set; }

    private Coroutine iframeCoroutine;

    void Awake()
    {
        currentHealth = maxHealth;

        if (playerSprites == null || playerSprites.Length == 0)
        {
            playerSprites =
                GetComponentsInChildren<SpriteRenderer>();
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || IsInvulnerable || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Perbarui UI setelah terkena damage.
        UpdateHealthUI();

        Debug.Log(
            "Player terkena damage: " + damage +
            " | HP: " + currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (iframeCoroutine != null)
            StopCoroutine(iframeCoroutine);

        iframeCoroutine = StartCoroutine(IFrameRoutine());
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    IEnumerator IFrameRoutine()
    {
        IsInvulnerable = true;

        float timer = 0f;
        bool visible = true;

        while (timer < iframeDuration)
        {
            visible = !visible;

            foreach (SpriteRenderer sprite in playerSprites)
            {
                if (sprite != null)
                    sprite.enabled = visible;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        foreach (SpriteRenderer sprite in playerSprites)
        {
            if (sprite != null)
                sprite.enabled = true;
        }

        IsInvulnerable = false;
        iframeCoroutine = null;
    }

    void Die()
    {
        IsDead = true;

        Debug.Log("Player defeated!");

        ResultUI.Instance.KalahUI();

        // Nanti bisa disambungkan ke animasi mati
        // atau sistem Game Over.
    }
}
