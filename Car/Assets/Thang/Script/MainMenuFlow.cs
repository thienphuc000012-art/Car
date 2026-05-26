using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuFlow : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loadingPanel;
    public GameObject pressAnyKeyPanel;
    public GameObject mainMenuPanel;
    public GameObject garagePanel;
    public GameObject lobbyPanel;
    public GameObject createRoomPanel;
    public GameObject joinRoomPanel;
    public GameObject lobbyGaragePanel;
    public GameObject settingsPanel;

    [Header("Loading")]
    public Image loadingBarFill;
    public TMP_Text loadingPercentText;
    public TMP_Text loadingStatusText;
    public float firstLoadingTime = 2.5f;
    public AudioSource startupAudioSource;
    public AudioClip startupCarSfx;

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.15f;

    [Header("Fullscreen")]
    public TMP_Dropdown fullscreenDropdown;

    bool canClickToStart;
    bool isSwitching;
    bool garageOpenedFromLobby;
    GameObject currentPanel;
    SettingsMenu settingsMenu;
    LobbyRoomController lobbyRoomController;

    void Awake()
    {
        BindSceneObjects();
        EnsureHelpers();
        BindButtons();
    }

    void Start()
    {
        HideAllPanels();

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        if (fullscreenDropdown != null)
        {
            fullscreenDropdown.ClearOptions();
            fullscreenDropdown.AddOptions(new System.Collections.Generic.List<string> { "Windowed", "Fullscreen" });
            fullscreenDropdown.SetValueWithoutNotify(Screen.fullScreen ? 1 : 0);
            fullscreenDropdown.onValueChanged.AddListener(SetFullscreenMode);
        }

        StartCoroutine(FirstLoading());
    }

    void Update()
    {
        if (canClickToStart && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            canClickToStart = false;
            ShowMainMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            HandleBack();

        if (Input.GetKeyDown(KeyCode.F11))
            ToggleFullscreen();
    }

    void BindSceneObjects()
    {
        loadingPanel = loadingPanel != null ? loadingPanel : FindSceneObject("LoadingPanel");
        pressAnyKeyPanel = pressAnyKeyPanel != null ? pressAnyKeyPanel : FindSceneObject("PressAnyKeyPanel");
        mainMenuPanel = mainMenuPanel != null ? mainMenuPanel : FindSceneObject("MainMenuPanel");
        garagePanel = garagePanel != null ? garagePanel : FindSceneObject("GaragePanel");
        lobbyPanel = lobbyPanel != null ? lobbyPanel : FindSceneObject("LobbyPanel");
        createRoomPanel = createRoomPanel != null ? createRoomPanel : FindSceneObject("CreateRoomPanel");
        joinRoomPanel = joinRoomPanel != null ? joinRoomPanel : FindSceneObject("JoinRoomPanel");
        lobbyGaragePanel = lobbyGaragePanel != null ? lobbyGaragePanel : FindSceneObject("LobbyGaragePanel");
        settingsPanel = settingsPanel != null ? settingsPanel : FindSceneObject("SettingsPanel");

        loadingBarFill = loadingBarFill != null ? loadingBarFill : FindComponentIn(loadingPanel, "LoadingBarFill", typeof(Image)) as Image;
        loadingPercentText = loadingPercentText != null ? loadingPercentText : FindComponentIn(loadingPanel, "LoadingPercentText", typeof(TMP_Text)) as TMP_Text;
        loadingStatusText = loadingStatusText != null ? loadingStatusText : FindComponentIn(loadingPanel, "LoadingStatusText", typeof(TMP_Text)) as TMP_Text;
        fullscreenDropdown = fullscreenDropdown != null ? fullscreenDropdown : FindComponentIn(settingsPanel, "FullscreenDropdown", typeof(TMP_Dropdown)) as TMP_Dropdown;

        GameObject fadePanel = FindSceneObject("FadePanel");
        if (fadePanel != null)
        {
            fadeGroup = fadeGroup != null ? fadeGroup : fadePanel.GetComponent<CanvasGroup>();
            if (fadeGroup == null)
                fadeGroup = fadePanel.AddComponent<CanvasGroup>();
        }

        if (loadingBarFill != null)
        {
            loadingBarFill.type = Image.Type.Filled;
            loadingBarFill.fillMethod = Image.FillMethod.Horizontal;
            loadingBarFill.fillOrigin = 0;
            loadingBarFill.fillAmount = 0f;
        }

        settingsMenu = GetComponent<SettingsMenu>();
        if (settingsMenu == null)
            settingsMenu = gameObject.AddComponent<SettingsMenu>();

        lobbyRoomController = GetComponent<LobbyRoomController>();
        if (lobbyRoomController == null)
            lobbyRoomController = gameObject.AddComponent<LobbyRoomController>();

        lobbyRoomController.menuFlow = this;
    }

    void EnsureHelpers()
    {
        GameObject animatedBackground = FindSceneObject("AnimatedBackground");
        if (animatedBackground != null && animatedBackground.GetComponent<MovingUIBackground>() == null)
            animatedBackground.AddComponent<MovingUIBackground>();

        GameObject clickText = FindSceneObject("ClickText");
        if (clickText != null && clickText.GetComponent<BlinkText>() == null)
            clickText.AddComponent<BlinkText>();

        AddPanelPop(mainMenuPanel);
        AddPanelPop(garagePanel);
        AddPanelPop(lobbyPanel);
        AddPanelPop(createRoomPanel);
        AddPanelPop(joinRoomPanel);
        AddPanelPop(lobbyGaragePanel);
        AddPanelPop(settingsPanel);

        foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (!button.gameObject.scene.IsValid())
                continue;

            if (button.GetComponent<UIButtonSound>() == null)
                button.gameObject.AddComponent<UIButtonSound>();
        }
    }

    void AddPanelPop(GameObject panel)
    {
        if (panel != null && panel.GetComponent<UIPanelPopEffect>() == null)
            panel.AddComponent<UIPanelPopEffect>();
    }

    void BindButtons()
    {
        BindButton(mainMenuPanel, "PlayButton", ShowLobby);
        BindButton(mainMenuPanel, "GarageButton", OpenGarage);
        BindButton(mainMenuPanel, "GarageButon", OpenGarage);
        BindButton(mainMenuPanel, "SettingButton", OpenSettings);
        BindButton(mainMenuPanel, "SettingsButton", OpenSettings);
        BindButton(mainMenuPanel, "QuitButton", QuitGame);

        BindButton(garagePanel, "BackButton", CloseGarage);
        BindButton(garagePanel, "BackButtonGarage", CloseGarage);

        BindButton(lobbyPanel, "CreateRoomButton", OpenCreateRoom);
        BindButton(lobbyPanel, "JoinRoomButton", OpenJoinRoom);
        BindButton(lobbyPanel, "QuickJoinButton", () => lobbyRoomController.StartQuickJoin());
        BindButton(lobbyPanel, "StopQuickJoinButton", () => lobbyRoomController.StopQuickJoin());
        BindButton(lobbyPanel, "LobbyGarageButton", OpenLobbyGarage);
        BindButton(lobbyPanel, "LeaveButton", () => lobbyRoomController.LeaveCurrentRoom());
        BindButton(lobbyPanel, "BackButton", ShowMainMenu);

        BindButton(createRoomPanel, "CreateConfirmButton", () => lobbyRoomController.CreateRoom());
        BindButton(createRoomPanel, "BackButton", ShowLobby);

        BindButton(joinRoomPanel, "JoinConfirmButton", () => lobbyRoomController.JoinByCode());
        BindButton(joinRoomPanel, "BackButton", ShowLobby);

        BindButton(lobbyGaragePanel, "BackButton", ShowLobby);
        BindButton(settingsPanel, "BackButton", ShowMainMenu);
    }

    void BindButton(GameObject root, string buttonName, UnityAction action)
    {
        Button button = FindComponentIn(root, buttonName, typeof(Button)) as Button;
        if (button == null)
            return;

        button.onClick.AddListener(action);
    }

    IEnumerator FirstLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (startupAudioSource != null && startupCarSfx != null)
            startupAudioSource.PlayOneShot(startupCarSfx);
        else if (AudioManager.Instance != null)
            AudioManager.Instance.PlayStartupCarSfx();

        float timer = 0f;

        while (timer < firstLoadingTime)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / firstLoadingTime);
            SetLoadingProgress(progress);
            SetLoadingStatus(progress);
            yield return null;
        }

        SetLoadingProgress(1f);

        if (loadingStatusText != null)
            loadingStatusText.text = "Hoan tat";

        yield return new WaitForSeconds(0.25f);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (pressAnyKeyPanel != null)
        {
            pressAnyKeyPanel.SetActive(true);
            currentPanel = pressAnyKeyPanel;
        }

        canClickToStart = true;
    }

    void SetLoadingProgress(float value)
    {
        if (loadingBarFill != null)
            loadingBarFill.fillAmount = value;

        if (loadingPercentText != null)
            loadingPercentText.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    void SetLoadingStatus(float value)
    {
        if (loadingStatusText == null)
            return;

        if (value < 0.3f)
            loadingStatusText.text = "Dang khoi tao menu...";
        else if (value < 0.65f)
            loadingStatusText.text = "Dang tai garage...";
        else
            loadingStatusText.text = "Dang chuan bi lobby...";
    }

    void HandleBack()
    {
        if (IsActive(settingsPanel) || IsActive(lobbyPanel))
            ShowMainMenu();
        else if (IsActive(garagePanel))
            CloseGarage();
        else if (IsActive(createRoomPanel) || IsActive(joinRoomPanel) || IsActive(lobbyGaragePanel))
            ShowLobby();
    }

    bool IsActive(GameObject obj)
    {
        return obj != null && obj.activeSelf;
    }

    void HideAllPanels()
    {
        SetPanelActive(loadingPanel, false);
        SetPanelActive(pressAnyKeyPanel, false);
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(garagePanel, false);
        SetPanelActive(lobbyPanel, false);
        SetPanelActive(createRoomPanel, false);
        SetPanelActive(joinRoomPanel, false);
        SetPanelActive(lobbyGaragePanel, false);
        SetPanelActive(settingsPanel, false);
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    void ShowPanel(GameObject panel)
    {
        if (panel == null || isSwitching || currentPanel == panel)
            return;

        StartCoroutine(SwitchPanel(panel));
    }

    IEnumerator SwitchPanel(GameObject panel)
    {
        isSwitching = true;

        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(0f, 1f, Mathf.Clamp01(timer / fadeDuration));
                yield return null;
            }
        }

        HideAllPanels();
        panel.SetActive(true);
        currentPanel = panel;

        if (fadeGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(timer / fadeDuration));
                yield return null;
            }

            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        isSwitching = false;
    }

    public void ShowMainMenu() => ShowPanel(mainMenuPanel);
    public void OpenGarage()
    {
        garageOpenedFromLobby = false;
        ShowPanel(garagePanel);
    }

    public void CloseGarage()
    {
        if (garageOpenedFromLobby)
            ShowLobby();
        else
            ShowMainMenu();
    }

    public void ShowLobby() => ShowPanel(lobbyPanel);
    public void OpenCreateRoom() => ShowPanel(createRoomPanel);
    public void OpenJoinRoom() => ShowPanel(joinRoomPanel);

    public void OpenLobbyGarage()
    {
        garageOpenedFromLobby = true;
        ShowPanel(lobbyGaragePanel != null ? lobbyGaragePanel : garagePanel);
    }
    public void OpenSettings() => ShowPanel(settingsPanel);

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;

        if (fullscreenDropdown != null)
            fullscreenDropdown.SetValueWithoutNotify(Screen.fullScreen ? 1 : 0);
    }

    public void SetFullscreenMode(int index)
    {
        Screen.fullScreen = index == 1;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public static GameObject FindSceneObject(string objectName)
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (!transform.gameObject.scene.IsValid())
                continue;

            if (transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }

    public static Component FindComponentIn(GameObject root, string objectName, System.Type componentType)
    {
        GameObject obj = FindChild(root, objectName);
        return obj != null ? obj.GetComponent(componentType) : null;
    }

    public static GameObject FindChild(GameObject root, string objectName)
    {
        if (root == null)
            return FindSceneObject(objectName);

        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }
}


