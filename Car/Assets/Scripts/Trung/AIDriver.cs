using UnityEngine;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
public class AIDriver : MonoBehaviour
{
    [Header("Waypoint")]
    public WaypointManager waypointManager;

    private int currentWaypoint = 0;

    [Header("Speed")]
    public float maxSpeed = 75f;

    [Header("Steering")]
    public float steerSensitivity = 45f;

    [Range(0.7f, 1.3f)]
    public float cornerSkill = 1f;

    [Header("Corner")]
    public float slowAngle = 15f;
    public float mediumAngle = 35f;
    public float hardAngle = 55f;

    [Header("Look Ahead")]
    public int lookAheadPoints = 3;

    public float waypointReachDistance = 12f;

    [Header("AI Avoid")]
    public LayerMask aiMask;

    public float aiAvoidDistance = 15f;

    public float aiAvoidStrength = 1.2f;

    [Header("Obstacle")]
    public LayerMask obstacleMask;

    public float obstacleRayDistance = 6f;

    public float obstacleAvoidStrength = 1.2f;

    [Header("Road")]
    public LayerMask roadMask;

    public float roadCheckDistance = 6f;

    [Header("Recovery")]
    public float stuckTime = 2f;

    public float reverseTime = 1.5f;

    [Header("Stability")]
    public float extraDownforce = 70f;

    private CarController car;

    private Rigidbody rb;

    private float stuckTimer;

    private bool isRecovering;

    private float reverseTimer;

    private float brakeMultiplier;

    private bool driftMode;

    void Start()
    {
        car = GetComponent<CarController>();

        rb = GetComponent<Rigidbody>();

        car.isAI = true;

        // mỗi xe skill hơi khác nhau
        cornerSkill += Random.Range(-0.08f, 0.08f);

        // tốc độ khác nhau
        maxSpeed += Random.Range(-5f, 5f);
    }

    void FixedUpdate()
    {
        if (waypointManager == null)
            return;

        if (waypointManager.waypoints.Length == 0)
            return;

        DriveAI();

        ApplyDownforce();
    }

    void DriveAI()
    {
        Transform currentWP =
            waypointManager.waypoints[currentWaypoint];

        Transform futureWP =
            waypointManager.waypoints[
                (currentWaypoint + lookAheadPoints)
                % waypointManager.waypoints.Length
            ];

        // =========================
        // ĐỌC GÓC CUA
        // =========================

        float totalCurve = 0f;

        Vector3 prevDir = transform.forward;

        int memoryPoints = 5;

        for (int i = 1; i <= memoryPoints; i++)
        {
            int index =
                (currentWaypoint + i)
                % waypointManager.waypoints.Length;

            Vector3 nextDir =
            (
                waypointManager.waypoints[index].position
                - transform.position
            ).normalized;

            float angle =
                Mathf.Abs(
                    Vector3.SignedAngle(
                        prevDir,
                        nextDir,
                        Vector3.up
                    )
                );

            totalCurve += angle;

            prevDir = nextDir;
        }

        float averageCurve =
            totalCurve / memoryPoints;

        // =========================
        // TARGET POSITION
        // ƯU TIÊN WAYPOINT
        // =========================

        Vector3 targetPos =
            Vector3.Lerp(
                currentWP.position,
                futureWP.position,
                0.35f
            );

        // =========================
        // KIỂM TRA ROAD
        // =========================

        bool onRoad =
            Physics.Raycast(
                targetPos + Vector3.up * 5f,
                Vector3.down,
                10f,
                roadMask
            );

        // nếu target ngoài road
        // ép về waypoint hiện tại
        if (!onRoad)
        {
            targetPos =
                currentWP.position;
        }

        // =========================
        // WAYPOINT CHECK
        // =========================

        Vector3 toWaypoint =
            currentWP.position -
            transform.position;

        float distance =
            toWaypoint.magnitude;

        float dot =
            Vector3.Dot(
                transform.forward,
                toWaypoint.normalized
            );

        bool sharpCorner =
            averageCurve > mediumAngle;

        if (
            (
                distance < waypointReachDistance
                && !sharpCorner
            )
            ||
            (
                distance < waypointReachDistance * 0.5f
                && sharpCorner
            )
            ||
            dot < -0.3f
        )
        {
            currentWaypoint++;

            if (currentWaypoint >= waypointManager.waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }

        // =========================
        // STEER
        // =========================

        Vector3 localTarget =
            transform.InverseTransformPoint(targetPos);

        float steer =
            Mathf.Clamp(
                localTarget.x /
                Mathf.Max(localTarget.magnitude, 0.1f),
                -1f,
                1f
            );

        // ưu tiên waypoint
        steer *= 1.15f;

        steer *=
            (steerSensitivity / 45f)
            * cornerSkill;

        // =========================
        // TARGET SPEED
        // =========================

        float targetSpeed = maxSpeed;

        if (driftMode)
        {
            steer *= 1.1f;

            targetSpeed *= 0.8f;
        }

        // =========================
        // GIẢM TỐC KHI CUA
        // =========================

        if (averageCurve > hardAngle)
        {
            targetSpeed *= 0.5f;

            steer *= 1.15f;
        }
        else if (averageCurve > mediumAngle)
        {
            targetSpeed *= 0.7f;

            steer *= 1.08f;
        }
        else if (averageCurve > slowAngle)
        {
            targetSpeed *= 0.88f;
        }

        // =========================
        // NÉ XE AI
        // =========================

        HandleAIAvoidance(
            ref steer,
            ref targetSpeed
        );

        // =========================
        // NÉ TƯỜNG
        // =========================

        HandleObstacleAvoidance(ref steer);

        // =========================
        // QUAY VỀ ROAD
        // =========================

        bool offRoad =
            HandleRoadRecovery(
                ref steer,
                ref targetSpeed
            );

        // =========================
        // STUCK
        // =========================

        HandleStuck(ref steer);

        float currentSpeed =
            rb.linearVelocity.magnitude * 3.6f;

        bool brake = false;

        float throttle = 1f;

        // =========================
        // BRAKE ZONE
        // =========================

        if (brakeMultiplier > 0f)
        {
            brake = true;

            throttle =
                Mathf.Lerp(
                    1f,
                    0f,
                    brakeMultiplier
                );

            targetSpeed *=
                Mathf.Lerp(
                    1f,
                    0.6f,
                    brakeMultiplier
                );
        }

        // =========================
        // RECOVERY
        // =========================

        if (isRecovering)
        {
            reverseTimer += Time.fixedDeltaTime;

            throttle = -1f;

            steer += Random.Range(-0.3f, 0.3f);

            if (reverseTimer >= reverseTime)
            {
                reverseTimer = 0f;

                isRecovering = false;
            }
        }
        else
        {
            if (currentSpeed > targetSpeed)
            {
                throttle = 0.2f;
            }
        }

        if (offRoad)
        {
            throttle = 1f;
        }

        car.SetInput(
            steer,
            throttle,
            brake
        );
    }

    // =================================
    // AI AVOID
    // =================================

    void HandleAIAvoidance(
        ref float steer,
        ref float targetSpeed
    )
    {
        Vector3 rayStart =
            transform.position +
            Vector3.up * 0.5f;

        RaycastHit hit;

        bool carAhead =
            Physics.SphereCast(
                rayStart,
                1.3f,
                transform.forward,
                out hit,
                aiAvoidDistance,
                aiMask
            );

        if (!carAhead)
            return;

        Rigidbody otherRb =
            hit.collider.GetComponent<Rigidbody>();

        if (otherRb == null)
            return;

        float otherSpeed =
            otherRb.linearVelocity.magnitude * 3.6f;

        float distance =
            hit.distance;

        if (distance < 12f)
        {
            // giảm tốc nhẹ
            targetSpeed =
                Mathf.Min(
                    targetSpeed,
                    otherSpeed * 0.96f
                );

            bool leftFree =
                !Physics.Raycast(
                    rayStart - transform.right * 2f,
                    transform.forward,
                    8f,
                    aiMask | obstacleMask
                );

            bool rightFree =
                !Physics.Raycast(
                    rayStart + transform.right * 2f,
                    transform.forward,
                    8f,
                    aiMask | obstacleMask
                );

            // né nhẹ
            if (leftFree)
            {
                steer -= aiAvoidStrength * 0.55f;
            }
            else if (rightFree)
            {
                steer += aiAvoidStrength * 0.55f;
            }
        }
    }

    // =================================
    // WALL AVOID
    // =================================

    void HandleObstacleAvoidance(ref float steer)
    {
        Vector3 leftRay =
            transform.position -
            transform.right * 2f;

        Vector3 rightRay =
            transform.position +
            transform.right * 2f;

        bool hitLeft =
            Physics.Raycast(
                leftRay,
                transform.forward,
                obstacleRayDistance,
                obstacleMask
            );

        bool hitRight =
            Physics.Raycast(
                rightRay,
                transform.forward,
                obstacleRayDistance,
                obstacleMask
            );

        if (hitLeft)
        {
            steer += obstacleAvoidStrength;
        }

        if (hitRight)
        {
            steer -= obstacleAvoidStrength;
        }
    }

    // =================================
    // ROAD RECOVERY
    // =================================

    bool HandleRoadRecovery(
        ref float steer,
        ref float targetSpeed
    )
    {
        bool onRoad =
            Physics.Raycast(
                transform.position + Vector3.up * 2f,
                Vector3.down,
                roadCheckDistance,
                roadMask
            );

        if (!onRoad)
        {
            Vector3 target =
                waypointManager
                .waypoints[currentWaypoint]
                .position;

            Vector3 localPoint =
                transform.InverseTransformPoint(target);

            steer =
                Mathf.Clamp(
                    localPoint.x /
                    localPoint.magnitude,
                    -1f,
                    1f
                );

            targetSpeed *= 0.45f;

            return true;
        }

        return false;
    }

    // =================================
    // STUCK
    // =================================

    void HandleStuck(ref float steer)
    {
        float speed =
            rb.linearVelocity.magnitude;

        if (speed < 2f)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTime)
            {
                isRecovering = true;

                reverseTimer = 0f;

                steer =
                    Random.Range(-1f, 1f);
            }

            if (stuckTimer >= 4f)
            {
                Transform wp =
                    waypointManager
                    .waypoints[currentWaypoint];

                transform.position =
                    wp.position +
                    Vector3.up * 2f;

                Vector3 dir =
                (
                    waypointManager.waypoints[
                        (currentWaypoint + 1)
                        % waypointManager.waypoints.Length
                    ].position -
                    wp.position
                ).normalized;

                transform.rotation =
                    Quaternion.LookRotation(dir);

                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                stuckTimer = 0f;

                isRecovering = false;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    // =================================
    // BRAKE ZONE
    // =================================

    void OnTriggerEnter(Collider other)
    {
        BrakeZone zone =
            other.GetComponent<BrakeZone>();

        if (zone != null)
        {
            brakeMultiplier = zone.brakeStrength;

            driftMode = zone.allowDrift;
        }
    }

    void OnTriggerExit(Collider other)
    {
        BrakeZone zone =
            other.GetComponent<BrakeZone>();

        if (zone != null)
        {
            brakeMultiplier = 0f;

            driftMode = false;
        }
    }

    // =================================
    // DOWNFORCE
    // =================================

    void ApplyDownforce()
    {
        float speed =
            rb.linearVelocity.magnitude;

        rb.AddForce(
            -transform.up *
            speed *
            extraDownforce,
            ForceMode.Force
        );
    }
}