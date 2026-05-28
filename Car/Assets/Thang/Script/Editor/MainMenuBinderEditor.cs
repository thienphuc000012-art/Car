#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
    static readonly string[] LobbyMapSceneNames = { "complete_track_demo" };
    static readonly string[] RequiredBuildScenePaths =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/complete_track_demo.unity"
    };

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
        DisableObsoleteOnlineObjects();
        BindOnClicks(flow, lobby);
        EnsureButtonSounds();
        EnsurePanelPopEffects();
        EnsureRequiredBuildScenes();

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
        flow.lobbyPanel = Find("LobbyPanel");
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
        settings.musicWheelHandle = GetSliderWheelVisual(settings.musicSlider);
        settings.sfxWheelHandle = GetSliderWheelVisual(settings.sfxSlider);
        EditorUtility.SetDirty(settings);
    }

    static void BindLobby(LobbyRoomController lobby, MainMenuFlow flow)
    {
        Undo.RecordObject(lobby, "Bind Lobby Selection Controller");
        GameObject lobbyPanel = Find("LobbyPanel");

        lobby.menuFlow = flow;
        EnsureDefaultLobbyData(lobby);

        lobby.mapCardsParent = GetFirstTransformIn(lobbyPanel, "MapCardsParent", "MapList", "MapContent", "SelectMapContent");
        lobby.carCardsParent = GetFirstTransformIn(lobbyPanel, "CarCardsParent", "CarList", "CarContent", "SelectCarContent");
        lobby.mapCardPrefab = GetFirstComponentIn<LobbySelectionCard>(lobbyPanel, "MapCardPrefab", "MapCardTemplate", "MapCard");
        lobby.carCardPrefab = GetFirstComponentIn<LobbySelectionCard>(lobbyPanel, "CarCardPrefab", "CarCardTemplate", "CarCard");
        lobby.mapCards = FindSelectionCards(lobbyPanel, "MapCard");
        lobby.carCards = FindSelectionCards(lobbyPanel, "CarCard");

        lobby.statusText = GetComponentIn<TMP_Text>(lobbyPanel, "StatusText");
        lobby.selectedMapText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedMapText", "MapText", "MapNameText");
        lobby.selectedCarText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedCarText", "CarText", "CarNameText");
        lobby.currentSelectionText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "CurrentSelectionText", "CurrentRoomText");
        lobby.selectedMapNameText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedMapNameText", "SelectedTrackNameText", "TrackNameText");
        lobby.selectedMapDistanceText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedMapDistanceText", "SelectedTrackDistanceText", "TrackDistanceText");
        lobby.selectedCarNameText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedCarNameText", "SelectedVehicleNameText", "VehicleNameText");
        lobby.selectedCarClassText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedCarClassText", "SelectedVehicleClassText", "VehicleClassText");
        lobby.selectedCarDescriptionText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "SelectedCarDescriptionText", "CarDescriptionText");

        lobby.raceInfoText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "RaceInfoText");
        lobby.lapsText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "LapsText", "LapText");
        lobby.timeOfDayText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "TimeOfDayText", "TimeText");
        lobby.weatherText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "WeatherText");
        lobby.trafficText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "TrafficText");
        lobby.trackInfoText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "TrackInfoText", "TrackDescriptionText");
        lobby.trackPreviewImage = GetFirstComponentIn<Image>(lobbyPanel, "TrackPreviewImage", "MiniMapImage", "TrackImage");

        lobby.topSpeedFill = GetFirstComponentIn<Image>(lobbyPanel, "TopSpeedFill", "SpeedFill");
        lobby.accelerationFill = GetFirstComponentIn<Image>(lobbyPanel, "AccelerationFill", "AccelFill");
        lobby.handlingFill = GetFirstComponentIn<Image>(lobbyPanel, "HandlingFill");
        lobby.brakingFill = GetFirstComponentIn<Image>(lobbyPanel, "BrakingFill", "BrakeFill");
        lobby.nitroFill = GetFirstComponentIn<Image>(lobbyPanel, "NitroFill", "BoostFill");

        lobby.topSpeedValueText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "TopSpeedValueText", "SpeedValueText");
        lobby.accelerationValueText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "AccelerationValueText", "AccelValueText");
        lobby.handlingValueText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "HandlingValueText");
        lobby.brakingValueText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "BrakingValueText", "BrakeValueText");
        lobby.nitroValueText = GetFirstComponentIn<TMP_Text>(lobbyPanel, "NitroValueText", "BoostValueText");

        EditorUtility.SetDirty(lobby);
    }

    static void EnsureDefaultLobbyData(LobbyRoomController lobby)
    {
        LobbyMapOption[] oldMaps = lobby.maps;
        LobbyMapOption[] newMaps = new LobbyMapOption[LobbyMapSceneNames.Length];

        for (int i = 0; i < LobbyMapSceneNames.Length; i++)
        {
            string sceneName = LobbyMapSceneNames[i];
            LobbyMapOption map = FindExistingMap(oldMaps, sceneName);

            if (map == null && oldMaps != null && i < oldMaps.Length)
                map = oldMaps[i];

            if (map == null)
                map = CreateMapOption(sceneName, i);

            ConfigureMapScene(map, sceneName, i);
            newMaps[i] = map;
        }

        lobby.maps = newMaps;

        if (lobby.cars == null || lobby.cars.Length == 0)
            lobby.cars = CreateDefaultCarOptions();
    }

    static LobbyMapOption FindExistingMap(LobbyMapOption[] maps, string sceneName)
    {
        if (maps == null)
            return null;

        foreach (LobbyMapOption map in maps)
        {
            if (map != null && map.sceneName == sceneName)
                return map;
        }

        return null;
    }

    static void ConfigureMapScene(LobbyMapOption map, string sceneName, int index)
    {
        map.id = sceneName;
        map.sceneName = sceneName;
        map.sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/" + sceneName + ".unity");

        if (string.IsNullOrWhiteSpace(map.displayName))
            map.displayName = SceneNameToDisplayName(sceneName, index);

        if (string.IsNullOrWhiteSpace(map.distanceText))
            map.distanceText = "-- KM";

        if (string.IsNullOrWhiteSpace(map.laps))
            map.laps = "---";

        if (string.IsNullOrWhiteSpace(map.timeOfDay))
            map.timeOfDay = "NOON";

        if (string.IsNullOrWhiteSpace(map.weather))
            map.weather = "CLEAR";

        if (string.IsNullOrWhiteSpace(map.traffic))
            map.traffic = "MEDIUM";
    }

    static LobbyMapOption CreateMapOption(string sceneName, int index)
    {
        string displayName = SceneNameToDisplayName(sceneName, index);
        string distance = index == 0 ? "-- KM" : index == 1 ? "3.6 KM" : "2.5 KM";

        return new LobbyMapOption
        {
            id = sceneName,
            displayName = displayName,
            distanceText = distance,
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/" + sceneName + ".unity"),
            sceneName = sceneName,
            laps = index == 0 ? "---" : index == 1 ? "2" : "3",
            timeOfDay = index == 2 ? "NIGHT" : index == 1 ? "SUNSET" : "NOON",
            weather = index == 1 ? "LIGHT FOG" : "CLEAR",
            traffic = index == 1 ? "LOW" : "MEDIUM",
            trackInfo = "Tu dien thong tin duong dua tai day."
        };
    }

    static string SceneNameToDisplayName(string sceneName, int index)
    {
        if (sceneName == "complete_track_demo")
            return "TRACK RACE";

        if (sceneName == "s1")
            return "COASTAL DRIVE";

        return string.IsNullOrWhiteSpace(sceneName) ? "MAP " + (index + 1) : sceneName.Replace("_", " ").ToUpperInvariant();
    }

    static LobbyCarOption[] CreateDefaultCarOptions()
    {
        return new[]
        {
            new LobbyCarOption { id = "supra_mk4", displayName = "1993 TOYOTA SUPRA MK4", classLabel = "CLASS A", topSpeed = 86, acceleration = 82, handling = 78, braking = 74, nitro = 80 },
            new LobbyCarOption { id = "bmw_m3_gtr_e46", displayName = "2005 BMW M3 GTR E46", classLabel = "CLASS S", topSpeed = 90, acceleration = 86, handling = 84, braking = 80, nitro = 82 },
            new LobbyCarOption { id = "subaru_wrx_sti_police", displayName = "2008 SUBARU WRX STI POLICE", classLabel = "CLASS A", topSpeed = 78, acceleration = 80, handling = 88, braking = 82, nitro = 72 },
            new LobbyCarOption { id = "lexus_lfa", displayName = "2012 LEXUS LFA", classLabel = "CLASS S", topSpeed = 92, acceleration = 88, handling = 82, braking = 78, nitro = 85 },
            new LobbyCarOption { id = "golf_mk7_gti", displayName = "2019 VW GOLF MK7 GTI", classLabel = "CLASS B", topSpeed = 74, acceleration = 76, handling = 84, braking = 76, nitro = 68 },
            new LobbyCarOption { id = "taycan_turbo_s", displayName = "2020 PORSCHE TAYCAN TURBO S", classLabel = "CLASS S", topSpeed = 94, acceleration = 96, handling = 80, braking = 84, nitro = 88 }
        };
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

    static void DisableObsoleteOnlineObjects()
    {
        SetObjectActive("CreateRoomPanel", false);
        SetObjectActive("JoinRoomPanel", false);
        SetObjectActive("GaragePanel", false);
        SetObjectActive("LobbyGaragePanel", false);

        ClearButtonAndHide("GarageButton");
        ClearButtonAndHide("GarageButon");
        ClearButtonAndHide("CreateRoomButton");
        ClearButtonAndHide("JoinRoomButton");
        ClearButtonAndHide("QuickJoinButton");
        ClearButtonAndHide("StopQuickJoinButton");
        ClearButtonAndHide("LobbyGarageButton");
    }

    static void SetObjectActive(string objectName, bool active)
    {
        GameObject obj = Find(objectName);
        if (obj == null)
            return;

        Undo.RecordObject(obj, "Update Main Menu Object Active");
        obj.SetActive(active);
        EditorUtility.SetDirty(obj);
    }

    static void ClearButtonAndHide(string buttonName)
    {
        GameObject obj = Find(buttonName);
        if (obj == null)
            return;

        Button button = obj.GetComponent<Button>();
        if (button != null)
        {
            Undo.RecordObject(button, "Clear Obsolete Button OnClick");
            ClearPersistentListeners(button.onClick);
            EditorUtility.SetDirty(button);
        }

        Undo.RecordObject(obj, "Hide Obsolete Button");
        obj.SetActive(false);
        EditorUtility.SetDirty(obj);
    }

    static void BindOnClicks(MainMenuFlow flow, LobbyRoomController lobby)
    {
        SetButtonClick(flow.mainMenuPanel, "PlayButton", flow, nameof(MainMenuFlow.ShowLobby));
        SetButtonClick(flow.mainMenuPanel, "SettingButton", flow, nameof(MainMenuFlow.OpenSettings));
        SetButtonClick(flow.mainMenuPanel, "SettingsButton", flow, nameof(MainMenuFlow.OpenSettings));
        SetButtonClick(flow.mainMenuPanel, "QuitButton", flow, nameof(MainMenuFlow.QuitGame));

        SetButtonClick(flow.lobbyPanel, "StartButton", lobby, nameof(LobbyRoomController.StartSelectedMap));
        SetButtonClick(flow.lobbyPanel, "RandomButton", lobby, nameof(LobbyRoomController.RandomizeSelection));
        SetButtonClick(flow.lobbyPanel, "randomButton", lobby, nameof(LobbyRoomController.RandomizeSelection));
        SetButtonClick(flow.lobbyPanel, "SettingButton", flow, nameof(MainMenuFlow.OpenSettings));
        SetButtonClick(flow.lobbyPanel, "SettingsButton", flow, nameof(MainMenuFlow.OpenSettings));
        SetButtonClick(flow.lobbyPanel, "BackButton", flow, nameof(MainMenuFlow.ShowMainMenu));
        SetButtonClick(flow.lobbyPanel, "LeaveButton", flow, nameof(MainMenuFlow.ShowMainMenu));
        SetButtonClick(flow.settingsPanel, "BackButton", flow, nameof(MainMenuFlow.CloseSettings));

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
            "MainMenuPanel", "LobbyPanel", "SettingsPanel"
        };

        foreach (string panelName in panelNames)
        {
            GameObject panel = Find(panelName);
            if (panel != null)
                EnsureComponent<UIPanelPopEffect>(panel);
        }
    }

    static void EnsureRequiredBuildScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (string requiredPath in RequiredBuildScenePaths)
        {
            bool found = false;
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path != requiredPath)
                    continue;

                scenes[i] = new EditorBuildSettingsScene(requiredPath, true);
                found = true;
                break;
            }

            if (!found)
                scenes.Add(new EditorBuildSettingsScene(requiredPath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
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

    static T GetFirstComponentIn<T>(GameObject root, params string[] objectNames) where T : Component
    {
        foreach (string objectName in objectNames)
        {
            T component = GetComponentIn<T>(root, objectName);
            if (component != null)
                return component;
        }

        return null;
    }

    static Transform GetFirstTransformIn(GameObject root, params string[] objectNames)
    {
        foreach (string objectName in objectNames)
        {
            GameObject obj = FindIn(root, objectName);
            if (obj != null)
                return obj.transform;
        }

        return null;
    }

    static LobbySelectionCard[] FindSelectionCards(GameObject root, string namePart)
    {
        List<LobbySelectionCard> cards = new List<LobbySelectionCard>();
        if (root == null)
            return cards.ToArray();

        foreach (LobbySelectionCard card in root.GetComponentsInChildren<LobbySelectionCard>(true))
        {
            if (card.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0)
                cards.Add(card);
        }

        return cards.ToArray();
    }

    static RectTransform GetSliderWheelVisual(Slider slider)
    {
        if (slider == null || slider.handleRect == null)
            return null;

        Image[] images = slider.handleRect.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            RectTransform imageRect = image.rectTransform;
            if (imageRect != slider.handleRect)
                return imageRect;
        }

        return slider.handleRect;
    }

    static string[] FindMapSceneNames()
    {
        List<string> sceneNames = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(sceneName) || sceneName == "MainMenu")
                continue;

            if (!sceneNames.Contains(sceneName))
                sceneNames.Add(sceneName);
        }

        return sceneNames.ToArray();
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



