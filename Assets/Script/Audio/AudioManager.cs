
using UnityEngine;

[System.Serializable]
public class AudioEntry
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM List")]
    public AudioEntry[] bgmList;

    [Header("SFX List")]
    public AudioEntry[] sfxList;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float bgmVolume = 1f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Header("SFX Audio Source Pool")]
    public int sfxSourceCount = 10;

    private AudioSource bgmSource;
    private AudioSource[] sfxSources;
    private int currentSFXSource = 0;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM Audio Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        // SFX Audio Source Pool
        sfxSources = new AudioSource[sfxSourceCount];

        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
            sfxSources[i].loop = false;
            sfxSources[i].spatialBlend = 0f;
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlayBGM("Menu");
    }

    // Memutar BGM berdasarkan ID
    public void PlayBGM(string id)
    {
        AudioEntry audio = FindAudio(bgmList, id);

        if (audio == null || audio.clip == null)
            return;

        // Tidak mengulang BGM jika musik yang sama sedang diputar
        if (bgmSource.clip == audio.clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = audio.clip;
        bgmSource.volume = audio.volume * bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // Menghentikan BGM
    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    // Memutar SFX berdasarkan ID
    public void PlaySFX(string id)
    {
        AudioEntry audio = FindAudio(sfxList, id);

        if (audio == null || audio.clip == null)
            return;

        AudioSource source = sfxSources[currentSFXSource];

        source.PlayOneShot(audio.clip, audio.volume * sfxVolume);

        currentSFXSource++;

        if (currentSFXSource >= sfxSources.Length)
            currentSFXSource = 0;
    }

    // Mencari audio berdasarkan ID
    private AudioEntry FindAudio(AudioEntry[] audioList, string id)
    {
        foreach (AudioEntry audio in audioList)
        {
            if (audio.id == id)
                return audio;
        }

        Debug.LogWarning("Audio dengan ID '" + id + "' tidak ditemukan!");
        return null;
    }

    // Mengatur volume BGM
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    // Mengatur volume SFX
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
