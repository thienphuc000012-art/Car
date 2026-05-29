using UnityEngine;

public class AudioCarManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioClip bgmClip;

    void Start()
    {
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
}
