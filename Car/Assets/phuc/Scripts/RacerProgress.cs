using UnityEngine;

public class RacerProgress : MonoBehaviour
{
    public int lapCount = 0;
    public float distanceTravelled = 0f;
    public float finishTime = -1f; 

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
