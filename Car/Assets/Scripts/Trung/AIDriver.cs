using UnityEngine;

[RequireComponent(typeof(CarController))]
public class AIDriver : MonoBehaviour
{
    [Header("Waypoint")]
    public WaypointManager waypointManager;

    public int currentWaypoint = 0;

    [Header("Speed")]
    public float maxSpeed = 120f;

    [Header("Steering")]
    public float steerSensitivity = 3f;

    [Header("Waypoint")]
    public float waypointReachDistance = 12f;

    [Header("Lane")]
    public float laneWidth = 4f;

    [Header("Avoid")]
    public float avoidDistance = 12f;

    [Header("Recovery")]
    public float stuckTime = 2f;

    private CarController carController;
    private Rigidbody rb;

    private float laneOffset;
    private float stuckTimer;

    void Start()
    {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

        carController.isAI = true;

        laneOffset = Random.Range(-laneWidth, laneWidth);
    }

    void Update()
    {
        if (waypointManager == null)
            return;

        if (waypointManager.waypoints.Length == 0)
            return;

        DriveAI();
    }

    void DriveAI()
    {
        Transform wp =
            waypointManager.waypoints[currentWaypoint];

        Vector3 targetPosition =
            wp.position +
            wp.right * laneOffset;

        Vector3 localTarget =
            transform.InverseTransformPoint(targetPosition);

        float steer =
            Mathf.Clamp(
                localTarget.x / localTarget.magnitude,
                -1f,
                1f);

        float speed =
            rb.linearVelocity.magnitude * 3.6f;

        float throttle = 1f;
        bool brake = false;

        //----------------------------------
        // Giảm tốc khi cua gắt
        //----------------------------------

        float cornerAmount =
            Mathf.Abs(steer);

        if (cornerAmount > 0.5f)
        {
            throttle = 0.6f;
        }

        if (cornerAmount > 0.8f)
        {
            throttle = 0.3f;
        }

        //----------------------------------
        // Né xe phía trước
        //----------------------------------

        RaycastHit hit;

        Vector3 rayOrigin =
            transform.position +
            transform.up * 0.5f;

        if (Physics.Raycast(
                rayOrigin,
                transform.forward,
                out hit,
                avoidDistance))
        {
            if (hit.collider.attachedRigidbody != null &&
                hit.collider.attachedRigidbody != rb)
            {
                throttle = 0.2f;

                if (hit.distance < 5f)
                {
                    brake = true;
                }

                if (hit.distance < 8f)
                {
                    laneOffset += Random.Range(-1.5f, 1.5f);

                    laneOffset =
                        Mathf.Clamp(
                            laneOffset,
                            -laneWidth,
                            laneWidth);
                }
            }
        }

        //----------------------------------
        // Giới hạn tốc độ
        //----------------------------------

        if (speed > maxSpeed)
        {
            throttle = 0;
        }

        //----------------------------------
        // Điều khiển xe
        //----------------------------------

        carController.SetInput(
            steer,
            throttle,
            brake);

        //----------------------------------
        // Đến waypoint tiếp theo
        //----------------------------------

        float distance =
            Vector3.Distance(
                transform.position,
                wp.position);

        if (distance < waypointReachDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypointManager.waypoints.Length)
            {
                currentWaypoint = 0;
            }

            laneOffset =
                Random.Range(
                    -laneWidth,
                    laneWidth);
        }

        //----------------------------------
        // Chống kẹt
        //----------------------------------

        if (speed < 3f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > stuckTime)
            {
                laneOffset =
                    Random.Range(
                        -laneWidth,
                        laneWidth);

                stuckTimer = 0;
            }
        }
        else
        {
            stuckTimer = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (waypointManager == null)
            return;

        if (waypointManager.waypoints.Length == 0)
            return;

        Transform wp =
            waypointManager.waypoints[currentWaypoint];

        Gizmos.color = Color.green;

        Gizmos.DrawLine(
            transform.position,
            wp.position);
    }
}