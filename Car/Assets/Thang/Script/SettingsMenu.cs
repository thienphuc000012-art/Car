using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        Bind();
    }

    public void Bind()
    {
        musicSlider = musicSlider != null ? musicSlider : MainMenuFlow.FindSceneObject("MusicSlider")?.GetComponent<Slider>();
        sfxSlider = sfxSlider != null ? sfxSlider : MainMenuFlow.FindSceneObject("SFXSlider")?.GetComponent<Slider>();

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }
}
