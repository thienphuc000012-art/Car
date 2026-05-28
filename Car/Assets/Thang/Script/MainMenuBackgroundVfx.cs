using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBackgroundVfx : MonoBehaviour
{
    public enum VfxPreset
    {
        LoadingSnowRoad,
        PressAnyKeySunsetTrack
    }

    [Header("Preset")]
    public VfxPreset preset;
    public bool rebuildOnStart = true;

    [Header("General")]
    public int seed = 1993;
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Header("Snow")]
    [Range(0, 300)] public int snowCount = 130;
    public Vector2 snowDirection = new Vector2(-520f, -760f);

    [Header("Dust / Smoke")]
    [Range(0, 180)] public int dustCount = 80;

    [Header("Press Any Key Tuning")]
    public bool rebuildWhenValuesChangeInEditMode;
    public bool rebuildWhenValuesChangeInPlayMode = true;
    public PressSunSettings pressSun = PressSunSettings.Default();
    public NitroFxSettings[] pressNitros = DefaultNitros();
    public TailLightPairSettings[] pressTailLightPairs = DefaultTailLights();
    public DustTrailSettings[] pressDustTrails = DefaultDustTrails();

    readonly List<Image> particles = new List<Image>();
    readonly List<ParticleState> particleStates = new List<ParticleState>();
    readonly List<PulseLight> pulseLights = new List<PulseLight>();
    readonly List<RectTransform> sunRays = new List<RectTransform>();
    readonly List<Image> sunRayImages = new List<Image>();
    readonly List<float> sunRayBaseAngles = new List<float>();
    readonly List<float> sunRayBaseAlphas = new List<float>();
    Sprite softCircleSprite;
    Sprite streakSprite;
    Sprite whiteSprite;
    Sprite sunRaySprite;
    RectTransform rectTransform;

    struct ParticleState
    {
        public Vector2 velocity;
        public Vector2 bounds;
        public float baseAlpha;
        public float pulseSpeed;
        public float phase;
        public float spin;
    }

    class PulseLight
    {
        public Image image;
        public RectTransform rect;
        public float minAlpha;
        public float maxAlpha;
        public float speed;
        public float phase;
        public float scalePulse;
    }

    [System.Serializable]
    public struct PressSunSettings
    {
        public Vector2 center;
        [Range(0.2f, 4f)] public float glowScale;
        [Range(0.2f, 4f)] public float rayLengthScale;
        [Range(0.2f, 5f)] public float rayThicknessScale;
        [Range(0.1f, 3f)] public float rayAlphaScale;
        [Range(0.05f, 0.7f)] public float rayOffsetFactor;
        [Range(0f, 12f)] public float raySwayDegrees;
        [Range(0.05f, 3f)] public float raySwaySpeed;

        public static PressSunSettings Default()
        {
            return new PressSunSettings
            {
                center = new Vector2(-135f, 322f),
                glowScale = 1.25f,
                rayLengthScale = 1.55f,
                rayThicknessScale = 2.25f,
                rayAlphaScale = 1.7f,
                rayOffsetFactor = 0.34f,
                raySwayDegrees = 3.5f,
                raySwaySpeed = 0.42f
            };
        }
    }

    [System.Serializable]
    public struct NitroFxSettings
    {
        public string name;
        public bool enabled;
        public Vector2 position;
        public float rotationZ;
        [Range(0.05f, 3f)] public float scale;
        public float phase;
    }

    [System.Serializable]
    public struct TailLightPairSettings
    {
        public string name;
        public bool enabled;
        public Vector2 leftPosition;
        public Vector2 rightPosition;
        public float rotationZ;
        [Range(0.05f, 3f)] public float scale;
        public float phase;
    }

    [System.Serializable]
    public struct DustTrailSettings
    {
        public string name;
        public bool enabled;
        public Vector2 origin;
        public Vector2 baseSize;
        public float rotationZ;
        public Vector2 drift;
        [Range(0.05f, 3f)] public float scale;
        [Range(0.05f, 3f)] public float alphaScale;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (rebuildOnStart)
            Rebuild();
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        if (Application.isPlaying)
        {
            if (rebuildWhenValuesChangeInPlayMode)
                Rebuild();
        }
        else if (rebuildWhenValuesChangeInEditMode)
        {
            Rebuild();
        }
    }

    void Update()
    {
        float time = Time.unscaledTime;

        for (int i = 0; i < particles.Count; i++)
        {
            Image particle = particles[i];
            if (particle == null)
                continue;

            ParticleState state = particleStates[i];
            RectTransform particleRect = particle.rectTransform;
            particleRect.anchoredPosition += state.velocity * Time.unscaledDeltaTime;

            Vector2 pos = particleRect.anchoredPosition;
            if (pos.x < -state.bounds.x || pos.x > state.bounds.x || pos.y < -state.bounds.y || pos.y > state.bounds.y)
            {
                pos.x = Random.Range(-state.bounds.x, state.bounds.x);
                pos.y = state.bounds.y + Random.Range(0f, 120f);
                particleRect.anchoredPosition = pos;
            }

            if (state.spin != 0f)
                particleRect.Rotate(0f, 0f, state.spin * Time.unscaledDeltaTime);

            if (state.pulseSpeed > 0f)
            {
                Color color = particle.color;
                color.a = state.baseAlpha * Mathf.Lerp(0.65f, 1f, Mathf.Abs(Mathf.Sin(time * state.pulseSpeed + state.phase)));
                particle.color = color;
            }
        }

        foreach (PulseLight light in pulseLights)
        {
            if (light.image == null)
                continue;

            float t = Mathf.Abs(Mathf.Sin(time * light.speed + light.phase));
            Color color = light.image.color;
            color.a = Mathf.Lerp(light.minAlpha, light.maxAlpha, t);
            light.image.color = color;

            if (light.scalePulse > 0f)
            {
                float scale = 1f + light.scalePulse * t;
                light.rect.localScale = new Vector3(scale, scale, 1f);
            }
        }

        for (int i = 0; i < sunRays.Count; i++)
        {
            RectTransform ray = sunRays[i];
            if (ray == null)
                continue;

            float swaySpeed = pressSun.raySwaySpeed > 0f ? pressSun.raySwaySpeed : 0.42f;
            float swayDegrees = pressSun.raySwayDegrees > 0f ? pressSun.raySwayDegrees : 3.5f;
            float sway = Mathf.Sin(time * swaySpeed + i * 0.55f) * swayDegrees;
            ray.localRotation = Quaternion.Euler(0f, 0f, sunRayBaseAngles[i] + sway);

            Image rayImage = sunRayImages[i];
            if (rayImage != null)
            {
                Color color = rayImage.color;
                color.a = Mathf.Clamp01(sunRayBaseAlphas[i] * Mathf.Lerp(0.72f, 1.12f, Mathf.Abs(Mathf.Sin(time * 0.55f + i * 0.35f))));
                rayImage.color = color;
            }
        }
    }

    [ContextMenu("Rebuild VFX")]
    public void Rebuild()
    {
        EnsureSprites();
        ClearChildren();
        particles.Clear();
        particleStates.Clear();
        pulseLights.Clear();
        sunRays.Clear();
        sunRayImages.Clear();
        sunRayBaseAngles.Clear();
        sunRayBaseAlphas.Clear();

        Random.InitState(seed);

        if (preset == VfxPreset.LoadingSnowRoad)
            BuildLoadingSnowRoad();
        else
            BuildPressAnyKeySunsetTrack();
    }

    [ContextMenu("Reset Press Any Key Tuning")]
    public void ResetPressAnyKeyTuning()
    {
        pressSun = PressSunSettings.Default();
        pressNitros = DefaultNitros();
        pressTailLightPairs = DefaultTailLights();
        pressDustTrails = DefaultDustTrails();
    }

    static NitroFxSettings[] DefaultNitros()
    {
        return new[]
        {
            new NitroFxSettings { name = "PlayerLeft", enabled = true, position = new Vector2(-363f, -396f), rotationZ = 3f, scale = 0.88f, phase = 0f },
            new NitroFxSettings { name = "PlayerRight", enabled = true, position = new Vector2(-235f, -388f), rotationZ = -2f, scale = 0.88f, phase = 0.75f },
            new NitroFxSettings { name = "RightCar", enabled = true, position = new Vector2(235f, -206f), rotationZ = -24f, scale = 0.45f, phase = 1.45f }
        };
    }

    static TailLightPairSettings[] DefaultTailLights()
    {
        return new[]
        {
            new TailLightPairSettings { name = "PlayerCar", enabled = true, leftPosition = new Vector2(-380f, -304f), rightPosition = new Vector2(-226f, -296f), rotationZ = 0f, scale = 0.9f, phase = 0.1f },
            new TailLightPairSettings { name = "RightCar", enabled = true, leftPosition = new Vector2(206f, -162f), rightPosition = new Vector2(286f, -157f), rotationZ = -5f, scale = 0.62f, phase = 1.2f },
            new TailLightPairSettings { name = "FarCar", enabled = true, leftPosition = new Vector2(-70f, -90f), rightPosition = new Vector2(-14f, -88f), rotationZ = 0f, scale = 0.42f, phase = 2.1f }
        };
    }

    static DustTrailSettings[] DefaultDustTrails()
    {
        return new[]
        {
            new DustTrailSettings { name = "PlayerCar", enabled = true, origin = new Vector2(-292f, -332f), baseSize = new Vector2(300f, 82f), rotationZ = -8f, drift = new Vector2(-18f, -22f), scale = 0.95f, alphaScale = 1f },
            new DustTrailSettings { name = "RightCar", enabled = true, origin = new Vector2(236f, -176f), baseSize = new Vector2(190f, 56f), rotationZ = -14f, drift = new Vector2(22f, -9f), scale = 0.66f, alphaScale = 1f },
            new DustTrailSettings { name = "FarCar", enabled = true, origin = new Vector2(-44f, -112f), baseSize = new Vector2(145f, 38f), rotationZ = -5f, drift = new Vector2(4f, -8f), scale = 0.44f, alphaScale = 1f }
        };
    }

    void BuildLoadingSnowRoad()
    {
        CreateSnow(snowCount, new Color(0.92f, 0.96f, 1f, 0.72f), 8f, 36f, 0.65f);

        // Road signs and reflective banners glowing through snow.
        CreateGlow("SignGlow_LeftArrow", new Vector2(-820f, -95f), new Vector2(115f, 75f), new Color(1f, 0.72f, 0.23f, 0.75f), 0.25f, 0.9f, 2.2f, 0.2f, 0.1f);
        CreateGlow("SignGlow_MidArrowA", new Vector2(-280f, -28f), new Vector2(70f, 45f), new Color(1f, 0.78f, 0.32f, 0.55f), 0.18f, 0.65f, 1.7f, 1.5f, 0.08f);
        CreateGlow("SignGlow_MidArrowB", new Vector2(40f, 40f), new Vector2(60f, 42f), new Color(1f, 0.78f, 0.32f, 0.45f), 0.14f, 0.55f, 1.5f, 2.2f, 0.08f);
        CreateGlow("BannerGlow_Right", new Vector2(775f, 250f), new Vector2(190f, 390f), new Color(0.65f, 0.82f, 1f, 0.3f), 0.06f, 0.28f, 0.9f, 1.1f, 0.03f);

        // Headlight bloom in the storm and tail lights blinking on cars.
        CreateGlow("HeadlightBloom_FarCar", new Vector2(420f, 48f), new Vector2(120f, 65f), new Color(1f, 0.94f, 0.78f, 0.9f), 0.45f, 0.95f, 1.4f, 0.5f, 0.06f);
        CreateGlow("TailLight_Left", new Vector2(-372f, -260f), new Vector2(95f, 45f), new Color(1f, 0.06f, 0.02f, 0.9f), 0.22f, 0.9f, 5.5f, 0f, 0.14f);
        CreateGlow("TailLight_Right", new Vector2(-188f, -250f), new Vector2(95f, 45f), new Color(1f, 0.06f, 0.02f, 0.9f), 0.22f, 0.9f, 5.5f, 0.45f, 0.14f);
        CreateGlow("TailLight_FarRight", new Vector2(520f, -18f), new Vector2(70f, 38f), new Color(1f, 0.08f, 0.02f, 0.65f), 0.14f, 0.6f, 4.8f, 1.0f, 0.1f);
    }

    void BuildPressAnyKeySunsetTrack()
    {
        Vector2 sunCenter = pressSun.center;
        float glowScale = Mathf.Max(0.05f, pressSun.glowScale);

        // Layered sun glow: small hot core, warm bloom, and soft rays along the image perspective.
        CreateGlow("SunWhiteCore", sunCenter, new Vector2(175f, 175f) * glowScale, new Color(1f, 0.98f, 0.82f, 1f), 0.78f, 1f, 0.72f, 0f, 0.03f);
        CreateGlow("SunWhiteGlare", sunCenter + new Vector2(4f, -2f), new Vector2(285f, 220f) * glowScale, new Color(1f, 0.96f, 0.7f, 0.86f), 0.38f, 0.86f, 0.62f, 0.35f, 0.035f);
        CreateGlow("SunGoldCore", sunCenter, new Vector2(420f, 370f) * glowScale, new Color(1f, 0.68f, 0.25f, 0.9f), 0.5f, 0.9f, 0.58f, 0.7f, 0.03f);
        CreateGlow("SunWarmBloom", sunCenter + new Vector2(28f, -18f) * glowScale, new Vector2(1280f, 760f) * glowScale, new Color(1f, 0.58f, 0.22f, 0.38f), 0.18f, 0.38f, 0.5f, 1.1f, 0.018f);
        CreateGlow("SunMountainBackLight", sunCenter + new Vector2(-95f, -36f) * glowScale, new Vector2(760f, 390f) * glowScale, new Color(1f, 0.78f, 0.42f, 0.26f), 0.09f, 0.26f, 0.48f, 1.6f, 0.02f);
        CreateGlow("SunTrackHaze", sunCenter + new Vector2(185f, -212f) * glowScale, new Vector2(920f, 190f) * glowScale, new Color(1f, 0.7f, 0.36f, 0.24f), 0.09f, 0.24f, 0.55f, 1.9f, 0.02f);
        CreateSunRays(sunCenter + new Vector2(-8f, -4f), new Color(1f, 0.8f, 0.38f, 0.16f));

        if (pressNitros != null)
        {
            foreach (NitroFxSettings nitro in pressNitros)
            {
                if (nitro.enabled)
                    CreateNitroJet("Nitro_" + nitro.name, nitro.position, nitro.rotationZ, nitro.phase, nitro.scale);
            }
        }

        if (pressTailLightPairs != null)
        {
            foreach (TailLightPairSettings tailLights in pressTailLightPairs)
            {
                if (tailLights.enabled)
                    CreateTailLightPair(tailLights.name, tailLights.leftPosition, tailLights.rightPosition, tailLights.rotationZ, tailLights.scale, tailLights.phase);
            }
        }

        CreateDust(dustCount);
    }

    void CreateSnow(int count, Color color, float minSize, float maxSize, float speedJitter)
    {
        Vector2 bounds = referenceResolution * 0.58f;

        for (int i = 0; i < count; i++)
        {
            bool streak = Random.value > 0.45f;
            Image snow = CreateImage("Snow_" + i, streak ? streakSprite : softCircleSprite, transform);
            float size = Random.Range(minSize, maxSize);
            snow.rectTransform.sizeDelta = streak ? new Vector2(size * Random.Range(0.45f, 0.75f), size * Random.Range(2.2f, 4.2f)) : new Vector2(size, size);
            snow.rectTransform.anchoredPosition = new Vector2(Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y));
            snow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-38f, -20f));
            color.a = Random.Range(0.25f, 0.82f);
            snow.color = color;

            particles.Add(snow);
            particleStates.Add(new ParticleState
            {
                velocity = snowDirection * Random.Range(0.45f, 1.25f) * Random.Range(1f - speedJitter * 0.2f, 1f + speedJitter),
                bounds = bounds,
                baseAlpha = color.a,
                pulseSpeed = Random.Range(0.35f, 1.2f),
                phase = Random.Range(0f, 6.28f),
                spin = Random.Range(-12f, 12f)
            });
        }
    }

    void CreateTailLightPair(string name, Vector2 leftPosition, Vector2 rightPosition, float rotationZ, float scale, float phase)
    {
        CreateTailLight(name + "_Left", leftPosition, rotationZ, scale, phase);
        CreateTailLight(name + "_Right", rightPosition, rotationZ, scale, phase + 0.42f);
    }

    void CreateTailLight(string name, Vector2 position, float rotationZ, float scale, float phase)
    {
        CreateGlow(name + "_Reflection", position + new Vector2(0f, -10f * scale), new Vector2(95f * scale, 32f * scale), new Color(1f, 0.04f, 0.015f, 0.28f), 0.03f, 0.24f, 4.6f, phase, 0.04f, rotationZ);
        CreateGlow(name + "_Halo", position, new Vector2(62f * scale, 26f * scale), new Color(1f, 0.03f, 0.01f, 0.58f), 0.08f, 0.48f, 5.2f, phase + 0.25f, 0.06f, rotationZ);
        CreateGlow(name + "_Core", position, new Vector2(24f * scale, 11f * scale), new Color(1f, 0.18f, 0.05f, 0.95f), 0.45f, 0.95f, 5.9f, phase + 0.5f, 0.08f, rotationZ);
    }

    void CreateNitroJet(string name, Vector2 position, float rotationZ, float phase, float scale)
    {
        CreateGlow(name + "_BlueOuter", position, new Vector2(46f * scale, 128f * scale), new Color(0.24f, 0.82f, 1f, 0.72f), 0.13f, 0.5f, 11f, phase, 0.16f, rotationZ);
        CreateGlow(name + "_VioletHeat", position + new Vector2(0f, 10f * scale), new Vector2(28f * scale, 92f * scale), new Color(0.46f, 0.36f, 1f, 0.68f), 0.1f, 0.46f, 13f, phase + 0.5f, 0.18f, rotationZ);
        CreateGlow(name + "_HotCore", position + new Vector2(0f, 24f * scale), new Vector2(16f * scale, 54f * scale), new Color(1f, 0.58f, 0.18f, 0.85f), 0.12f, 0.58f, 15f, phase + 1.1f, 0.2f, rotationZ);
    }

    void CreateDust(int count)
    {
        if (pressDustTrails == null || pressDustTrails.Length == 0)
            return;

        int trailCount = Mathf.Max(10, count / 3);
        foreach (DustTrailSettings dustTrail in pressDustTrails)
        {
            if (!dustTrail.enabled)
                continue;

            CreateDustTrail("Dust_" + dustTrail.name, dustTrail.origin, dustTrail.baseSize, dustTrail.rotationZ, trailCount, dustTrail.drift, dustTrail.scale, dustTrail.alphaScale);
        }
    }

    void CreateDustTrail(string name, Vector2 origin, Vector2 baseSize, float rotationZ, int count, Vector2 drift, float scale, float alphaScale)
    {
        Vector2 bounds = referenceResolution * 0.58f;

        for (int i = 0; i < count; i++)
        {
            Image dust = CreateImage(name + "_" + i, softCircleSprite, transform);
            float depth = i / Mathf.Max(1f, count - 1f);
            float width = baseSize.x * Random.Range(0.55f, 1.15f) * Mathf.Lerp(1f, 0.45f, depth);
            float height = baseSize.y * Random.Range(0.55f, 1.15f) * Mathf.Lerp(1f, 0.5f, depth);
            Vector2 laneOffset = new Vector2(Random.Range(-baseSize.x * 0.22f, baseSize.x * 0.22f), Random.Range(-baseSize.y * 0.65f, baseSize.y * 0.65f));

            dust.rectTransform.sizeDelta = new Vector2(width, height) * scale;
            dust.rectTransform.anchoredPosition = origin + laneOffset + drift * Random.Range(0f, 1.2f);
            dust.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ + Random.Range(-5f, 5f));
            dust.color = new Color(0.92f, 0.72f, 0.46f, Random.Range(0.025f, 0.095f) * scale * alphaScale);

            particles.Add(dust);
            particleStates.Add(new ParticleState
            {
                velocity = drift * Random.Range(0.4f, 1.25f) + new Vector2(Random.Range(-16f, 22f), Random.Range(10f, 38f)),
                bounds = bounds,
                baseAlpha = dust.color.a,
                pulseSpeed = Random.Range(0.18f, 0.55f),
                phase = Random.Range(0f, 6.28f),
                spin = Random.Range(-0.8f, 0.8f)
            });
        }
    }

    void CreateSunRays(Vector2 center, Color color)
    {
        float[] angles =
        {
            -70f, -63f, -56f, -50f, -44f, -38f, -32f, -27f, -22f, -17f, -12f, -7f,
            -2f, 4f, 10f, 16f, 23f, 30f, 38f, 47f, 57f, 68f
        };
        float[] lengths =
        {
            2550f, 2820f, 3100f, 2960f, 3280f, 3050f, 3400f, 3200f, 3500f, 3320f, 3600f,
            3440f, 3360f, 3180f, 3020f, 2860f, 2680f, 2520f, 2360f, 2200f, 2050f, 1900f
        };
        float lengthScale = Mathf.Max(0.05f, pressSun.rayLengthScale);
        float thicknessScale = Mathf.Max(0.05f, pressSun.rayThicknessScale);
        float alphaScale = Mathf.Max(0.01f, pressSun.rayAlphaScale);
        float originInset = Mathf.Max(0f, pressSun.rayOffsetFactor * 35f);

        for (int i = 0; i < angles.Length; i++)
        {
            Image ray = CreateImage("SunRay_" + i, sunRaySprite, transform);
            ray.color = new Color(color.r, color.g, color.b, color.a * alphaScale * Random.Range(0.5f, 1.05f));
            ray.raycastTarget = false;
            RectTransform rt = ray.rectTransform;
            float length = lengths[i] * lengthScale;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(length, Random.Range(105f, 230f) * thicknessScale);
            Vector3 originOffset = Quaternion.Euler(0f, 0f, angles[i]) * new Vector3(-originInset, 0f, 0f);
            rt.anchoredPosition = center + new Vector2(originOffset.x, originOffset.y);
            rt.localRotation = Quaternion.Euler(0f, 0f, angles[i]);
            sunRays.Add(rt);
            sunRayImages.Add(ray);
            sunRayBaseAngles.Add(angles[i]);
            sunRayBaseAlphas.Add(ray.color.a);
        }
    }

    Image CreateGlow(string name, Vector2 position, Vector2 size, Color color, float minAlpha, float maxAlpha, float speed, float phase, float scalePulse)
    {
        return CreateGlow(name, position, size, color, minAlpha, maxAlpha, speed, phase, scalePulse, 0f);
    }

    Image CreateGlow(string name, Vector2 position, Vector2 size, Color color, float minAlpha, float maxAlpha, float speed, float phase, float scalePulse, float rotationZ)
    {
        Image glow = CreateImage(name, softCircleSprite, transform);
        glow.rectTransform.anchoredPosition = position;
        glow.rectTransform.sizeDelta = size;
        glow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        glow.color = new Color(color.r, color.g, color.b, maxAlpha);
        glow.raycastTarget = false;

        pulseLights.Add(new PulseLight
        {
            image = glow,
            rect = glow.rectTransform,
            minAlpha = minAlpha,
            maxAlpha = maxAlpha,
            speed = speed,
            phase = phase,
            scalePulse = scalePulse
        });

        return glow;
    }

    Image CreateImage(string name, Sprite sprite, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (!Application.isPlaying)
            obj.hideFlags = HideFlags.DontSaveInEditor;

        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    void EnsureSprites()
    {
        if (softCircleSprite == null)
            softCircleSprite = CreateSoftCircleSprite(96);

        if (streakSprite == null)
            streakSprite = CreateStreakSprite(16, 96);

        if (whiteSprite == null)
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        if (sunRaySprite == null)
            sunRaySprite = CreateSunRaySprite(512, 80);
    }

    Sprite CreateSoftCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.name = "RuntimeSoftCircle";

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha * alpha;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateStreakSprite(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.name = "RuntimeSnowStreak";

        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Abs(x - center.x) / (width * 0.5f);
                float dy = Mathf.Abs(y - center.y) / (height * 0.5f);
                float alpha = Mathf.Clamp01(1f - dx) * Mathf.Clamp01(1f - dy);
                alpha = Mathf.Pow(alpha, 1.8f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateSunRaySprite(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.name = "RuntimeSunRay";

        float centerY = (height - 1) * 0.5f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float horizontal = x / (float)(width - 1);
                float vertical = Mathf.Abs(y - centerY) / centerY;
                float startFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(horizontal * 7f));
                float endFade = Mathf.Pow(1f - horizontal, 1.35f);
                float edgeFade = Mathf.Pow(Mathf.Clamp01(1f - vertical), 2.4f);
                float alpha = startFade * endFade * edgeFade;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0f, 0.5f), 100f);
    }
}
