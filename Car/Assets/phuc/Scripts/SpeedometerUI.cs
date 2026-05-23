using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedometerUI : MonoBehaviour
{
    public Rigidbody carRigidbody;   
    public RectTransform needle;     
    public TMP_Text speedText;       

    public float maxSpeed = 200f;   
    public float minAngle = -130f;   
    public float maxAngle = 130f;    

    void Update()
    {
       
        float speed = carRigidbody.linearVelocity.magnitude * 3.6f;

        float angle = Mathf.Lerp(minAngle, maxAngle, speed / maxSpeed);

        needle.localRotation = Quaternion.Euler(0, 0, angle);

        if (speedText != null)
            speedText.text = Mathf.RoundToInt(speed) + " km/h";
    }
}
