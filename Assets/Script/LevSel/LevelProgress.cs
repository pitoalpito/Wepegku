
using UnityEngine;

public static class LevelProgress
{
    // Level pertama menggunakan index 0.
    // Nilai ini menunjukkan index level tertinggi yang sudah terbuka.
    private const string UnlockKey = "HighestUnlockedLevel";

    public static int HighestUnlockedLevel
    {
        get { return PlayerPrefs.GetInt(UnlockKey, 0); }
    }

    public static void CompleteLevel(int completedLevelIndex)
    {
        int nextLevelIndex = completedLevelIndex + 1;

        if (nextLevelIndex > HighestUnlockedLevel)
        {
            PlayerPrefs.SetInt(UnlockKey, nextLevelIndex);
            PlayerPrefs.Save();

            Debug.Log("Level " + nextLevelIndex + " berhasil dibuka!");
        }
    }

    public static bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= HighestUnlockedLevel;
    }
}
