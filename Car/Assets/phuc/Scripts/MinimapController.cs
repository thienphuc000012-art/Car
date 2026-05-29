using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Camera")]
    public Camera minimapCamera;

    [Header("UI")]
    [Tooltip("Tên của RawImage trên Canvas")]
    public string minimapRawImageName = "MinimapRawImage";

    private RawImage minimapRawImage;

    void Start()
    {
        SetupMinimap();
    }

    void SetupMinimap()
    {
        if (minimapCamera == null)
        {
            minimapCamera = GetComponentInChildren<Camera>();
        }
        if (minimapRawImage == null)
        {
            GameObject uiObject = GameObject.Find(minimapRawImageName);

            if (uiObject != null)
            {
                minimapRawImage = uiObject.GetComponent<RawImage>();
            }
            else
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    minimapRawImage = canvas.GetComponentInChildren<RawImage>(true);
                }
            }
        }
        if (minimapCamera != null && minimapRawImage != null)
        {
            if (minimapCamera.targetTexture == null)
            {
                RenderTexture rt = new RenderTexture(512, 512, 24);
                minimapCamera.targetTexture = rt;
            }

            minimapRawImage.texture = minimapCamera.targetTexture;

            Debug.Log(" Minimap đã được gán tự động: " + minimapRawImage.name);
        }
        else
        {
            Debug.LogWarning(" Không tìm thấy RawImage tên: " + minimapRawImageName);
        }
    }

    void OnDestroy()
    {
        if (minimapCamera != null && minimapCamera.targetTexture != null)
        {
            minimapCamera.targetTexture.Release();
        }
    }
}