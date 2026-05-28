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

        // Chọn spawn point
        Transform spawnPoint = GetSpawnPoint();

        // Spawn xe
        GameObject playerCar = Instantiate(
            CarSelectionState.SelectedCarPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Thiết lập thông tin cơ bản
        playerCar.name = CarSelectionState.SelectedCarDisplayName;
        playerCar.tag = "Player";

        // Thêm vào danh sách racers của RaceManager
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

        // Thêm vào list racers nếu chưa có
        if (!RaceManager.Instance.racers.Contains(car))
        {
            RaceManager.Instance.racers.Add(car);
        }

        // Đảm bảo có component RacerProgress
        if (car.GetComponent<RacerProgress>() == null)
        {
            car.AddComponent<RacerProgress>();
        }
    }

    private Transform GetSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Không có spawn point nào! Spawn tại vị trí (0, 0, 0)");
            return transform; // hoặc có thể return new GameObject().transform;
        }

        // Lấy spawn point đầu tiên
        return spawnPoints[0];
    }
}