using UnityEngine;
using Unity.Cinemachine;

public class AutoAssignCameraTarget : MonoBehaviour
{
    public CinemachineCamera vcam;

    void Start()
    {
        if (vcam != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                vcam.Follow = player.transform;
                vcam.LookAt = player.transform;
            }
            else
            {
                Debug.LogWarning("Không tìm thấy GameObject với tag Player!");
            }
        }
    }
}
