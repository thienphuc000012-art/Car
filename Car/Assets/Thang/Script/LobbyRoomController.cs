using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyRoomController : MonoBehaviour
{
    public MainMenuFlow menuFlow;

    [Header("Maps")]
    public string[] mapSceneNames = { "complete_track_demo", "phuc", "s1" };
    public string[] mapDisplayNames = { "complete_track_demo", "phuc", "s1" };
    public int selectedMapIndex;

    [Header("Cars")]
    public string[] carIds =
    {
        "1993 Toyota Supra MK4",
        "2005 BMW M3 GTR E46",
        "2008 Subaru Impreza WRX STi Police",
        "2012 Lexus LFA",
        "2019 VW Golf MK7 GTI",
        "2020 Porsche Taycan Turbo S"
    };

    public string[] carDisplayNames =
    {
        "1993 Toyota Supra MK4",
        "2005 BMW M3 GTR E46",
        "2008 Subaru Impreza WRX STi - Police",
        "2012 Lexus LFA",
        "2019 VW Golf MK7 GTI",
        "2020 Porsche Taycan Turbo S"
    };

    public int selectedCarIndex;

    [Header("Texts")]
    public TMP_Text statusText;
    public TMP_Text selectedMapText;
    public TMP_Text selectedCarText;
    public TMP_Text currentSelectionText;

    void Start()
    {
        BindSceneObjects();
        ClampSelection();
        ApplySelectionState();
        UpdateTexts("San sang chon xe va map.");
    }

    public void BindSceneObjects()
    {
        menuFlow = menuFlow != null ? menuFlow : GetComponent<MainMenuFlow>();

        GameObject lobbyPanel = MainMenuFlow.FindSceneObject("LobbyPanel");
        statusText = statusText != null ? statusText : MainMenuFlow.FindComponentIn(lobbyPanel, "StatusText", typeof(TMP_Text)) as TMP_Text;
        selectedMapText = selectedMapText != null ? selectedMapText : FindFirstText(lobbyPanel, "SelectedMapText", "MapText", "MapNameText");
        selectedCarText = selectedCarText != null ? selectedCarText : FindFirstText(lobbyPanel, "SelectedCarText", "CarText", "CarNameText");
        currentSelectionText = currentSelectionText != null ? currentSelectionText : FindFirstText(lobbyPanel, "CurrentSelectionText", "CurrentRoomText");
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

    public void SelectNextMap()
    {
        if (mapSceneNames == null || mapSceneNames.Length == 0)
        {
            UpdateTexts("Chua co map de chon.");
            return;
        }

        selectedMapIndex = (selectedMapIndex + 1) % mapSceneNames.Length;
        ApplySelectionState();
        UpdateTexts("Da chon map tiep theo.");
    }

    public void SelectPreviousMap()
    {
        if (mapSceneNames == null || mapSceneNames.Length == 0)
        {
            UpdateTexts("Chua co map de chon.");
            return;
        }

        selectedMapIndex = (selectedMapIndex - 1 + mapSceneNames.Length) % mapSceneNames.Length;
        ApplySelectionState();
        UpdateTexts("Da chon map truoc.");
    }

    public void SelectNextCar()
    {
        if (carIds == null || carIds.Length == 0)
        {
            UpdateTexts("Chua co xe de chon.");
            return;
        }

        selectedCarIndex = (selectedCarIndex + 1) % carIds.Length;
        ApplySelectionState();
        UpdateTexts("Da chon xe tiep theo.");
    }

    public void SelectPreviousCar()
    {
        if (carIds == null || carIds.Length == 0)
        {
            UpdateTexts("Chua co xe de chon.");
            return;
        }

        selectedCarIndex = (selectedCarIndex - 1 + carIds.Length) % carIds.Length;
        ApplySelectionState();
        UpdateTexts("Da chon xe truoc.");
    }

    public void RandomizeSelection()
    {
        if (mapSceneNames != null && mapSceneNames.Length > 0)
            selectedMapIndex = mapSceneNames.Length == 1 ? 0 : Random.Range(0, mapSceneNames.Length);

        if (carIds != null && carIds.Length > 0)
            selectedCarIndex = carIds.Length == 1 ? 0 : Random.Range(0, carIds.Length);

        ApplySelectionState();
        UpdateTexts("Da random xe va map.");
    }

    public void StartSelectedMap()
    {
        ClampSelection();
        ApplySelectionState();

        string sceneName = GetSelectedMapSceneName();
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            UpdateTexts("Chua co map. Hay gan Map Scene Names trong Inspector.");
            return;
        }

        UpdateTexts("Dang vao map " + GetSelectedMapDisplayName() + "...");
        SceneManager.LoadScene(sceneName);
    }

    void ClampSelection()
    {
        selectedMapIndex = ClampIndex(selectedMapIndex, mapSceneNames);
        selectedCarIndex = ClampIndex(selectedCarIndex, carIds);
    }

    int ClampIndex(int index, string[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        return Mathf.Clamp(index, 0, values.Length - 1);
    }

    void ApplySelectionState()
    {
        ClampSelection();
        CarSelectionState.SelectedCarIndex = selectedCarIndex;
        CarSelectionState.SelectedCarId = GetSelectedCarId();
        CarSelectionState.SelectedMapIndex = selectedMapIndex;
        CarSelectionState.SelectedMapSceneName = GetSelectedMapSceneName();
        CarSelectionState.SelectedMapDisplayName = GetSelectedMapDisplayName();
    }

    void UpdateTexts(string status)
    {
        if (statusText != null)
            statusText.text = status;

        string mapName = GetSelectedMapDisplayName();
        string carName = GetSelectedCarDisplayName();

        if (selectedMapText != null)
            selectedMapText.text = string.IsNullOrEmpty(mapName) ? "Map: Chua co" : "Map: " + mapName;

        if (selectedCarText != null)
            selectedCarText.text = string.IsNullOrEmpty(carName) ? "Xe: Chua co" : "Xe: " + carName;

        if (currentSelectionText != null)
            currentSelectionText.text = "Map: " + (string.IsNullOrEmpty(mapName) ? "Chua co" : mapName) + "\nXe: " + (string.IsNullOrEmpty(carName) ? "Chua co" : carName);
    }

    string GetSelectedMapSceneName()
    {
        if (mapSceneNames == null || mapSceneNames.Length == 0)
            return "";

        return mapSceneNames[Mathf.Clamp(selectedMapIndex, 0, mapSceneNames.Length - 1)];
    }

    string GetSelectedMapDisplayName()
    {
        if (mapSceneNames == null || mapSceneNames.Length == 0)
            return "";

        if (mapDisplayNames != null && selectedMapIndex >= 0 && selectedMapIndex < mapDisplayNames.Length && !string.IsNullOrWhiteSpace(mapDisplayNames[selectedMapIndex]))
            return mapDisplayNames[selectedMapIndex];

        return GetSelectedMapSceneName();
    }

    string GetSelectedCarId()
    {
        if (carIds == null || carIds.Length == 0)
            return "";

        return carIds[Mathf.Clamp(selectedCarIndex, 0, carIds.Length - 1)];
    }

    string GetSelectedCarDisplayName()
    {
        if (carIds == null || carIds.Length == 0)
            return "";

        if (carDisplayNames != null && selectedCarIndex >= 0 && selectedCarIndex < carDisplayNames.Length && !string.IsNullOrWhiteSpace(carDisplayNames[selectedCarIndex]))
            return carDisplayNames[selectedCarIndex];

        return GetSelectedCarId();
    }

    // Compatibility for old online button bindings. These intentionally no longer create/join/leave rooms.
    public void CreateRoom() => StartSelectedMap();
    public void JoinByCode() => UpdateTexts("Online da tat. Lobby hien dung de chon map va xe.");
    public void StartQuickJoin() => RandomizeSelection();
    public void StopQuickJoin() => UpdateTexts("Da dung random.");
    public void LeaveCurrentRoom()
    {
        if (menuFlow != null)
            menuFlow.ShowMainMenu();
    }
}
