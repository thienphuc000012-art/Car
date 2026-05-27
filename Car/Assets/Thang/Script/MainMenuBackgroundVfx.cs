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

    readonly List<Image> particles = new List<Image>();
    readonly List<ParticleState> particleStates = new List<ParticleState>();
    readonly List<PulseLight> pulseLights = new List<PulseLight>();
    readonly List<RectTransform> sunRays = new List<RectTransform>();
    Sprite softCircleSprite;
    Sprite streakSprite;
    Sprite whiteSprite;
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

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (rebuildOnStart)
            Rebuild();
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
            if (ray != null)
                ray.localRotation = Quaternion.Euler(0f, 0f, ray.localEulerAngles.z + Mathf.Sin(time * 0.3f + i) * 0.01f);
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

        Random.InitState(seed);

        if (preset == VfxPreset.LoadingSnowRoad)
            BuildLoadingSnowRoad();
        else
            BuildPressAnyKeySunsetTrack();
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
        // Sun disk, flare, and thin rays following the light direction in the image.
        CreateGlow("SunCore", new Vector2(-25f, 305f), new Vector2(260f, 260f), new Color(1f, 0.72f, 0.28f, 0.82f), 0.42f, 0.92f, 0.75f, 0f, 0.035f);
        CreateGlow("SunWideBloom", new Vector2(-18f, 305f), new Vector2(720f, 450f), new Color(1f, 0.58f, 0.22f, 0.22f), 0.09f, 0.24f, 0.55f, 1.1f, 0.02f);
        CreateSunRays(new Vector2(-25f, 300f), new Color(1f, 0.76f, 0.32f, 0.15f));

        // Nitro sputter at the rear exhausts.
        CreateGlow("NitroFlame_Left", new Vector2(-170f, -368f), new Vector2(72f, 150f), new Color(0.35f, 0.8f, 1f, 0.92f), 0.35f, 0.95f, 9f, 0f, 0.18f);
        CreateGlow("NitroFlame_Right", new Vector2(-45f, -358f), new Vector2(72f, 150f), new Color(0.35f, 0.8f, 1f, 0.92f), 0.35f, 0.95f, 9f, 0.7f, 0.18f);
        CreateGlow("NitroHotCore_Left", new Vector2(-170f, -346f), new Vector2(36f, 82f), new Color(1f, 0.55f, 0.18f, 0.95f), 0.22f, 0.75f, 12f, 1.1f, 0.15f);
        CreateGlow("NitroHotCore_Right", new Vector2(-45f, -338f), new Vector2(36f, 82f), new Color(1f, 0.55f, 0.18f, 0.95f), 0.22f, 0.75f, 12f, 1.6f, 0.15f);

        // Tail lights on closest and mid cars.
        CreateGlow("TailLight_PlayerLeft", new Vector2(-196f, -300f), new Vector2(80f, 38f), new Color(1f, 0.05f, 0.02f, 0.82f), 0.2f, 0.82f, 4.6f, 0.1f, 0.1f);
        CreateGlow("TailLight_PlayerRight", new Vector2(-23f, -292f), new Vector2(80f, 38f), new Color(1f, 0.05f, 0.02f, 0.82f), 0.2f, 0.82f, 4.6f, 0.52f, 0.1f);
        CreateGlow("TailLight_RightCar", new Vector2(315f, -150f), new Vector2(74f, 36f), new Color(1f, 0.08f, 0.03f, 0.55f), 0.12f, 0.55f, 3.8f, 1.2f, 0.08f);

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

    void CreateDust(int count)
    {
        Vector2 bounds = referenceResolution * 0.58f;
        Vector2[] origins =
        {
            new Vector2(-120f, -330f),
            new Vector2(280f, -185f),
            new Vector2(-15f, -135f)
        };

        for (int i = 0; i < count; i++)
        {
            Image dust = CreateImage("Dust_" + i, softCircleSprite, transform);
            Vector2 origin = origins[Random.Range(0, origins.Length)];
            float size = Random.Range(28f, 130f);
            dust.rectTransform.sizeDelta = new Vector2(size * Random.Range(1.2f, 2.6f), size);
            dust.rectTransform.anchoredPosition = origin + new Vector2(Random.Range(-90f, 120f), Random.Range(-60f, 70f));
            dust.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-14f, 14f));
            dust.color = new Color(0.86f, 0.68f, 0.45f, Random.Range(0.05f, 0.16f));

            particles.Add(dust);
            particleStates.Add(new ParticleState
            {
                velocity = new Vector2(Random.Range(-70f, 160f), Random.Range(18f, 85f)),
                bounds = bounds,
                baseAlpha = dust.color.a,
                pulseSpeed = Random.Range(0.25f, 0.7f),
                phase = Random.Range(0f, 6.28f),
                spin = Random.Range(-2f, 2f)
            });
        }
    }

    void CreateSunRays(Vector2 center, Color color)
    {
        float[] angles = { -28f, -17f, -7f, 8f, 19f, 31f };
        float[] lengths = { 840f, 720f, 960f, 900f, 760f, 640f };

        for (int i = 0; i < angles.Length; i++)
        {
            Image ray = CreateImage("SunRay_" + i, whiteSprite, transform);
            ray.color = color;
            ray.raycastTarget = false;
            RectTransform rt = ray.rectTransform;
            rt.sizeDelta = new Vector2(lengths[i], Random.Range(8f, 18f));
            Vector3 rotatedOffset = Quaternion.Euler(0f, 0f, angles[i]) * new Vector3(lengths[i] * 0.22f, 0f, 0f);
            rt.anchoredPosition = center + new Vector2(rotatedOffset.x, rotatedOffset.y);
            rt.localRotation = Quaternion.Euler(0f, 0f, angles[i]);
            sunRays.Add(rt);
        }
    }

    Image CreateGlow(string name, Vector2 position, Vector2 size, Color color, float minAlpha, float maxAlpha, float speed, float phase, float scalePulse)
    {
        Image glow = CreateImage(name, softCircleSprite, transform);
        glow.rectTransform.anchoredPosition = position;
        glow.rectTransform.sizeDelta = size;
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
}
