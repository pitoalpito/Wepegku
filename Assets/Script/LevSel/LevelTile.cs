
using UnityEngine;

public class LevelTile : MonoBehaviour
{
    [Header("Level Settings")]
    public string sceneName = "BossRoom1";
    public int levelIndex = 0;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite defaultSprite;
    public Sprite selectedSprite;
    public Sprite lockedSprite;

    [Header("Level Text")]
    public GameObject levelText;

    private bool playerNearby;
    private CircleTransition transition;
    private bool isLoading;
    private bool isUnlocked;

    private void Start()
    {
        transition = FindFirstObjectByType<CircleTransition>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        CheckUnlock();
        UpdateSprite();

        if (levelText != null)
            levelText.SetActive(false);
    }

    private void CheckUnlock()
    {
        isUnlocked = LevelProgress.IsLevelUnlocked(levelIndex);
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;

        if (!isUnlocked)
            spriteRenderer.sprite = lockedSprite;
        else if (playerNearby)
            spriteRenderer.sprite = selectedSprite;
        else
            spriteRenderer.sprite = defaultSprite;

        // Tampilkan tulisan hanya jika level terbuka dan diinjak.
        if (levelText != null)
            levelText.SetActive(playerNearby && isUnlocked);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            UpdateSprite();

            if (isUnlocked)
                Debug.Log("Player masuk area level! Tekan J.");
            else
                Debug.Log("Level ini masih terkunci!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            UpdateSprite();
        }
    }

    private void Update()
    {
        if (playerNearby && isUnlocked &&
            Input.GetKeyDown(KeyCode.J) && !isLoading)
        {
            if (transition != null)
            {
                isLoading = true;
                transition.LoadLevel(sceneName);
            }
            else
            {
                Debug.LogError("CircleTransition tidak ditemukan!");
            }
        }
    }
}
