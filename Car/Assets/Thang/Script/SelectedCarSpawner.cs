using UnityEngine;
using Unity.Cinemachine;

public class SelectedCarSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;
    public GameObject fallbackCarPrefab;
    public bool spawnOnStart = true;
    public bool useExistingPlayerWhenNoMenuSelection = true;
    public bool spawnFallbackWhenNoExistingPlayer = false;
    public bool replaceExistingPlayerOnMenuSelection = true;

    [Header("Camera")]
    public bool autoAssignCamera = true;
    public CinemachineCamera cinemachineCamera;
    public CarCameraController carCameraController;
    public string cameraTargetName = "CameraTarget";
    public Vector3 cameraTargetLocalOffset = new Vector3(0f, 1.2f, 0f);

    public GameObject SpawnedCar { get; private set; }

    void Start()
    {
        if (spawnOnStart)
            SpawnSelectedCar();
    }

    public GameObject SpawnSelectedCar()
    {
        if (SpawnedCar != null)
            return SpawnedCar;

        bool hasMenuSelection = CarSelectionState.HasMenuSelection && CarSelectionState.SelectedCarPrefab != null;
        CarController existingPlayerCar = FindExistingPlayerCar();

        if (!hasMenuSelection && useExistingPlayerWhenNoMenuSelection && existingPlayerCar != null)
        {
            SpawnedCar = existingPlayerCar.gameObject;

            if (autoAssignCamera)
                AssignCameraToSpawnedCar();

            return SpawnedCar;
        }

        GameObject prefab = hasMenuSelection ? CarSelectionState.SelectedCarPrefab : fallbackCarPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("SelectedCarSpawner: Chua co xe duoc chon va chua gan Fallback Car Prefab.");
            return null;
        }

        if (!hasMenuSelection && !spawnFallbackWhenNoExistingPlayer)
        {
            Debug.LogWarning("SelectedCarSpawner: Mo thang scene map nhung khong co xe player san va dang tat spawn fallback.");
            return null;
        }

        if (hasMenuSelection && replaceExistingPlayerOnMenuSelection)
            DestroyExistingPlayerCars();

        Transform point = spawnPoint != null ? spawnPoint : transform;
        SpawnedCar = Instantiate(prefab, point.position, point.rotation);
        SpawnedCar.name = prefab.name;

        CarController spawnedController = SpawnedCar.GetComponentInChildren<CarController>();
        if (spawnedController != null)
            spawnedController.isAI = false;

        if (autoAssignCamera)
            AssignCameraToSpawnedCar();

        return SpawnedCar;
    }

    void AssignCameraToSpawnedCar()
    {
        if (SpawnedCar == null)
            return;

        Transform cameraTarget = GetOrCreateCameraTarget(SpawnedCar.transform);
        Rigidbody carRb = SpawnedCar.GetComponentInChildren<Rigidbody>();

        CinemachineCamera vcam = cinemachineCamera != null ? cinemachineCamera : FindObjectOfType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = cameraTarget;
            vcam.LookAt = cameraTarget;
            cinemachineCamera = vcam;
        }
        else
        {
            Debug.LogWarning("SelectedCarSpawner: Khong tim thay CinemachineCamera trong scene.");
        }

        CarCameraController cameraController = carCameraController != null ? carCameraController : FindObjectOfType<CarCameraController>();
        if (cameraController != null)
        {
            cameraController.carRb = carRb;
            if (cameraController.vcam == null && vcam != null)
                cameraController.vcam = vcam;

            carCameraController = cameraController;
        }

        if (carRb == null)
            Debug.LogWarning("SelectedCarSpawner: Xe vua spawn khong co Rigidbody, CarCameraController se khong tinh duoc toc do.");
    }

    CarController FindExistingPlayerCar()
    {
        foreach (CarController car in FindObjectsByType<CarController>(FindObjectsSortMode.None))
        {
            if (car != null && car.isActiveAndEnabled && !car.isAI)
                return car;
        }

        return null;
    }

    void DestroyExistingPlayerCars()
    {
        foreach (CarController car in FindObjectsByType<CarController>(FindObjectsSortMode.None))
        {
            if (car == null || car.isAI)
                continue;

            Destroy(car.gameObject);
        }
    }

    Transform GetOrCreateCameraTarget(Transform carRoot)
    {
        Transform target = carRoot.Find(cameraTargetName);
        if (target != null)
            return target;

        GameObject targetObject = new GameObject(cameraTargetName);
        target = targetObject.transform;
        target.SetParent(carRoot, false);
        target.localPosition = cameraTargetLocalOffset;
        target.localRotation = Quaternion.identity;
        target.localScale = Vector3.one;
        return target;
    }
}
