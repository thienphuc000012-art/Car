using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum RoomPrivacy
{
    Public,
    Private
}

public enum JoinRule
{
    Instant,
    HostApproval
}

[System.Serializable]
public class RoomInfo
{
    public string code;
    public string roomName;
    public RoomPrivacy privacy;
    public JoinRule joinRule;
    public string password;
    public int players;
    public int maxPlayers = 4;
}

public class LobbyRoomController : MonoBehaviour
{
    public MainMenuFlow menuFlow;

    [Header("Create Room")]
    public TMP_InputField createRoomNameInput;
    public TMP_InputField createPasswordInput;
    public TMP_Dropdown privacyDropdown;
    public TMP_Dropdown joinRuleDropdown;

    [Header("Join Room")]
    public TMP_InputField roomCodeInput;
    public TMP_InputField joinPasswordInput;

    [Header("Texts")]
    public TMP_Text statusText;
    public TMP_Text currentRoomText;
    public TMP_Text searchStatusText;

    [Header("Quick Join")]
    public GameObject stopQuickJoinButton;

    readonly Dictionary<string, RoomInfo> rooms = new Dictionary<string, RoomInfo>();
    string currentRoomCode = "";
    Coroutine quickJoinCoroutine;
    bool quickJoinActive;

    void Start()
    {
        BindSceneObjects();
        UpdateCurrentRoomText();
    }

    public void BindSceneObjects()
    {
        menuFlow = menuFlow != null ? menuFlow : GetComponent<MainMenuFlow>();

        GameObject createPanel = MainMenuFlow.FindSceneObject("CreateRoomPanel");
        GameObject joinPanel = MainMenuFlow.FindSceneObject("JoinRoomPanel");
        GameObject lobbyPanel = MainMenuFlow.FindSceneObject("LobbyPanel");

        createRoomNameInput = createRoomNameInput != null ? createRoomNameInput : MainMenuFlow.FindComponentIn(createPanel, "RoomNameInput", typeof(TMP_InputField)) as TMP_InputField;
        createPasswordInput = createPasswordInput != null ? createPasswordInput : MainMenuFlow.FindComponentIn(createPanel, "PasswordInput", typeof(TMP_InputField)) as TMP_InputField;
        privacyDropdown = privacyDropdown != null ? privacyDropdown : MainMenuFlow.FindComponentIn(createPanel, "PrivacyDropdown", typeof(TMP_Dropdown)) as TMP_Dropdown;
        joinRuleDropdown = joinRuleDropdown != null ? joinRuleDropdown : MainMenuFlow.FindComponentIn(createPanel, "JoinRuleDropdown", typeof(TMP_Dropdown)) as TMP_Dropdown;

        roomCodeInput = roomCodeInput != null ? roomCodeInput : MainMenuFlow.FindComponentIn(joinPanel, "RoomCodeInput", typeof(TMP_InputField)) as TMP_InputField;
        joinPasswordInput = joinPasswordInput != null ? joinPasswordInput : MainMenuFlow.FindComponentIn(joinPanel, "PasswordInput", typeof(TMP_InputField)) as TMP_InputField;
        searchStatusText = searchStatusText != null ? searchStatusText : MainMenuFlow.FindComponentIn(joinPanel, "SearchStatusText", typeof(TMP_Text)) as TMP_Text;

        statusText = statusText != null ? statusText : MainMenuFlow.FindComponentIn(lobbyPanel, "StatusText", typeof(TMP_Text)) as TMP_Text;
        currentRoomText = currentRoomText != null ? currentRoomText : MainMenuFlow.FindComponentIn(lobbyPanel, "CurrentRoomText", typeof(TMP_Text)) as TMP_Text;
        stopQuickJoinButton = stopQuickJoinButton != null ? stopQuickJoinButton : MainMenuFlow.FindChild(lobbyPanel, "StopQuickJoinButton");

        SetupDropdowns();

        if (stopQuickJoinButton != null)
            stopQuickJoinButton.SetActive(false);
    }

    void SetupDropdowns()
    {
        if (privacyDropdown != null)
        {
            privacyDropdown.ClearOptions();
            privacyDropdown.AddOptions(new List<string> { "Public", "Private" });
        }

        if (joinRuleDropdown != null)
        {
            joinRuleDropdown.ClearOptions();
            joinRuleDropdown.AddOptions(new List<string> { "Vao ngay", "Chu phong duyet" });
        }
    }

    public void CreateRoom()
    {
        BindSceneObjects();

        if (!string.IsNullOrEmpty(currentRoomCode))
        {
            SetStatus("Ban dang o trong phong roi.");
            return;
        }

        string code = GenerateRoomCode();
        string roomName = createRoomNameInput != null ? createRoomNameInput.text : "";
        string password = createPasswordInput != null ? createPasswordInput.text : "";

        RoomInfo room = new RoomInfo
        {
            code = code,
            roomName = string.IsNullOrWhiteSpace(roomName) ? "Room " + code : roomName,
            privacy = privacyDropdown != null && privacyDropdown.value == 1 ? RoomPrivacy.Private : RoomPrivacy.Public,
            joinRule = joinRuleDropdown != null && joinRuleDropdown.value == 1 ? JoinRule.HostApproval : JoinRule.Instant,
            password = password,
            players = 1,
            maxPlayers = 4
        };

        if (room.privacy == RoomPrivacy.Private && string.IsNullOrWhiteSpace(room.password))
        {
            SetStatus("Phong private can mat khau.");
            return;
        }

        rooms.Add(code, room);
        currentRoomCode = code;
        SetStatus("Da tao phong #" + code);
        SetSearchStatus("Da tao phong #" + code);
        UpdateCurrentRoomText();

        if (menuFlow != null)
            menuFlow.ShowLobby();
    }

    public void JoinByCode()
    {
        BindSceneObjects();
        string code = NormalizeCode(roomCodeInput != null ? roomCodeInput.text : "");

        if (string.IsNullOrEmpty(code))
        {
            SetSearchStatus("Nhap ma phong 4 so.");
            return;
        }

        if (!rooms.TryGetValue(code, out RoomInfo room))
        {
            SetSearchStatus("Khong tim thay phong #" + code);
            return;
        }

        SetSearchStatus("Tim thay phong #" + code);
        TryJoinRoom(room);
    }

    public void StartQuickJoin()
    {
        BindSceneObjects();

        if (!string.IsNullOrEmpty(currentRoomCode))
        {
            SetStatus("Ban dang o trong phong roi.");
            return;
        }

        if (quickJoinCoroutine != null)
            StopCoroutine(quickJoinCoroutine);

        quickJoinCoroutine = StartCoroutine(QuickJoinRoutine());
    }

    IEnumerator QuickJoinRoutine()
    {
        quickJoinActive = true;

        if (stopQuickJoinButton != null)
            stopQuickJoinButton.SetActive(true);

        SetStatus("Dang tham gia nhanh...");
        yield return new WaitForSeconds(0.5f);

        foreach (RoomInfo room in rooms.Values)
        {
            if (!quickJoinActive)
                yield break;

            if (room.privacy != RoomPrivacy.Public || room.players >= room.maxPlayers)
                continue;

            if (room.joinRule == JoinRule.Instant)
            {
                JoinRoom(room);
                FinishQuickJoin();
                yield break;
            }

            SetStatus("Dang cho chu phong #" + room.code + " chap nhan...");
            yield return new WaitForSeconds(2f);

            if (!string.IsNullOrEmpty(currentRoomCode))
            {
                SetStatus("Khong the vao: ban da o phong khac.");
                FinishQuickJoin();
                yield break;
            }

            JoinRoom(room);
            FinishQuickJoin();
            yield break;
        }

        SetStatus("Khong co phong public phu hop.");
        FinishQuickJoin();
    }

    public void StopQuickJoin()
    {
        quickJoinActive = false;

        if (quickJoinCoroutine != null)
        {
            StopCoroutine(quickJoinCoroutine);
            quickJoinCoroutine = null;
        }

        if (stopQuickJoinButton != null)
            stopQuickJoinButton.SetActive(false);

        SetStatus("Da dung tham gia nhanh.");
    }

    void FinishQuickJoin()
    {
        quickJoinActive = false;
        quickJoinCoroutine = null;

        if (stopQuickJoinButton != null)
            stopQuickJoinButton.SetActive(false);
    }

    public void LeaveCurrentRoom()
    {
        if (string.IsNullOrEmpty(currentRoomCode))
        {
            SetStatus("Ban chua o trong phong nao.");
            return;
        }

        if (rooms.TryGetValue(currentRoomCode, out RoomInfo room))
            room.players = Mathf.Max(0, room.players - 1);

        currentRoomCode = "";
        SetStatus("Da roi phong.");
        UpdateCurrentRoomText();
    }

    void TryJoinRoom(RoomInfo room)
    {
        if (!string.IsNullOrEmpty(currentRoomCode))
        {
            SetStatus("Ban dang o trong phong khac.");
            return;
        }

        if (room.players >= room.maxPlayers)
        {
            SetStatus("Phong da day.");
            return;
        }

        string password = joinPasswordInput != null ? joinPasswordInput.text : "";
        if (room.privacy == RoomPrivacy.Private && password != room.password)
        {
            SetStatus("Sai mat khau phong.");
            SetSearchStatus("Sai mat khau phong.");
            return;
        }

        if (room.joinRule == JoinRule.HostApproval)
        {
            SetStatus("Da gui yeu cau vao phong, cho chu phong chap nhan.");
            StartCoroutine(HostApprovalRoutine(room));
            return;
        }

        JoinRoom(room);
    }

    IEnumerator HostApprovalRoutine(RoomInfo room)
    {
        yield return new WaitForSeconds(2f);

        if (!string.IsNullOrEmpty(currentRoomCode))
        {
            SetStatus("Chu phong da chap nhan nhung ban da vao phong khac.");
            yield break;
        }

        JoinRoom(room);
    }

    void JoinRoom(RoomInfo room)
    {
        room.players++;
        currentRoomCode = room.code;
        SetStatus("Da vao phong #" + room.code);
        SetSearchStatus("Da vao phong #" + room.code);
        UpdateCurrentRoomText();

        if (menuFlow != null)
            menuFlow.ShowLobby();
    }

    string GenerateRoomCode()
    {
        for (int i = 0; i < 1000; i++)
        {
            string code = Random.Range(1000, 10000).ToString();
            if (!rooms.ContainsKey(code))
                return code;
        }

        return Random.Range(1000, 10000).ToString();
    }

    string NormalizeCode(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";

        raw = raw.Replace("#", "").Trim();
        return raw.Length > 4 ? raw.Substring(0, 4) : raw;
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void SetSearchStatus(string message)
    {
        if (searchStatusText != null)
            searchStatusText.text = message;
    }

    void UpdateCurrentRoomText()
    {
        if (currentRoomText == null)
            return;

        currentRoomText.text = string.IsNullOrEmpty(currentRoomCode)
            ? "Chua vao phong"
            : "Dang o phong #" + currentRoomCode;
    }
}
