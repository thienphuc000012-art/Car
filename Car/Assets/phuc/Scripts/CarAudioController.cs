using UnityEngine;

public class CarAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource engineSource;
    public AudioSource nitroSource;
    public AudioSource driftSource;
    public AudioSource pickupSource;

    [Header("Clips")]
    public AudioClip engineIdleClip;
    public AudioClip engineAccelClip;
    public AudioClip nitroClip;
    public AudioClip driftClip;
    public AudioClip pickupClip;

    private CarController car;

  void Start()
{
    car = GetComponent<CarController>();

    if (engineSource == null) engineSource = gameObject.AddComponent<AudioSource>();
    if (nitroSource == null) nitroSource = gameObject.AddComponent<AudioSource>();
    if (driftSource == null) driftSource = gameObject.AddComponent<AudioSource>();
    if (pickupSource == null) pickupSource = gameObject.AddComponent<AudioSource>();

    AudioSettingsManager asm = FindFirstObjectByType<AudioSettingsManager>();
    if (asm != null)
    {
        asm.RegisterCarSources(this);
    }
}


    void Update()
    {
        if (car == null) return;

        float speed = car.wheelColliders[0].attachedRigidbody.linearVelocity.magnitude;
        float throttle = Input.GetAxis("Vertical");

        if (!car.raceStarted)
        {
            if (engineIdleClip != null && !engineSource.isPlaying)
                engineSource.PlayOneShot(engineIdleClip);
        }
        else
        {
            if (throttle > 0.75f && engineAccelClip != null)
            {
                if (!engineSource.isPlaying)
                    engineSource.PlayOneShot(engineAccelClip);
            }
            else
            {
                if (engineIdleClip != null && !engineSource.isPlaying)
                    engineSource.PlayOneShot(engineIdleClip);
            }
        }

        engineSource.pitch = Mathf.Lerp(1f, 2f, speed / 150f);

        if (car.isUsingNitro && nitroClip != null)
        {
            if (!nitroSource.isPlaying)
                nitroSource.PlayOneShot(nitroClip);
        }

        if (!car.isUsingNitro && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.7f && car.raceStarted)
        {
            if (!driftSource.isPlaying && driftClip != null)
                driftSource.PlayOneShot(driftClip);
        }
    }

    public void PlayPickupSound()
    {
        if (pickupSource != null && pickupClip != null)
        {
            pickupSource.PlayOneShot(pickupClip);
        }
    }
}
