using UnityEngine;

public class ResultUI : MonoBehaviour
{
    public GameObject menang, kalah;
    public static ResultUI Instance;
    public float waitTime = 2f; // Time to wait before pausing the game
    bool startWaiting = false;

    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (startWaiting)
        {
            waitTime -= Time.deltaTime; // Decrease the wait time
            if (waitTime <= 0)
            {
                Time.timeScale = 0f; // Pause the game
                startWaiting = false; // Stop waiting after pausing
            }
        }
    }

    public void MenangUI()
    {
        menang.SetActive(true);
        kalah.SetActive(false);
        startWaiting = true; // Start waiting
    }

    public void KalahUI()
    {
        kalah.SetActive(true);
        menang.SetActive(false);
        startWaiting = true; // Start waiting
    }

}
