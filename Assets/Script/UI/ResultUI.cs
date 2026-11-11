using UnityEngine;

public class ResultUI : MonoBehaviour
{
    public GameObject menang;
    public GameObject kalah;
    public static ResultUI Instance;

    void Awake()
    {
        Instance = this;
    }

    public void MenangUI()
    {
        menang.SetActive(true);
        kalah.SetActive(false);
        Time.timeScale = 0f; // Pause the game
    }

    public void KalahUI()
    {
        kalah.SetActive(true);
        menang.SetActive(false);
        Time.timeScale = 0f; // Pause the game
    }

}
