using UnityEngine;

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
    }

    void Start()
    {
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXVolumeKey, 0.8f));
        PlayMenuMusic();
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

    public void PlayMenuMusic()
    {
        if (menuMusic == null || musicSource == null)
            return;

        if (musicSource.clip == menuMusic && musicSource.isPlaying)
            return;

        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

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
}
