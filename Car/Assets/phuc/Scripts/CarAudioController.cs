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

        if (engineIdleClip != null)
        {
            engineSource.clip = engineIdleClip;
            engineSource.loop = true;
            engineSource.playOnAwake = false;
            engineSource.Play();
           // Debug.Log($"{name}: Engine idle sound started");
        }
    }


    void Update()
    {
        if (car == null) return;

        float speed = car.wheelColliders[0].attachedRigidbody.linearVelocity.magnitude;
        float throttle = Input.GetAxis("Vertical");

        if (throttle > 0.75f && engineAccelClip != null)
        {
            if (engineSource.clip != engineAccelClip)
            {
                engineSource.clip = engineAccelClip;
                engineSource.loop = true;
                engineSource.Play();
              //  Debug.Log($"{name}: Engine accel sound started");
            }
        }
        else
        {
            if (engineSource.clip != engineIdleClip && engineIdleClip != null)
            {
                engineSource.clip = engineIdleClip;
                engineSource.loop = true;
                engineSource.Play();
               // Debug.Log($"{name}: Engine idle sound started");
            }
        }
        engineSource.pitch = Mathf.Lerp(1f, 2f, speed / 150f);
        if (car.isUsingNitro)
        {
            if (!nitroSource.isPlaying && nitroClip != null)
            {
                nitroSource.clip = nitroClip;
                nitroSource.loop = true;
                nitroSource.Play();
              //  Debug.Log($"{name}: Nitro sound started");
            }
            if (driftSource.isPlaying)
            {
                driftSource.Stop();
               // Debug.Log($"{name}: Drift sound stopped (nitro active)");
            }
        }
        else
        {
            if (nitroSource.isPlaying)
            {
                nitroSource.Stop();
               // Debug.Log($"{name}: Nitro sound stopped");
            }
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.7f && car.raceStarted)
            {
                if (!driftSource.isPlaying && driftClip != null)
                {
                    driftSource.clip = driftClip;
                    driftSource.loop = true;
                    driftSource.Play();
                   // Debug.Log($"{name}: Drift sound started");
                }
            }
            else
            {
                if (driftSource.isPlaying)
                {
                    driftSource.Stop();
                   // Debug.Log($"{name}: Drift sound stopped");
                }
            }
        }
    }


    public void PlayPickupSound()
    {
        if (pickupSource != null && pickupClip != null)
        {
            pickupSource.PlayOneShot(pickupClip);
           // Debug.Log($"{name}: Pickup sound played");
        }
    }
}
