using UnityEngine;

public class SelectedCarSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject fallbackCarPrefab;
    public bool spawnOnStart = true;

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

        GameObject prefab = CarSelectionState.SelectedCarPrefab != null ? CarSelectionState.SelectedCarPrefab : fallbackCarPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("SelectedCarSpawner: Chua co xe duoc chon va chua gan Fallback Car Prefab.");
            return null;
        }

        Transform point = spawnPoint != null ? spawnPoint : transform;
        SpawnedCar = Instantiate(prefab, point.position, point.rotation);
        SpawnedCar.name = prefab.name;
        return SpawnedCar;
    }
}
