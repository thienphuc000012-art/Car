using UnityEngine;

public class AIDriver : MonoBehaviour
{
    [Header("Waypoint")]
    public WaypointManager waypointManager;

    private int currentWaypoint = 0;

    [Header("Waypoint Settings")]
    public float waypointReachDistance = 3f;

    [Header("AI Speed")]
    public float maxSpeed = 70f;

    [Header("Steering")]
    public float steerSensitivity = 45f;

    [Header("Brake Settings")]
    public float slowAngle = 15f;
    public float mediumAngle = 30f;
    public float hardAngle = 50f;

    [Header("Recover")]
    public float stuckSpeed = 2f;
    public float stuckTime = 3f;

    private float stuckTimer = 0f;

    private Rigidbody rb;
    private CarController carController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        carController = GetComponent<CarController>();

        // bật AI mode
        carController.isAI = true;

        // tìm waypoint gần nhất lúc spawn
        FindClosestWaypoint();
    }

    private void FixedUpdate()
    {
        if (waypointManager == null) return;

        DriveToWaypoint();

        CheckIfStuck();

        LimitSpeed();
    }

    void DriveToWaypoint()
    {
        Transform targetWaypoint =
            waypointManager.waypoints[currentWaypoint];

        // ===== HƯỚNG TỚI WAYPOINT =====

        Vector3 localTarget =
            transform.InverseTransformPoint(
                targetWaypoint.position
            );

        // ===== TÍNH GÓC LÁI =====

        float angle =
            Mathf.Atan2(
                localTarget.x,
                localTarget.z
            ) * Mathf.Rad2Deg;

        // ===== STEER =====

        float steer =
            Mathf.Clamp(
                angle / steerSensitivity,
                -0.6f,
                0.6f
            );

        // ===== SPEED =====

        float throttle = 1f;

        bool brake = false;

        float absAngle = Mathf.Abs(angle);

        float currentSpeed =
            rb.linearVelocity.magnitude * 3.6f;

        // cua nhẹ
        if(absAngle > slowAngle)
        {
            throttle = 0.7f;
        }

        // cua vừa
        if(absAngle > mediumAngle)
        {
            throttle = 0.4f;

            if(currentSpeed > 45f)
            {
                brake = true;
            }
        }

        // cua gắt
        if(absAngle > hardAngle)
        {
            throttle = 0.15f;
            brake = true;
        }

        // ===== GỬI INPUT =====

        carController.SetInput(
            steer,
            throttle,
            brake
        );

        // ===== CHUYỂN WAYPOINT =====

        float distance =
            Vector3.Distance(
                transform.position,
                targetWaypoint.position
            );

        if(distance <= waypointReachDistance)
        {
            currentWaypoint++;

            // loop
            if(currentWaypoint >= waypointManager.waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }

        // ===== DEBUG =====

        Debug.DrawLine(
            transform.position,
            targetWaypoint.position,
            Color.green
        );
    }

    void LimitSpeed()
    {
        float speed =
            rb.linearVelocity.magnitude * 3.6f;

        if(speed > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized *
                (maxSpeed / 3.6f);
        }
    }

    void CheckIfStuck()
    {
        // xe gần như đứng yên
        if(rb.linearVelocity.magnitude < stuckSpeed)
        {
            stuckTimer += Time.fixedDeltaTime;

            // bị kẹt
            if(stuckTimer >= stuckTime)
            {
                RecoverCar();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void RecoverCar()
    {
        Debug.Log("Recover AI Car");

        // tìm waypoint gần nhất
        FindClosestWaypoint();

        Transform targetWaypoint =
            waypointManager.waypoints[currentWaypoint];

        // reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // đặt xe hơi cao lên
        transform.position =
            targetWaypoint.position + Vector3.up * 2f;

        // quay đúng hướng waypoint tiếp theo
        int nextWaypoint =
            (currentWaypoint + 1) %
            waypointManager.waypoints.Length;

        Vector3 direction =
            (
                waypointManager.waypoints[nextWaypoint].position
                - targetWaypoint.position
            ).normalized;

        transform.rotation =
            Quaternion.LookRotation(direction);

        stuckTimer = 0f;
    }

    void FindClosestWaypoint()
    {
        float closestDistance = Mathf.Infinity;

        for(int i = 0; i < waypointManager.waypoints.Length; i++)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    waypointManager.waypoints[i].position
                );

            if(distance < closestDistance)
            {
                closestDistance = distance;

                currentWaypoint = i;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(waypointManager == null) return;

        if(currentWaypoint >= waypointManager.waypoints.Length) return;

        Gizmos.color = Color.green;

        Gizmos.DrawSphere(
            waypointManager.waypoints[currentWaypoint].position,
            2f
        );
    }
}