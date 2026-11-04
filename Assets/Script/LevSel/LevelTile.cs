
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTile : MonoBehaviour
{
    public string sceneName = "BossRoom1";

    private bool playerNearby;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Ada yang menyentuh tile: " + other.name);

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
            Debug.Log("Player keluar area level.");
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("J ditekan! Membuka scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}
