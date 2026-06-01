using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public Slider bgmSlider;
    public Slider vfxSlider;
    public Button openButton;
    public Button backButton;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource[] vfxSources;   

    void Start()
    {

        CarAudioController[] cars = FindObjectsByType<CarAudioController>(FindObjectsSortMode.None);


        vfxSources = cars
            .SelectMany(car => new AudioSource[] {
                car.engineSource,
                car.nitroSource,
                car.driftSource,
                car.pickupSource
            })
            .Where(src => src != null) 
            .ToArray();


        if (bgmSource != null)
            bgmSlider.value = bgmSource.volume;

        if (vfxSources.Length > 0)
            vfxSlider.value = vfxSources[0].volume;

        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        vfxSlider.onValueChanged.AddListener(SetVFXVolume);

        openButton.onClick.AddListener(OpenSettings);
        backButton.onClick.AddListener(CloseSettings);

        settingsPanel.SetActive(false);
    }

    public void SetBGMVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;
    }

    public void SetVFXVolume(float value)
    {
        foreach (AudioSource src in vfxSources)
        {
            if (src != null)
                src.volume = value;
        }
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    public void RegisterCarSources(CarAudioController car)
    {
        var sources = new AudioSource[] {
        car.engineSource,
        car.nitroSource,
        car.driftSource,
        car.pickupSource
    };

        vfxSources = vfxSources.Concat(sources.Where(s => s != null)).ToArray();
    }

}
