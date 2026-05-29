using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == CarSelectionState.SelectedMapSceneName)
        {
            SpawnPlayerCar();
        }
    }

    public void SpawnPlayerCar()
    {
        if (CarSelectionState.SelectedCarPrefab == null)
        {
            Debug.LogError("Không tìm thấy Car Prefab trong CarSelectionState!");
            return;
        }

        Transform spawnPoint = GetSpawnPoint();

        GameObject playerCar = Instantiate(
            CarSelectionState.SelectedCarPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        playerCar.name = CarSelectionState.SelectedCarDisplayName;
        playerCar.tag = "Player";

        AddToRaceManager(playerCar);


        Debug.Log($"Đã spawn Player Car: {playerCar.name} tại {spawnPoint.name}");
    }

    private void AddToRaceManager(GameObject car)
    {
        if (RaceManager.Instance == null)
        {
            Debug.LogError("RaceManager.Instance chưa tồn tại! Kiểm tra RaceManager có trong scene không.");
            return;
        }

        if (!RaceManager.Instance.racers.Contains(car))
        {
            RaceManager.Instance.racers.Add(car);
        }

        if (car.GetComponent<RacerProgress>() == null)
        {
            car.AddComponent<RacerProgress>();
        }
        TrafficLightController tlc = Object.FindFirstObjectByType<TrafficLightController>();
        if (tlc != null)
        {
            CarController cc = car.GetComponent<CarController>();
            if (cc != null && !tlc.cars.Contains(cc))
            {
                tlc.cars.Add(cc);
            }
        }
    }

    private Transform GetSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Không có spawn point nào! Spawn tại vị trí (0, 0, 0)");
            return transform; 
        }
        return spawnPoints[0];
    }
}