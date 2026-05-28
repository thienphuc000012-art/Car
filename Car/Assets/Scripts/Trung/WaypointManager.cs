using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public Transform[] waypoints;

    void Awake()
    {
        waypoints = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform current = transform.GetChild(i);

            Gizmos.DrawSphere(current.position, 1f);

            Transform next;

            if (i == transform.childCount - 1)
            {
                next = transform.GetChild(0);
            }
            else
            {
                next = transform.GetChild(i + 1);
            }

            Gizmos.DrawLine(current.position, next.position);
        }
    }
}
