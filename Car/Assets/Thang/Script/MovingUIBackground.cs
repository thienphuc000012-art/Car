using UnityEngine;
using UnityEngine.UI;

public class MovingUIBackground : MonoBehaviour
{
    public RawImage rawImage;
    public float scrollSpeedX = 0.03f;
    public float scrollSpeedY = 0f;

    void Awake()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        if (rawImage == null)
            return;

        Rect uv = rawImage.uvRect;
        uv.x += scrollSpeedX * Time.deltaTime;
        uv.y += scrollSpeedY * Time.deltaTime;
        rawImage.uvRect = uv;
    }
}
