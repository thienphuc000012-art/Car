using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class LobbyMapOption
{
    public string id = "coastal_drive";
    public string displayName = "TRACK RACE";
    public string distanceText = "-- KM";

#if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;
#endif

    public string sceneName = "complete_track_demo";
    public Sprite previewImage;
    public Sprite miniMapImage;
    public bool locked;

    [Header("Race Info")]
    public string laps = "3";
    public string timeOfDay = "NOON";
    public string weather = "CLEAR";
    public string traffic = "MEDIUM";

    [TextArea(2, 5)]
    public string trackInfo = "Road course beside the coast. Fill this text with your real track info later.";

#if UNITY_EDITOR
    public void SyncSceneNameFromAsset()
    {
        if (sceneAsset != null)
            sceneName = sceneAsset.name;
    }
#endif
}

[System.Serializable]
public class LobbyCarOption
{
    public string id = "car_id";
    public string displayName = "CAR NAME";
    public string classLabel = "CLASS A";
    public GameObject carPrefab;
    public Sprite previewImage;
    public bool locked;

    [Range(0, 100)] public int topSpeed = 80;
    [Range(0, 100)] public int acceleration = 80;
    [Range(0, 100)] public int handling = 80;
    [Range(0, 100)] public int braking = 80;
    [Range(0, 100)] public int nitro = 80;

    [TextArea(2, 4)]
    public string description = "Balanced street race build.";
}

public class LobbyRoomController : MonoBehaviour
{
    public MainMenuFlow menuFlow;

    [Header("Maps")]
    public LobbyMapOption[] maps;
    public int selectedMapIndex;

    [Header("Cars")]
    public LobbyCarOption[] cars;
    public int selectedCarIndex;

    [Header("Card Build")]
    public bool autoBuildCards = true;
    public Transform mapCardsParent;
    public Transform carCardsParent;
    public LobbySelectionCard mapCardPrefab;
    public LobbySelectionCard carCardPrefab;
    public LobbySelectionCard[] mapCards;
    public LobbySelectionCard[] carCards;

    [Header("Summary Texts")]
    public TMP_Text statusText;
    public TMP_Text selectedMapText;
    public TMP_Text selectedCarText;
    public TMP_Text currentSelectionText;
    public TMP_Text selectedMapNameText;
    public TMP_Text selectedMapDistanceText;
    public TMP_Text selectedCarNameText;
    public TMP_Text selectedCarClassText;
    public TMP_Text selectedCarDescriptionText;

    [Header("Race Info Texts")]
    public TMP_Text raceInfoText;
    public TMP_Text lapsText;
    public TMP_Text timeOfDayText;
    public TMP_Text weatherText;
    public TMP_Text trafficText;

    [Header("Track Info Texts")]
    public TMP_Text trackInfoText;
    public Image trackPreviewImage;

    [Header("Car Stat Bars")]
    public Image topSpeedFill;
    public Image accelerationFill;
    public Image handlingFill;
    public Image brakingFill;
    public Image nitroFill;

    [Header("Car Stat Values")]
    public TMP_Text topSpeedValueText;
    public TMP_Text accelerationValueText;
    public TMP_Text handlingValueText;
    public TMP_Text brakingValueText;
    public TMP_Text nitroValueText;

    void Awake()
    {
        EnsureDefaultData();
        BindSceneObjects();
    }

    void Start()
    {
        EnsureDefaultData();
        ClampSelection();
        BuildCards();
        ApplySelectionState();
        RefreshAll("San sang chon map va xe.");
    }

    void OnValidate()
    {
        EnsureDefaultData();

#if UNITY_EDITOR
        if (maps != null)
        {
            foreach (LobbyMapOption map in maps)
            {
                if (map != null)
                    map.SyncSceneNameFromAsset();
            }
        }
#endif

        ClampSelection();
    }

    public void BindSceneObjects()
    {
        menuFlow = menuFlow != null ? menuFlow : GetComponent<MainMenuFlow>();

        GameObject lobbyPanel = MainMenuFlow.FindSceneObject("LobbyPanel");

        mapCardsParent = mapCardsParent != null ? mapCardsParent : FindTransform(lobbyPanel, "MapCardsParent", "MapList", "MapContent", "SelectMapContent");
        carCardsParent = carCardsParent != null ? carCardsParent : FindTransform(lobbyPanel, "CarCardsParent", "CarList", "CarContent", "SelectCarContent");

        statusText = statusText != null ? statusText : FindFirstText(lobbyPanel, "StatusText", "SearchStatusText");
        selectedMapText = selectedMapText != null ? selectedMapText : FindFirstText(lobbyPanel, "SelectedMapText", "MapText", "MapNameText");
        selectedCarText = selectedCarText != null ? selectedCarText : FindFirstText(lobbyPanel, "SelectedCarText", "CarText", "CarNameText");
        currentSelectionText = currentSelectionText != null ? currentSelectionText : FindFirstText(lobbyPanel, "CurrentSelectionText", "CurrentRoomText");
        selectedMapNameText = selectedMapNameText != null ? selectedMapNameText : FindFirstText(lobbyPanel, "SelectedMapNameText", "SelectedTrackNameText", "TrackNameText");
        selectedMapDistanceText = selectedMapDistanceText != null ? selectedMapDistanceText : FindFirstText(lobbyPanel, "SelectedMapDistanceText", "SelectedTrackDistanceText", "TrackDistanceText");
        selectedCarNameText = selectedCarNameText != null ? selectedCarNameText : FindFirstText(lobbyPanel, "SelectedCarNameText", "SelectedVehicleNameText", "VehicleNameText");
        selectedCarClassText = selectedCarClassText != null ? selectedCarClassText : FindFirstText(lobbyPanel, "SelectedCarClassText", "SelectedVehicleClassText", "VehicleClassText");
        selectedCarDescriptionText = selectedCarDescriptionText != null ? selectedCarDescriptionText : FindFirstText(lobbyPanel, "SelectedCarDescriptionText", "CarDescriptionText");

        raceInfoText = raceInfoText != null ? raceInfoText : FindFirstText(lobbyPanel, "RaceInfoText");
        lapsText = lapsText != null ? lapsText : FindFirstText(lobbyPanel, "LapsText", "LapText");
        timeOfDayText = timeOfDayText != null ? timeOfDayText : FindFirstText(lobbyPanel, "TimeOfDayText", "TimeText");
        weatherText = weatherText != null ? weatherText : FindFirstText(lobbyPanel, "WeatherText");
        trafficText = trafficText != null ? trafficText : FindFirstText(lobbyPanel, "TrafficText");
        trackInfoText = trackInfoText != null ? trackInfoText : FindFirstText(lobbyPanel, "TrackInfoText", "TrackDescriptionText");
        trackPreviewImage = trackPreviewImage != null ? trackPreviewImage : FindFirstImage(lobbyPanel, "TrackPreviewImage", "MiniMapImage", "TrackImage");

        topSpeedFill = topSpeedFill != null ? topSpeedFill : FindFirstImage(lobbyPanel, "TopSpeedFill", "SpeedFill");
        accelerationFill = accelerationFill != null ? accelerationFill : FindFirstImage(lobbyPanel, "AccelerationFill", "AccelFill");
        handlingFill = handlingFill != null ? handlingFill : FindFirstImage(lobbyPanel, "HandlingFill");
        brakingFill = brakingFill != null ? brakingFill : FindFirstImage(lobbyPanel, "BrakingFill", "BrakeFill");
        nitroFill = nitroFill != null ? nitroFill : FindFirstImage(lobbyPanel, "NitroFill", "BoostFill");

        topSpeedValueText = topSpeedValueText != null ? topSpeedValueText : FindFirstText(lobbyPanel, "TopSpeedValueText", "SpeedValueText");
        accelerationValueText = accelerationValueText != null ? accelerationValueText : FindFirstText(lobbyPanel, "AccelerationValueText", "AccelValueText");
        handlingValueText = handlingValueText != null ? handlingValueText : FindFirstText(lobbyPanel, "HandlingValueText");
        brakingValueText = brakingValueText != null ? brakingValueText : FindFirstText(lobbyPanel, "BrakingValueText", "BrakeValueText");
        nitroValueText = nitroValueText != null ? nitroValueText : FindFirstText(lobbyPanel, "NitroValueText", "BoostValueText");
    }

    public void SelectMap(int index)
    {
        if (!HasMaps())
        {
            RefreshAll("Chua co map de chon.");
            return;
        }

        index = Mathf.Clamp(index, 0, maps.Length - 1);
        if (maps[index].locked)
        {
            RefreshAll("Map nay dang khoa.");
            return;
        }

        selectedMapIndex = index;
        ApplySelectionState();
        RefreshAll("Da chon map " + GetSelectedMap().displayName + ".");
    }

    public void SelectCar(int index)
    {
        if (!HasCars())
        {
            RefreshAll("Chua co xe de chon.");
            return;
        }

        index = Mathf.Clamp(index, 0, cars.Length - 1);
        if (cars[index].locked)
        {
            RefreshAll("Xe nay dang khoa.");
            return;
        }

        selectedCarIndex = index;
        ApplySelectionState();
        RefreshAll("Da chon xe " + GetSelectedCar().displayName + ".");
    }

    public void SelectNextMap()
    {
        SelectNextAvailableMap(1);
    }

    public void SelectPreviousMap()
    {
        SelectNextAvailableMap(-1);
    }

    public void SelectNextCar()
    {
        SelectNextAvailableCar(1);
    }

    public void SelectPreviousCar()
    {
        SelectNextAvailableCar(-1);
    }

    public void RandomizeSelection()
    {
        if (HasMaps())
            selectedMapIndex = GetRandomUnlockedMapIndex();

        if (HasCars())
            selectedCarIndex = GetRandomUnlockedCarIndex();

        ApplySelectionState();
        RefreshAll("Da random map va xe.");
    }

    public void StartSelectedMap()
    {
        ClampSelection();

        LobbyMapOption map = GetSelectedMap();
        if (map == null)
        {
            RefreshAll("Chua co map. Hay them Maps trong Inspector.");
            return;
        }

        if (map.locked)
        {
            RefreshAll("Map nay dang khoa.");
            return;
        }

        if (string.IsNullOrWhiteSpace(map.sceneName))
        {
            RefreshAll("Map chua co Scene Name. Keo scene vao Scene Asset hoac dien Scene Name.");
            return;
        }

        ApplySelectionState();
        CarSelectionState.HasMenuSelection = true;
        RefreshAll("Dang vao map " + map.displayName + "...");
        SceneManager.LoadScene(map.sceneName);
    }

    void SelectNextAvailableMap(int step)
    {
        if (!HasMaps())
        {
            RefreshAll("Chua co map de chon.");
            return;
        }

        selectedMapIndex = FindNextUnlockedIndex(maps, selectedMapIndex, step);
        ApplySelectionState();
        RefreshAll("Da doi map.");
    }

    void SelectNextAvailableCar(int step)
    {
        if (!HasCars())
        {
            RefreshAll("Chua co xe de chon.");
            return;
        }

        selectedCarIndex = FindNextUnlockedIndex(cars, selectedCarIndex, step);
        ApplySelectionState();
        RefreshAll("Da doi xe.");
    }

    int FindNextUnlockedIndex(LobbyMapOption[] values, int current, int step)
    {
        for (int i = 1; i <= values.Length; i++)
        {
            int index = (current + step * i + values.Length) % values.Length;
            if (!values[index].locked)
                return index;
        }

        return Mathf.Clamp(current, 0, values.Length - 1);
    }

    int FindNextUnlockedIndex(LobbyCarOption[] values, int current, int step)
    {
        for (int i = 1; i <= values.Length; i++)
        {
            int index = (current + step * i + values.Length) % values.Length;
            if (!values[index].locked)
                return index;
        }

        return Mathf.Clamp(current, 0, values.Length - 1);
    }

    int GetRandomUnlockedMapIndex()
    {
        int unlockedCount = 0;
        foreach (LobbyMapOption map in maps)
        {
            if (!map.locked)
                unlockedCount++;
        }

        if (unlockedCount == 0)
            return Mathf.Clamp(selectedMapIndex, 0, maps.Length - 1);

        int target = Random.Range(0, unlockedCount);
        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i].locked)
                continue;

            if (target == 0)
                return i;

            target--;
        }

        return 0;
    }

    int GetRandomUnlockedCarIndex()
    {
        int unlockedCount = 0;
        foreach (LobbyCarOption car in cars)
        {
            if (!car.locked)
                unlockedCount++;
        }

        if (unlockedCount == 0)
            return Mathf.Clamp(selectedCarIndex, 0, cars.Length - 1);

        int target = Random.Range(0, unlockedCount);
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].locked)
                continue;

            if (target == 0)
                return i;

            target--;
        }

        return 0;
    }

    void BuildCards()
    {
        BuildMapCards();
        BuildCarCards();
    }

    void BuildMapCards()
    {
        if (autoBuildCards && mapCardPrefab != null && mapCardsParent != null && HasMaps())
        {
            ClearGeneratedCards(mapCardsParent, mapCardPrefab);
            mapCards = new LobbySelectionCard[maps.Length];

            for (int i = 0; i < maps.Length; i++)
            {
                LobbySelectionCard card = Instantiate(mapCardPrefab, mapCardsParent);
                card.name = "Generated_MapCard_" + i;
                card.gameObject.SetActive(true);
                mapCards[i] = card;
            }

            if (mapCardPrefab.gameObject.scene.IsValid())
                mapCardPrefab.gameObject.SetActive(false);
        }

        if (mapCards == null)
            return;

        for (int i = 0; i < mapCards.Length; i++)
        {
            if (mapCards[i] == null)
                continue;

            if (maps == null || i >= maps.Length)
            {
                mapCards[i].gameObject.SetActive(false);
                continue;
            }

            mapCards[i].gameObject.SetActive(true);
            mapCards[i].SetupMap(this, i, maps[i]);
        }
    }

    void BuildCarCards()
    {
        if (autoBuildCards && carCardPrefab != null && carCardsParent != null && HasCars())
        {
            ClearGeneratedCards(carCardsParent, carCardPrefab);
            carCards = new LobbySelectionCard[cars.Length];

            for (int i = 0; i < cars.Length; i++)
            {
                LobbySelectionCard card = Instantiate(carCardPrefab, carCardsParent);
                card.name = "Generated_CarCard_" + i;
                card.gameObject.SetActive(true);
                carCards[i] = card;
            }

            if (carCardPrefab.gameObject.scene.IsValid())
                carCardPrefab.gameObject.SetActive(false);
        }

        if (carCards == null)
            return;

        for (int i = 0; i < carCards.Length; i++)
        {
            if (carCards[i] == null)
                continue;

            if (cars == null || i >= cars.Length)
            {
                carCards[i].gameObject.SetActive(false);
                continue;
            }

            carCards[i].gameObject.SetActive(true);
            carCards[i].SetupCar(this, i, cars[i]);
        }
    }

    void ClearGeneratedCards(Transform parent, LobbySelectionCard prefab)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            LobbySelectionCard card = child.GetComponent<LobbySelectionCard>();
            if (card == null || card == prefab)
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    void RefreshAll(string status)
    {
        ClampSelection();
        RefreshCards();
        RefreshTexts(status);
        RefreshStats();
    }

    void RefreshCards()
    {
        if (mapCards != null)
        {
            for (int i = 0; i < mapCards.Length; i++)
            {
                if (mapCards[i] != null)
                    mapCards[i].SetSelected(i == selectedMapIndex);
            }
        }

        if (carCards != null)
        {
            for (int i = 0; i < carCards.Length; i++)
            {
                if (carCards[i] != null)
                    carCards[i].SetSelected(i == selectedCarIndex);
            }
        }
    }

    void RefreshTexts(string status)
    {
        LobbyMapOption map = GetSelectedMap();
        LobbyCarOption car = GetSelectedCar();

        string mapName = map != null ? map.displayName : "Chua co";
        string carName = car != null ? car.displayName : "Chua co";

        if (statusText != null)
            statusText.text = status;

        if (selectedMapText != null)
            selectedMapText.text = "Map: " + mapName;

        if (selectedCarText != null)
            selectedCarText.text = "Xe: " + carName;

        if (currentSelectionText != null)
            currentSelectionText.text = "Map: " + mapName + "\nXe: " + carName;

        if (selectedMapNameText != null)
            selectedMapNameText.text = mapName;

        if (selectedMapDistanceText != null)
            selectedMapDistanceText.text = map != null ? map.distanceText : "";

        if (selectedCarNameText != null)
            selectedCarNameText.text = carName;

        if (selectedCarClassText != null)
            selectedCarClassText.text = car != null ? car.classLabel : "";

        if (selectedCarDescriptionText != null)
            selectedCarDescriptionText.text = car != null ? car.description : "";

        RefreshRaceInfo(map);
        RefreshTrackInfo(map);
    }

    void RefreshRaceInfo(LobbyMapOption map)
    {
        if (map == null)
            return;

        if (raceInfoText != null)
            raceInfoText.text = "LAPS\n" + map.laps + "\n\nTIME OF DAY\n" + map.timeOfDay + "\n\nWEATHER\n" + map.weather + "\n\nTRAFFIC\n" + map.traffic;

        if (lapsText != null)
            lapsText.text = map.laps;

        if (timeOfDayText != null)
            timeOfDayText.text = map.timeOfDay;

        if (weatherText != null)
            weatherText.text = map.weather;

        if (trafficText != null)
            trafficText.text = map.traffic;
    }

    void RefreshTrackInfo(LobbyMapOption map)
    {
        if (trackInfoText != null)
            trackInfoText.text = map != null ? map.trackInfo : "";

        if (trackPreviewImage != null && map != null)
        {
            Sprite sprite = map.miniMapImage != null ? map.miniMapImage : map.previewImage;
            if (sprite != null)
                trackPreviewImage.sprite = sprite;
        }
    }

    void RefreshStats()
    {
        LobbyCarOption car = GetSelectedCar();
        if (car == null)
            return;

        SetStat(topSpeedFill, topSpeedValueText, car.topSpeed);
        SetStat(accelerationFill, accelerationValueText, car.acceleration);
        SetStat(handlingFill, handlingValueText, car.handling);
        SetStat(brakingFill, brakingValueText, car.braking);
        SetStat(nitroFill, nitroValueText, car.nitro);
    }

    void SetStat(Image fill, TMP_Text valueText, int value)
    {
        if (fill != null)
        {
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = Mathf.Clamp01(value / 100f);
        }

        if (valueText != null)
            valueText.text = value.ToString();
    }

    void ApplySelectionState()
    {
        ClampSelection();

        LobbyMapOption map = GetSelectedMap();
        LobbyCarOption car = GetSelectedCar();

        CarSelectionState.SelectedMapIndex = selectedMapIndex;
        CarSelectionState.SelectedMapSceneName = map != null ? map.sceneName : "";
        CarSelectionState.SelectedMapDisplayName = map != null ? map.displayName : "";

        CarSelectionState.SelectedCarIndex = selectedCarIndex;
        CarSelectionState.SelectedCarId = car != null ? car.id : "";
        CarSelectionState.SelectedCarDisplayName = car != null ? car.displayName : "";
        CarSelectionState.SelectedCarClass = car != null ? car.classLabel : "";
        CarSelectionState.SelectedCarPrefab = car != null ? car.carPrefab : null;

        if (car != null)
        {
            CarSelectionState.SelectedTopSpeed = car.topSpeed;
            CarSelectionState.SelectedAcceleration = car.acceleration;
            CarSelectionState.SelectedHandling = car.handling;
            CarSelectionState.SelectedBraking = car.braking;
            CarSelectionState.SelectedNitro = car.nitro;
        }
    }

    void ClampSelection()
    {
        selectedMapIndex = HasMaps() ? Mathf.Clamp(selectedMapIndex, 0, maps.Length - 1) : 0;
        selectedCarIndex = HasCars() ? Mathf.Clamp(selectedCarIndex, 0, cars.Length - 1) : 0;

        if (HasMaps() && maps[selectedMapIndex].locked)
            selectedMapIndex = GetRandomUnlockedMapIndex();

        if (HasCars() && cars[selectedCarIndex].locked)
            selectedCarIndex = GetRandomUnlockedCarIndex();
    }

    bool HasMaps()
    {
        return maps != null && maps.Length > 0;
    }

    bool HasCars()
    {
        return cars != null && cars.Length > 0;
    }

    LobbyMapOption GetSelectedMap()
    {
        if (!HasMaps())
            return null;

        return maps[Mathf.Clamp(selectedMapIndex, 0, maps.Length - 1)];
    }

    LobbyCarOption GetSelectedCar()
    {
        if (!HasCars())
            return null;

        return cars[Mathf.Clamp(selectedCarIndex, 0, cars.Length - 1)];
    }

    void EnsureDefaultData()
    {
        if (maps == null || maps.Length == 0)
            maps = CreateDefaultMaps();

        if (cars == null || cars.Length == 0)
            cars = CreateDefaultCars();

        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i] == null)
                maps[i] = new LobbyMapOption { id = "map_" + i, displayName = "MAP " + (i + 1), sceneName = "" };
        }

        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i] == null)
                cars[i] = new LobbyCarOption { id = "car_" + i, displayName = "CAR " + (i + 1) };
        }
    }

    LobbyMapOption[] CreateDefaultMaps()
    {
        return new[]
        {
            new LobbyMapOption
            {
                id = "complete_track_demo",
                displayName = "TRACK RACE",
                distanceText = "-- KM",
                sceneName = "complete_track_demo",
                laps = "---",
                timeOfDay = "NOON",
                weather = "CLEAR",
                traffic = "MEDIUM",
                trackInfo = "Tu dien thong tin duong dua tai day."
            }
        };
    }

    LobbyCarOption[] CreateDefaultCars()
    {
        return new[]
        {
            new LobbyCarOption
            {
                id = "supra_mk4",
                displayName = "1993 TOYOTA SUPRA MK4",
                classLabel = "CLASS A",
                topSpeed = 86,
                acceleration = 82,
                handling = 78,
                braking = 74,
                nitro = 80,
                description = "Strong straight-line build with stable corner exits."
            },
            new LobbyCarOption
            {
                id = "bmw_m3_gtr_e46",
                displayName = "2005 BMW M3 GTR E46",
                classLabel = "CLASS S",
                topSpeed = 90,
                acceleration = 86,
                handling = 84,
                braking = 80,
                nitro = 82,
                description = "Race-tuned grip car with balanced power and control."
            },
            new LobbyCarOption
            {
                id = "subaru_wrx_sti_police",
                displayName = "2008 SUBARU WRX STI POLICE",
                classLabel = "CLASS A",
                topSpeed = 78,
                acceleration = 80,
                handling = 88,
                braking = 82,
                nitro = 72,
                description = "All-wheel-drive control, quick recovery, and reliable braking."
            },
            new LobbyCarOption
            {
                id = "lexus_lfa",
                displayName = "2012 LEXUS LFA",
                classLabel = "CLASS S",
                topSpeed = 92,
                acceleration = 88,
                handling = 82,
                braking = 78,
                nitro = 85,
                description = "High-rev supercar with sharp response and strong nitro pull."
            },
            new LobbyCarOption
            {
                id = "golf_mk7_gti",
                displayName = "2019 VW GOLF MK7 GTI",
                classLabel = "CLASS B",
                topSpeed = 74,
                acceleration = 76,
                handling = 84,
                braking = 76,
                nitro = 68,
                description = "Compact hatch with easy handling and forgiving corner speed."
            },
            new LobbyCarOption
            {
                id = "taycan_turbo_s",
                displayName = "2020 PORSCHE TAYCAN TURBO S",
                classLabel = "CLASS S",
                topSpeed = 94,
                acceleration = 96,
                handling = 80,
                braking = 84,
                nitro = 88,
                description = "Instant electric launch with heavy but very fast acceleration."
            }
        };
    }

    TMP_Text FindFirstText(GameObject root, params string[] names)
    {
        foreach (string textName in names)
        {
            TMP_Text found = MainMenuFlow.FindComponentIn(root, textName, typeof(TMP_Text)) as TMP_Text;
            if (found != null)
                return found;
        }

        return null;
    }

    Image FindFirstImage(GameObject root, params string[] names)
    {
        foreach (string imageName in names)
        {
            Image found = MainMenuFlow.FindComponentIn(root, imageName, typeof(Image)) as Image;
            if (found != null)
                return found;
        }

        return null;
    }

    Transform FindTransform(GameObject root, params string[] names)
    {
        foreach (string objectName in names)
        {
            GameObject found = MainMenuFlow.FindChild(root, objectName);
            if (found != null)
                return found.transform;
        }

        return null;
    }

    // Compatibility for old online button bindings. Lobby is offline now, so these route to the new selection flow.
    public void CreateRoom() => StartSelectedMap();
    public void JoinByCode() => RefreshAll("Online da tat. Lobby hien dung de chon map va xe.");
    public void StartQuickJoin() => RandomizeSelection();
    public void StopQuickJoin() => RefreshAll("Da dung random.");
    public void LeaveCurrentRoom()
    {
        if (menuFlow != null)
            menuFlow.ShowMainMenu();
    }
}
