using UnityEngine;
using TMPro;   

public class SpeedometerUI : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public RectTransform needle;
    public CarController carController;

    [Header("Speedometer Settings")]
    public float maxSpeed = 220f;
    public float minAngle = 125.8f;
    public float maxAngle = -125.8f;

    [Header("Needle Settings")]
    public float smoothTimeNormal = 0.3f;
    public float smoothTimeNitro = 0.1f;

    [Header("Odometer UI")]
    public TextMeshProUGUI distanceText;   

    private float currentAngle;
    private float angleVelocity;
    private float totalDistance;

    void Start()
    {
        // Tự tìm Player theo tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            carRigidbody = player.GetComponent<Rigidbody>();
            carController = player.GetComponent<CarController>();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GameObject với tag Player!");
        }
    }
    void Update()
    {
        float speedMS = carRigidbody.linearVelocity.magnitude; 
        float speedKMH = speedMS * 3.6f;
        totalDistance += speedMS * Time.deltaTime;
        if (distanceText != null)
        {
            distanceText.text = $"{(totalDistance / 1000f):F2} km";
        }
        speedKMH = Mathf.Clamp(speedKMH, 0, maxSpeed);

        float targetAngle = Mathf.Lerp(minAngle, maxAngle, speedKMH / maxSpeed);
        float smoothTime = (carController != null && carController.isUsingNitro)
            ? smoothTimeNitro
            : smoothTimeNormal;
        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref angleVelocity, smoothTime);
        needle.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}
