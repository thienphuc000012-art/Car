using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip menuMusic;

    public AudioClip startupCarSfx;
    public AudioClip buttonClickSFX;
    public AudioClip hoverSFX;

    const string MusicVolumeKey = "MusicVolume";
    const string SFXVolumeKey = "SFXVolume";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSources();

        // Đăng ký sự kiện
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Sửa lỗi ở đây
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXVolumeKey, 0.8f));
        PlayMenuMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsRaceScene(scene.name))
        {
            StopMenuMusic();
            Debug.Log("Đã tắt nhạc menu khi vào race scene: " + scene.name);
        }
        else
        {
            PlayMenuMusic();
        }
    }

    private bool IsRaceScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        string name = sceneName.ToLower();
        return name.Contains("track") ||
               name.Contains("race") ||
               name.Contains("demo") ||
               name == "complete_track_demo";
    }

    public void PlayMenuMusic()
    {
        if (menuMusic == null || musicSource == null) return;

        if (musicSource.isPlaying && musicSource.clip == menuMusic)
            return;

        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMenuMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    // ==================== CÁC HÀM KHÁC ====================
    public void PlayStartupCarSfx()
    {
        if (startupCarSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(startupCarSfx);
    }

    public void PlayButtonClick()
    {
        if (buttonClickSFX != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClickSFX);
    }

    public void PlayHover()
    {
        if (hoverSFX != null && sfxSource != null)
            sfxSource.PlayOneShot(hoverSFX);
    }

    public void SetMusicVolume(float value)
    {
        EnsureSources();
        musicSource.volume = value;
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        EnsureSources();
        sfxSource.volume = value;
        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
    }

    void EnsureSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (musicSource == null)
            musicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }
}