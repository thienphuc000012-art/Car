using UnityEngine;
using Unity.Cinemachine; 

public class CarCameraController : MonoBehaviour
{
    public Rigidbody carRb;
    public CinemachineCamera vcam;
    private CinemachineFollow follow;
    private CinemachineImpulseSource impulseSource; 
    public float baseFOV = 60f;
    public float maxFOV = 80f;
    public float speedForMaxFOV = 200f;

    void Start()
    {
        follow = vcam.GetComponent<CinemachineFollow>();
        impulseSource = vcam.GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        float speed = carRb.linearVelocity.magnitude * 3.6f; 
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speed / speedForMaxFOV);
        vcam.Lens.FieldOfView = targetFOV;
        if (impulseSource != null && speed > 100f)
        {
            impulseSource.GenerateImpulse();
        }
    }
}
