//using UnityEngine;
//using Unity.Cinemachine;

//public class CarCameraController : MonoBehaviour
//{
//    public Rigidbody carRb;
//    public CinemachineCamera vcam;
//    private CinemachineFollow follow;

//    public float baseFOV = 60f;
//    public float maxFOV = 80f;
//    public float speedForMaxFOV = 200f;

//    public float baseDistance = 10f;
//    public float minDistance = 6f;

//    void Start()
//    {
//        follow = vcam.GetComponent<CinemachineFollow>();
//    }

//    void Update()
//    {
//        float speed = carRb.linearVelocity.magnitude * 3.6f;

//        // FOV zoom theo tốc độ
//        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speed / speedForMaxFOV);
//        vcam.Lens.FieldOfView = targetFOV;

//        // Camera tiến gần theo tốc độ
//        if (follow != null)
//        {
//            Vector3 offset = follow.FollowOffset;
//            offset.z = Mathf.Lerp(-baseDistance, -minDistance, speed / speedForMaxFOV);
//            follow.FollowOffset = offset;
//        }
//    }
//}
