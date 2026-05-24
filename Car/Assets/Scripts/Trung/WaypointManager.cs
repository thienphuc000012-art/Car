using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public Transform[] waypoints;
    private void Awake()
    {
        // Tự lấy toàn bộ child waypoint
        waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Vẽ Waypoint
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform currentPoint = transform.GetChild(i);
            Gizmos.DrawSphere(currentPoint.position, 1f);
            // waypoint tiếp theo
            Transform nextPoint;
            // nối line
            if (i == transform.childCount - 1)
            {
                // nối waypoint cuối với waypoint đầu
                nextPoint = transform.GetChild(0);
                
            }
            else
            {
                nextPoint = transform.GetChild(i + 1);
                
            }
            Gizmos.DrawLine(currentPoint.position, nextPoint.position);
        }
    }
}
