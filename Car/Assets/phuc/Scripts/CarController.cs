using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider[] wheelColliders = new WheelCollider[4]; 
    public Transform[] wheelMeshes = new Transform[4];
    public float maxMotorTorque = 1500f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 3000f;

    public ParticleSystem nitroPrefab;
    public ParticleSystem exhaustPrefab;
    public ParticleSystem[] tireSmokePrefabs;

    public Transform nitroPoint;
    public Transform exhaustPoint;
    public Transform[] tireSmokePoints;

    private ParticleSystem nitroVFX;
    private ParticleSystem exhaustVFX;
    private ParticleSystem[] tireSmokeVFX;

    void Start()
    {
        if (nitroPrefab != null && nitroPoint != null)
        {
            nitroVFX = Instantiate(nitroPrefab, nitroPoint.position, nitroPoint.rotation, nitroPoint);
        }

        if (exhaustPrefab != null && exhaustPoint != null)
        {
            exhaustVFX = Instantiate(exhaustPrefab, exhaustPoint.position, exhaustPoint.rotation, exhaustPoint);
        }

        int count = Mathf.Min(tireSmokePoints.Length, tireSmokePrefabs.Length, wheelMeshes.Length);
        tireSmokeVFX = new ParticleSystem[count];

        for (int i = 0; i < count; i++)
        {
            if (tireSmokePrefabs[i] != null && wheelMeshes[i] != null)
            {
                tireSmokeVFX[i] = Instantiate(
                    tireSmokePrefabs[i],
                    wheelMeshes[i].position,
                    wheelMeshes[i].rotation,
                    wheelMeshes[i]  
                );

                tireSmokeVFX[i].transform.localPosition = Vector3.zero;
                tireSmokeVFX[i].transform.localRotation = Quaternion.identity;
            }
        }
    }


    void Update()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteerAngle * Input.GetAxis("Horizontal");

        ApplyDrive(motor, steering);
        UpdateWheelMeshes();
        HandleVFX();
    }

    void ApplyDrive(float motor, float steering)
    {
        wheelColliders[0].steerAngle = steering;
        wheelColliders[1].steerAngle = steering;

        wheelColliders[2].motorTorque = motor;
        wheelColliders[3].motorTorque = motor;

        if (Input.GetKey(KeyCode.Space))
        {
            wheelColliders[2].brakeTorque = brakeForce;
            wheelColliders[3].brakeTorque = brakeForce;
        }
        else
        {
            wheelColliders[2].brakeTorque = 0;
            wheelColliders[3].brakeTorque = 0;
        }
    }

    void UpdateWheelMeshes()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheelColliders[i].GetWorldPose(out pos, out quat);
            wheelMeshes[i].position = pos;
            wheelMeshes[i].rotation = quat;
        }
    }

    void HandleVFX()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (nitroVFX != null && !nitroVFX.isPlaying) nitroVFX.Play();
        }
        else
        {
            if (nitroVFX != null && nitroVFX.isPlaying) nitroVFX.Stop();
        }

        if (Input.GetAxis("Vertical") > 0.8f)
        {
            if (exhaustVFX != null && !exhaustVFX.isPlaying) exhaustVFX.Play();
        }
        else
        {
            if (exhaustVFX != null && exhaustVFX.isPlaying) exhaustVFX.Stop();
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.7f)
        {
            for (int i = 0; i < tireSmokeVFX.Length; i++)
            {
                if (tireSmokeVFX[i] != null && !tireSmokeVFX[i].isPlaying)
                    tireSmokeVFX[i].Play();
            }
        }
        else
        {
            for (int i = 0; i < tireSmokeVFX.Length; i++)
            {
                if (tireSmokeVFX[i] != null && tireSmokeVFX[i].isPlaying)
                    tireSmokeVFX[i].Stop();
            }
        }

    }
}
