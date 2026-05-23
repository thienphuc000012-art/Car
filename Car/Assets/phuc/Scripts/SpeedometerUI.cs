using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedometerUI : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public RectTransform needle;

    public float maxSpeed = 220f;    
    public float minAngle = 125.8f;   
    public float maxAngle = -125.8f;  

    void Update()
    {
        float speed = carRigidbody.linearVelocity.magnitude * 3.6f;
        speed = Mathf.Clamp(speed, 0, maxSpeed);
        float angle = Mathf.Lerp(minAngle, maxAngle, speed / maxSpeed);
        needle.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
