using UnityEngine;

[RequireComponent(typeof(CarController))]
public class AIDriver : MonoBehaviour
{
    [Header("Waypoint")]
    public WaypointManager waypointManager;
    public int currentWaypoint = 0;

    [Header("Speed")]
    public float maxSpeed = 120f;

    [Header("Waypoint")]
    public float waypointReachDistance = 12f;

    [Header("Lane")]
    public float laneWidth = 5f;

    [Header("Avoid Cars")]
    public float avoidDistance = 10f;

    [Header("Recovery")]
    public float stuckTime = 3f;

    [Header("Road")]
    public LayerMask roadMask;
    public float roadCheckDistance = 5f;

    [Header("Reset")]
    public float maxOffRoadTime = 3f;

    private CarController carController;
    private Rigidbody rb;

    private float laneOffset;
    private float stuckTimer;
    private float offRoadTimer;

    void Start()
    {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

        carController.isAI = true;

        laneOffset =
            Random.Range(-laneWidth, laneWidth);
    }

    void Update()
    {
        if (waypointManager == null)
            return;

        if (waypointManager.waypoints.Length == 0)
            return;

        if (NeedRecovery())
        {
            RecoverToTrack();
            return;
        }

        DriveAI();
    }

    void DriveAI()
    {
        Transform wp =
            waypointManager.waypoints[currentWaypoint];

        Vector3 targetPos =
            wp.position +
            wp.right * laneOffset;

        Vector3 localTarget =
            transform.InverseTransformPoint(targetPos);

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
        // Corner Slowdown
        //----------------------------------

        float corner =
            Mathf.Abs(steer);

        if (corner > 0.5f)
            throttle = 0.6f;

        if (corner > 0.8f)
            throttle = 0.3f;

        //----------------------------------
        // Wall Detection
        //----------------------------------

        RaycastHit wallHit;

        Vector3 rayOrigin =
            transform.position +
            transform.up * 0.5f;

        if (Physics.Raycast(
            rayOrigin,
            transform.forward,
            out wallHit,
            6f))
        {
            if (wallHit.collider.attachedRigidbody == null)
            {
                laneOffset +=
                    Random.Range(-3f, 3f);

                laneOffset =
                    Mathf.Clamp(
                        laneOffset,
                        -laneWidth,
                        laneWidth);

                throttle = 0.2f;
            }
        }

        //----------------------------------
        // Car Avoidance
        //----------------------------------

        RaycastHit hit;

        if (Physics.Raycast(
            rayOrigin,
            transform.forward,
            out hit,
            avoidDistance))
        {
            Rigidbody otherRb =
                hit.collider.attachedRigidbody;

            if (otherRb != null &&
                otherRb != rb)
            {
                throttle = 0.3f;

                if (hit.distance < 5f)
                    brake = true;

                if (hit.distance < 8f)
                {
                    laneOffset +=
                        Random.Range(-2f, 2f);

                    laneOffset =
                        Mathf.Clamp(
                            laneOffset,
                            -laneWidth,
                            laneWidth);
                }
            }
        }

        //----------------------------------
        // Speed Limit
        //----------------------------------

        if (speed > maxSpeed)
            throttle = 0;

        //----------------------------------
        // Control
        //----------------------------------

        carController.SetInput(
            steer,
            throttle,
            brake);

        //----------------------------------
        // Next Waypoint
        //----------------------------------

        float distance =
            Vector3.Distance(
                transform.position,
                wp.position);

        if (distance < waypointReachDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypointManager.waypoints.Length)
                currentWaypoint = 0;

            laneOffset =
                Random.Range(
                    -laneWidth,
                    laneWidth);
        }

        //----------------------------------
        // Stuck Detect
        //----------------------------------

        if (speed < 3f)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0;
        }
    }

    bool NeedRecovery()
    {
        if (!IsOnRoad())
        {
            offRoadTimer += Time.deltaTime;

            if (offRoadTimer > maxOffRoadTime)
                return true;
        }
        else
        {
            offRoadTimer = 0;
        }

        if (stuckTimer > stuckTime)
            return true;

        return false;
    }

    bool IsOnRoad()
    {
        RaycastHit hit;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            Vector3.down,
            out hit,
            roadCheckDistance,
            roadMask))
        {
            return true;
        }

        return false;
    }

    void RecoverToTrack()
    {
        Transform wp =
            waypointManager.waypoints[currentWaypoint];

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 spawnPos =
            wp.position
            - wp.forward * 5f
            + Vector3.up * 6f;

        transform.position = spawnPos;

        transform.rotation =
            Quaternion.LookRotation(
                wp.forward,
                Vector3.up);

        laneOffset =
            Random.Range(
                -laneWidth,
                laneWidth);

        stuckTimer = 0;
        offRoadTimer = 0;
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