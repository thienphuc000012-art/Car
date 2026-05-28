using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Wheel Handles")]
    public RectTransform musicWheelHandle;
    public RectTransform sfxWheelHandle;
    public float wheelRotationPerSliderRange = 720f;
    public bool clockwiseWhenIncreasing = true;

    RectTransform activeMusicWheel;
    RectTransform activeSfxWheel;
    float lastMusicValue;
    float lastSfxValue;

    void Start()
    {
        Bind();
    }

    void Update()
    {
        UpdateWheelRotation(musicSlider, musicWheelHandle, ref activeMusicWheel, ref lastMusicValue);
        UpdateWheelRotation(sfxSlider, sfxWheelHandle, ref activeSfxWheel, ref lastSfxValue);
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
            activeMusicWheel = ResolveWheelVisual(musicSlider, musicWheelHandle);
            musicWheelHandle = musicWheelHandle != null ? musicWheelHandle : activeMusicWheel;
            PrepareWheelVisual(activeMusicWheel);
            lastMusicValue = musicSlider.value;

            if (musicSlider.onValueChanged.GetPersistentEventCount() == 0)
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            activeSfxWheel = ResolveWheelVisual(sfxSlider, sfxWheelHandle);
            sfxWheelHandle = sfxWheelHandle != null ? sfxWheelHandle : activeSfxWheel;
            PrepareWheelVisual(activeSfxWheel);
            lastSfxValue = sfxSlider.value;

            if (sfxSlider.onValueChanged.GetPersistentEventCount() == 0)
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    void UpdateWheelRotation(Slider slider, RectTransform assignedWheel, ref RectTransform activeWheel, ref float lastValue)
    {
        if (slider == null)
            return;

        activeWheel = ResolveWheelVisual(slider, assignedWheel);
        PrepareWheelVisual(activeWheel);

        if (activeWheel == null)
            return;

        float range = slider.maxValue - slider.minValue;
        if (Mathf.Approximately(range, 0f))
            return;

        float delta = (slider.value - lastValue) / range;
        if (Mathf.Approximately(delta, 0f))
            return;

        float direction = clockwiseWhenIncreasing ? -1f : 1f;
        float nextRotation = activeWheel.localEulerAngles.z + delta * wheelRotationPerSliderRange * direction;
        activeWheel.localRotation = Quaternion.Euler(0f, 0f, nextRotation);
        lastValue = slider.value;
    }

    RectTransform ResolveWheelVisual(Slider slider, RectTransform assignedWheel)
    {
        if (assignedWheel != null && assignedWheel != slider.handleRect)
            return assignedWheel;

        RectTransform handle = slider.handleRect;
        if (handle == null)
            return assignedWheel;

        Image[] images = handle.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            RectTransform imageRect = image.rectTransform;
            if (imageRect != handle)
                return imageRect;
        }

        return handle;
    }

    void PrepareWheelVisual(RectTransform wheelVisual)
    {
        if (wheelVisual == null)
            return;

        if (Mathf.Approximately(wheelVisual.pivot.x, 0.5f) && Mathf.Approximately(wheelVisual.pivot.y, 0.5f))
            return;

        Vector3 centerBefore = wheelVisual.TransformPoint(wheelVisual.rect.center);
        wheelVisual.pivot = new Vector2(0.5f, 0.5f);
        Vector3 centerAfter = wheelVisual.TransformPoint(wheelVisual.rect.center);
        wheelVisual.position += centerBefore - centerAfter;
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

