#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuBinderEditor
{
    [MenuItem("Tools/Thang/Bind Main Menu")]
    public static void BindMainMenu()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "MainMenu")
        {
            EditorUtility.DisplayDialog("Bind Main Menu", "Hay mo dung scene MainMenu truoc khi chay tool nay.", "OK");
            return;
        }

        GameObject managerObject = RequireObject("MainMenuManager");
        GameObject audioObject = RequireObject("AudioManager");

        MainMenuFlow flow = EnsureComponent<MainMenuFlow>(managerObject);
        SettingsMenu settings = EnsureComponent<SettingsMenu>(managerObject);
        LobbyRoomController lobby = EnsureComponent<LobbyRoomController>(managerObject);
        AudioManager audio = EnsureComponent<AudioManager>(audioObject);

        BindMainMenuFlow(flow);
        BindSettings(settings);
        BindLobby(lobby, flow);
        BindAudio(audio);
        BindAudioClips(audio);
        BindHelpers();
        BindBackgroundVfxLayers();
        BindOnClicks(flow, lobby);
        EnsureButtonSounds();
        EnsurePanelPopEffects();

        EditorUtility.SetDirty(managerObject);
        EditorUtility.SetDirty(audioObject);
        EditorUtility.SetDirty(flow);
        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(lobby);
        EditorUtility.SetDirty(audio);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Bind Main Menu", "Da gan reference va OnClick cho scene MainMenu.", "OK");
    }

    static void BindMainMenuFlow(MainMenuFlow flow)
    {
        Undo.RecordObject(flow, "Bind Main Menu Flow");

        flow.loadingPanel = Find("LoadingPanel");
        flow.pressAnyKeyPanel = Find("PressAnyKeyPanel");
        flow.mainMenuPanel = Find("MainMenuPanel");
        flow.garagePanel = Find("GaragePanel");
        flow.lobbyPanel = Find("LobbyPanel");
        flow.createRoomPanel = Find("CreateRoomPanel");
        flow.joinRoomPanel = Find("JoinRoomPanel");
        flow.lobbyGaragePanel = Find("LobbyGaragePanel");
        flow.settingsPanel = Find("SettingsPanel");

        flow.loadingBarFill = GetComponentIn<Image>(flow.loadingPanel, "LoadingBarFill");
        flow.loadingPercentText = GetComponentIn<TMP_Text>(flow.loadingPanel, "LoadingPercentText");
        flow.loadingStatusText = GetComponentIn<TMP_Text>(flow.loadingPanel, "LoadingStatusText");
        flow.fullscreenDropdown = GetComponentIn<TMP_Dropdown>(flow.settingsPanel, "FullscreenDropdown");

        GameObject fadePanel = Find("FadePanel");
        flow.fadeGroup = fadePanel != null ? EnsureComponent<CanvasGroup>(fadePanel) : null;

        if (flow.loadingBarFill != null)
        {
            Undo.RecordObject(flow.loadingBarFill, "Configure Loading Fill");
            flow.loadingBarFill.type = Image.Type.Filled;
            flow.loadingBarFill.fillMethod = Image.FillMethod.Horizontal;
            flow.loadingBarFill.fillOrigin = 0;
            flow.loadingBarFill.fillAmount = 0f;
            EditorUtility.SetDirty(flow.loadingBarFill);
        }

        if (flow.fullscreenDropdown != null)
            SetDropdownOptions(flow.fullscreenDropdown, "Windowed", "Fullscreen");
    }

    static void BindSettings(SettingsMenu settings)
    {
        Undo.RecordObject(settings, "Bind Settings Menu");
        settings.musicSlider = GetComponent<Slider>("MusicSlider");
        settings.sfxSlider = GetComponent<Slider>("SFXSlider");
    }

    static void BindLobby(LobbyRoomController lobby, MainMenuFlow flow)
    {
        Undo.RecordObject(lobby, "Bind Lobby Room Controller");

        GameObject createPanel = Find("CreateRoomPanel");
        GameObject joinPanel = Find("JoinRoomPanel");
        GameObject lobbyPanel = Find("LobbyPanel");

        lobby.menuFlow = flow;
        lobby.createRoomNameInput = GetComponentIn<TMP_InputField>(createPanel, "RoomNameInput");
        lobby.createPasswordInput = GetComponentIn<TMP_InputField>(createPanel, "PasswordInput");
        lobby.privacyDropdown = GetComponentIn<TMP_Dropdown>(createPanel, "PrivacyDropdown");
        lobby.joinRuleDropdown = GetComponentIn<TMP_Dropdown>(createPanel, "JoinRuleDropdown");
        lobby.roomCodeInput = GetComponentIn<TMP_InputField>(joinPanel, "RoomCodeInput");
        lobby.joinPasswordInput = GetComponentIn<TMP_InputField>(joinPanel, "PasswordInput");
        lobby.statusText = GetComponentIn<TMP_Text>(lobbyPanel, "StatusText");
        lobby.currentRoomText = GetComponentIn<TMP_Text>(lobbyPanel, "CurrentRoomText");
        lobby.searchStatusText = GetComponentIn<TMP_Text>(joinPanel, "SearchStatusText");
        lobby.stopQuickJoinButton = FindIn(lobbyPanel, "StopQuickJoinButton");

        if (lobby.stopQuickJoinButton != null)
            lobby.stopQuickJoinButton.SetActive(false);

        if (lobby.privacyDropdown != null)
            SetDropdownOptions(lobby.privacyDropdown, "Public", "Private");

        if (lobby.joinRuleDropdown != null)
            SetDropdownOptions(lobby.joinRuleDropdown, "Vao ngay", "Chu phong duyet");
    }

    static void BindAudio(AudioManager audio)
    {
        Undo.RecordObject(audio, "Bind Audio Manager");

        AudioSource[] sources = audio.GetComponents<AudioSource>();
        while (sources.Length < 2)
        {
            Undo.AddComponent<AudioSource>(audio.gameObject);
            sources = audio.GetComponents<AudioSource>();
        }

        audio.musicSource = sources[0];
        audio.sfxSource = sources[1];
        audio.musicSource.playOnAwake = false;
        audio.musicSource.loop = true;
        audio.sfxSource.playOnAwake = false;
        audio.sfxSource.loop = false;

        EditorUtility.SetDirty(audio.musicSource);
        EditorUtility.SetDirty(audio.sfxSource);
    }


    static void BindAudioClips(AudioManager audio)
    {
        if (audio == null)
            return;

        AssetDatabase.Refresh();
        Undo.RecordObject(audio, "Bind Main Menu Audio Clips");

        AudioClip menuMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Thang/Audio/MainMenu/racing_game_menu_bpm165.ogg");
        AudioClip startupSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Thang/Audio/MainMenu/startup_engine_sound.mp3");
        AudioClip clickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Thang/Audio/MainMenu/ui_click_kenney_click_001.ogg");
        AudioClip hoverSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Thang/Audio/MainMenu/ui_hover_kenney_select_001.ogg");

        if (menuMusic != null)
            audio.menuMusic = menuMusic;

        if (startupSfx != null)
            audio.startupCarSfx = startupSfx;

        if (clickSfx != null)
            audio.buttonClickSFX = clickSfx;

        if (hoverSfx != null)
            audio.hoverSFX = hoverSfx;

        EditorUtility.SetDirty(audio);
    }
    static void BindHelpers()
    {
        GameObject animatedBackground = Find("AnimatedBackground");
        if (animatedBackground != null)
        {
            MovingUIBackground mover = EnsureComponent<MovingUIBackground>(animatedBackground);
            Undo.RecordObject(mover, "Bind Moving Background");
            mover.rawImage = animatedBackground.GetComponent<RawImage>();
            EditorUtility.SetDirty(mover);
        }

        GameObject clickText = Find("ClickText");
        if (clickText != null)
        {
            BlinkText blink = EnsureComponent<BlinkText>(clickText);
            Undo.RecordObject(blink, "Bind Blink Text");
            blink.targetText = clickText.GetComponent<TMP_Text>();
            EditorUtility.SetDirty(blink);
        }
    }


    static void BindBackgroundVfxLayers()
    {
        GameObject loadingPanel = Find("LoadingPanel");
        GameObject pressPanel = Find("PressAnyKeyPanel");

        Component loadingVfx = EnsureVfxLayer(loadingPanel, "LoadingVfxLayer", 0);
        if (loadingVfx != null)
        {
            Undo.RecordObject(loadingVfx, "Configure Loading VFX");
            SetSerializedValue(loadingVfx, "seed", 1993);
            SetSerializedValue(loadingVfx, "snowCount", 150);
            SetSerializedValue(loadingVfx, "rebuildOnStart", true);
            EditorUtility.SetDirty(loadingVfx);
        }

        Component pressVfx = EnsureVfxLayer(pressPanel, "PressAnyKeyVfxLayer", 1);
        if (pressVfx != null)
        {
            Undo.RecordObject(pressVfx, "Configure Press Any Key VFX");
            SetSerializedValue(pressVfx, "seed", 2005);
            SetSerializedValue(pressVfx, "dustCount", 90);
            SetSerializedValue(pressVfx, "rebuildOnStart", true);
            EditorUtility.SetDirty(pressVfx);
        }
    }

    static Component EnsureVfxLayer(GameObject parent, string layerName, int presetIndex)
    {
        if (parent == null)
            return null;

        Type vfxType = Type.GetType("MainMenuBackgroundVfx, Assembly-CSharp");
        if (vfxType == null)
        {
            Debug.LogWarning("MainMenuBackgroundVfx chua duoc Unity compile. Hay doi Unity compile xong roi chay Tools > Thang > Bind Main Menu lai.");
            return null;
        }

        GameObject layer = FindIn(parent, layerName);
        if (layer == null)
        {
            layer = new GameObject(layerName, typeof(RectTransform), typeof(CanvasRenderer));
            Undo.RegisterCreatedObjectUndo(layer, "Create Main Menu VFX Layer");
            layer.transform.SetParent(parent.transform, false);
        }

        RectTransform rect = EnsureComponent<RectTransform>(layer);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        PlaceVfxLayerAboveBackground(parent, rect);

        Component vfx = layer.GetComponent(vfxType);
        if (vfx == null)
            vfx = Undo.AddComponent(layer, vfxType);

        SetSerializedValue(vfx, "preset", presetIndex);
        return vfx;
    }

    static void SetSerializedValue(UnityEngine.Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        if (property.propertyType == SerializedPropertyType.Enum)
            property.enumValueIndex = value;
        else
            property.intValue = value;

        serializedObject.ApplyModifiedProperties();
    }

    static void SetSerializedValue(UnityEngine.Object target, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        property.boolValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    static void PlaceVfxLayerAboveBackground(GameObject parent, RectTransform vfxLayer)
    {
        GameObject background = FindIn(parent, "BackgroundImage");
        if (background == null)
            background = FindIn(parent, "AnimatedBackground");

        if (background != null && background.transform.parent == parent.transform)
        {
            int targetIndex = Mathf.Min(background.transform.GetSiblingIndex() + 1, parent.transform.childCount - 1);
            vfxLayer.SetSiblingIndex(targetIndex);
            return;
        }

        vfxLayer.SetAsFirstSibling();
    }

    static void BindOnClicks(MainMenuFlow flow, LobbyRoomController lobby)
    {
        SetButtonClick(flow.mainMenuPanel, "PlayButton", flow, nameof(MainMenuFlow.ShowLobby));
        SetButtonClick(flow.mainMenuPanel, "GarageButton", flow, nameof(MainMenuFlow.OpenGarage));
        SetButtonClick(flow.mainMenuPanel, "GarageButon", flow, nameof(MainMenuFlow.OpenGarage));
        SetButtonClick(flow.mainMenuPanel, "SettingButton", flow, nameof(MainMenuFlow.OpenSettings));
        SetButtonClick(flow.mainMenuPanel, "SettingsButton", flow, nameof(MainMenuFlow.OpenSettings));
        SetButtonClick(flow.mainMenuPanel, "QuitButton", flow, nameof(MainMenuFlow.QuitGame));

        SetButtonClick(flow.garagePanel, "BackButton", flow, nameof(MainMenuFlow.CloseGarage));
        SetButtonClick(flow.garagePanel, "BackButtonGarage", flow, nameof(MainMenuFlow.CloseGarage));

        SetButtonClick(flow.lobbyPanel, "CreateRoomButton", flow, nameof(MainMenuFlow.OpenCreateRoom));
        SetButtonClick(flow.lobbyPanel, "JoinRoomButton", flow, nameof(MainMenuFlow.OpenJoinRoom));
        SetButtonClick(flow.lobbyPanel, "QuickJoinButton", lobby, nameof(LobbyRoomController.StartQuickJoin));
        SetButtonClick(flow.lobbyPanel, "StopQuickJoinButton", lobby, nameof(LobbyRoomController.StopQuickJoin));
        SetButtonClick(flow.lobbyPanel, "LobbyGarageButton", flow, nameof(MainMenuFlow.OpenLobbyGarage));
        SetButtonClick(flow.lobbyPanel, "LeaveButton", lobby, nameof(LobbyRoomController.LeaveCurrentRoom));
        SetButtonClick(flow.lobbyPanel, "BackButton", flow, nameof(MainMenuFlow.ShowMainMenu));

        SetButtonClick(flow.createRoomPanel, "CreateConfirmButton", lobby, nameof(LobbyRoomController.CreateRoom));
        SetButtonClick(flow.createRoomPanel, "BackButton", flow, nameof(MainMenuFlow.ShowLobby));

        SetButtonClick(flow.joinRoomPanel, "JoinConfirmButton", lobby, nameof(LobbyRoomController.JoinByCode));
        SetButtonClick(flow.joinRoomPanel, "BackButton", flow, nameof(MainMenuFlow.ShowLobby));

        SetButtonClick(flow.lobbyGaragePanel, "BackButton", flow, nameof(MainMenuFlow.ShowLobby));
        SetButtonClick(flow.settingsPanel, "BackButton", flow, nameof(MainMenuFlow.ShowMainMenu));

        SetSliderFloatEvent(Find("MusicSlider")?.GetComponent<Slider>(), Find("MainMenuManager")?.GetComponent<SettingsMenu>(), nameof(SettingsMenu.SetMusicVolume));
        SetSliderFloatEvent(Find("SFXSlider")?.GetComponent<Slider>(), Find("MainMenuManager")?.GetComponent<SettingsMenu>(), nameof(SettingsMenu.SetSFXVolume));
        SetDropdownIntEvent(flow.fullscreenDropdown, flow, nameof(MainMenuFlow.SetFullscreenMode));
    }

    static void EnsureButtonSounds()
    {
        foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (!button.gameObject.scene.IsValid())
                continue;

            EnsureComponent<UIButtonSound>(button.gameObject);
        }
    }

    static void EnsurePanelPopEffects()
    {
        string[] panelNames =
        {
            "MainMenuPanel", "GaragePanel", "LobbyPanel", "CreateRoomPanel",
            "JoinRoomPanel", "LobbyGaragePanel", "SettingsPanel"
        };

        foreach (string panelName in panelNames)
        {
            GameObject panel = Find(panelName);
            if (panel != null)
                EnsureComponent<UIPanelPopEffect>(panel);
        }
    }

    static void SetButtonClick(GameObject root, string buttonName, UnityEngine.Object target, string methodName)
    {
        Button button = GetComponentIn<Button>(root, buttonName);
        if (button == null || target == null)
            return;

        Undo.RecordObject(button, "Bind Button OnClick");
        ClearPersistentListeners(button.onClick);
        UnityAction action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, methodName);
        UnityEventTools.AddPersistentListener(button.onClick, action);
        EditorUtility.SetDirty(button);
    }

    static void SetSliderFloatEvent(Slider slider, UnityEngine.Object target, string methodName)
    {
        if (slider == null || target == null)
            return;

        Undo.RecordObject(slider, "Bind Slider Event");
        ClearPersistentListeners(slider.onValueChanged);
        UnityAction<float> action = (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>), target, methodName);
        UnityEventTools.AddPersistentListener(slider.onValueChanged, action);
        EditorUtility.SetDirty(slider);
    }

    static void SetDropdownIntEvent(TMP_Dropdown dropdown, UnityEngine.Object target, string methodName)
    {
        if (dropdown == null || target == null)
            return;

        Undo.RecordObject(dropdown, "Bind Dropdown Event");
        ClearPersistentListeners(dropdown.onValueChanged);
        UnityAction<int> action = (UnityAction<int>)Delegate.CreateDelegate(typeof(UnityAction<int>), target, methodName);
        UnityEventTools.AddPersistentListener(dropdown.onValueChanged, action);
        EditorUtility.SetDirty(dropdown);
    }

    static void ClearPersistentListeners(UnityEventBase unityEvent)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(unityEvent, i);
    }

    static void SetDropdownOptions(TMP_Dropdown dropdown, params string[] options)
    {
        Undo.RecordObject(dropdown, "Set Dropdown Options");
        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>(options));
        dropdown.SetValueWithoutNotify(0);
        EditorUtility.SetDirty(dropdown);
    }

    static GameObject RequireObject(string objectName)
    {
        GameObject obj = Find(objectName);
        if (obj == null)
            throw new InvalidOperationException("Missing required object: " + objectName);

        return obj;
    }

    static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        if (obj == null)
            return null;

        T component = obj.GetComponent<T>();
        if (component == null)
            component = Undo.AddComponent<T>(obj);

        return component;
    }

    static T GetComponent<T>(string objectName) where T : Component
    {
        GameObject obj = Find(objectName);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    static T GetComponentIn<T>(GameObject root, string objectName) where T : Component
    {
        GameObject obj = FindIn(root, objectName);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    static GameObject Find(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            GameObject found = FindIn(root, objectName);
            if (found != null)
                return found;
        }

        return null;
    }

    static GameObject FindIn(GameObject root, string objectName)
    {
        if (root == null)
            return null;

        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }
}
#endif



