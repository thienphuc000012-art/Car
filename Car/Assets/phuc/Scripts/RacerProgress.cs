using UnityEngine;

public class RacerProgress : MonoBehaviour
{
    public int lapCount = 0;
    public float distanceTravelled = 0f;
    public float finishTime = -1f;

    // Thêm biến này để quản lý checkpoint
    public int nextCheckpointIndex = 0;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb != null && finishTime < 0f)
        {
            distanceTravelled += rb.linearVelocity.magnitude * Time.deltaTime;
        }
    }

    public void FinishRace(float raceTime)
    {
        if (finishTime < 0f)
        {
            finishTime = raceTime;
        }
    }
}
