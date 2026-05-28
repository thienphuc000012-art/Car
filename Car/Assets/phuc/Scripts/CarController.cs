using System;
using UnityEngine;
using UnityEngine.UI;

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

    public Transform[] vfxPoints;
    public Transform[] tireSmokePoints;
    private ParticleSystem[] nitroVFX;
    private ParticleSystem[] exhaustVFX;
    private ParticleSystem[] tireSmokeVFX;

    [Header("Nitro Settings")]
    public float nitroBoostMultiplier = 2f;
    public float maxNitroEnergy = 10f;
    public Slider nitroBar;

    private float nitroEnergy;
    public bool isUsingNitro;
    [Header("AI Control")]
    public bool isAI = false;

    private float aiMotor;
    private float aiSteering;
    private bool aiBrake;

    [Header("Acceleration Curve")]
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 2f, 150f, 1f);

    void Start()
    {
        Rigidbody rb = wheelColliders[0].attachedRigidbody;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        if (CompareTag("Player"))
        {
            GameObject nitroObj = GameObject.FindGameObjectWithTag("NitroUI");
            if (nitroObj != null)
            {
                nitroBar = nitroObj.GetComponent<Slider>();
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Slider Nitro với tag NitroUI!");
            }
        }

        nitroVFX = new ParticleSystem[vfxPoints.Length];
        for (int i = 0; i < vfxPoints.Length; i++)
        {
            if (nitroPrefab != null && vfxPoints[i] != null)
            {
                nitroVFX[i] = Instantiate(nitroPrefab, vfxPoints[i].position, vfxPoints[i].rotation, vfxPoints[i]);
            }
        }

        exhaustVFX = new ParticleSystem[vfxPoints.Length];
        for (int i = 0; i < vfxPoints.Length; i++)
        {
            if (exhaustPrefab != null && vfxPoints[i] != null)
            {
                exhaustVFX[i] = Instantiate(exhaustPrefab, vfxPoints[i].position, vfxPoints[i].rotation, vfxPoints[i]);
            }
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

        nitroEnergy = maxNitroEnergy;
        if (nitroBar != null)
        {
            nitroBar.maxValue = maxNitroEnergy;
            nitroBar.value = nitroEnergy;
        }
    }

    void Update()
    {
        float motor;
        float steering;

        if (!isAI)
        {
            steering = maxSteerAngle * Input.GetAxis("Horizontal");

            Rigidbody rb = wheelColliders[0].attachedRigidbody;
            float speed = rb.linearVelocity.magnitude * 3.6f;

            float accelFactor = accelerationCurve.Evaluate(speed);
            motor = maxMotorTorque * Input.GetAxis("Vertical") * accelFactor;

            isUsingNitro = Input.GetKey(KeyCode.LeftShift) && nitroEnergy > 0;
        }
        else
        {
            motor = aiMotor;
            steering = aiSteering;
            isUsingNitro = false;
        }

        ApplyDrive(motor, steering);

        UpdateWheelMeshes();
        HandleVFX();
        UpdateNitroUI();
    }

    void ApplyDrive(float motor, float steering)
    {
        Rigidbody rb = wheelColliders[0].attachedRigidbody;

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        localVel.x *= 0.9f;
        rb.linearVelocity = transform.TransformDirection(localVel);

        float speed = rb.linearVelocity.magnitude * 3.6f;
        float maxSpeed = isUsingNitro ? 300 : 150;

        if (speed > maxSpeed) motor = 0;

        float steerLimit = Mathf.SmoothStep(maxSteerAngle, 15f, speed / maxSpeed);
        float adjustedSteer = steering * (steerLimit / maxSteerAngle);
        wheelColliders[0].steerAngle = adjustedSteer;
        wheelColliders[1].steerAngle = adjustedSteer;

        if (isUsingNitro && nitroEnergy > 0)
        {
            motor *= nitroBoostMultiplier;
            nitroEnergy -= Time.deltaTime;
        }

        wheelColliders[2].motorTorque = motor;
        wheelColliders[3].motorTorque = motor;

        if ((!isAI && Input.GetKey(KeyCode.Space)) || (isAI && aiBrake))
        {
            wheelColliders[2].brakeTorque = brakeForce;
            wheelColliders[3].brakeTorque = brakeForce;
        }
        else
        {
            wheelColliders[2].brakeTorque = 0;
            wheelColliders[3].brakeTorque = 0;
        }

        float downforce = rb.linearVelocity.magnitude * 15f;
        rb.AddForce(-transform.up * downforce);
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
        if (Input.GetKey(KeyCode.LeftShift) && nitroEnergy > 0)
        {
            isUsingNitro = true;
            foreach (var vfx in nitroVFX)
                if (vfx != null && !vfx.isPlaying) vfx.Play();

            foreach (var vfx in exhaustVFX)
                if (vfx != null && vfx.isPlaying) vfx.Stop();
        }
        else
        {
            isUsingNitro = false;
            foreach (var vfx in nitroVFX)
                if (vfx != null && vfx.isPlaying) vfx.Stop();
        }

        if (!isUsingNitro && Input.GetAxis("Vertical") > 0.8f)
        {
            foreach (var vfx in exhaustVFX)
                if (vfx != null && !vfx.isPlaying) vfx.Play();
        }
        else if (!isUsingNitro)
        {
            foreach (var vfx in exhaustVFX)
                if (vfx != null && vfx.isPlaying) vfx.Stop();
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

    void UpdateNitroUI()
    {
        if (nitroBar != null)
            nitroBar.value = nitroEnergy;
    }

    public void AddNitroEnergy(float amount)
    {
        nitroEnergy = Mathf.Min(nitroEnergy + amount, maxNitroEnergy);
    }

    public void SetInput(float steer, float throttle, bool brake)
    {
        aiSteering = steer * maxSteerAngle;
        aiMotor = throttle * maxMotorTorque;
        aiBrake = brake;
    }
}
