
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    public GameObject menang, kalah;
    public static ResultUI Instance;

    [Header("Level Progress")]
    public int completedLevelIndex = 0;

    public float waitTime = 2f;
    bool startWaiting = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (startWaiting)
        {
            waitTime -= Time.deltaTime;

            if (waitTime <= 0)
            {
                Time.timeScale = 0f;
                startWaiting = false;
            }
        }
    }

    public void MenangUI()
    {
        menang.SetActive(true);
        kalah.SetActive(false);

        // Simpan progress dan buka level berikutnya.
        LevelProgress.CompleteLevel(completedLevelIndex);

        startWaiting = true;
    }

    public void KalahUI()
    {
        kalah.SetActive(true);
        menang.SetActive(false);

        startWaiting = true;
    }
}
