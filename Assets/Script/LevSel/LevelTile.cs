
using UnityEngine;

public class LevelTile : MonoBehaviour
{
    public string sceneName = "BossRoom1";

    private bool playerNearby;
    private CircleTransition transition;
    private bool isLoading;

    private void Start()
    {
        transition = FindFirstObjectByType<CircleTransition>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Player masuk area level! Tekan J.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.J) && !isLoading)
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
