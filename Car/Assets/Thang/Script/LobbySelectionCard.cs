using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySelectionCard : MonoBehaviour
{
    [Header("UI")]
    public Button button;
    public Image previewImage;
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public GameObject selectedMark;
    public GameObject lockedOverlay;
    public Image selectionFrame;

    [Header("Colors")]
    public Color normalFrameColor = new Color(1f, 1f, 1f, 0.25f);
    public Color selectedFrameColor = new Color(1f, 0.48f, 0.08f, 1f);

    LobbyRoomController controller;
    int optionIndex = -1;
    bool isMapCard;

    void Awake()
    {
        BindMissingReferences();
    }

    public void SetupMap(LobbyRoomController owner, int index, LobbyMapOption option)
    {
        BindMissingReferences();
        if (option == null)
        {
            gameObject.SetActive(false);
            return;
        }

        controller = owner;
        optionIndex = index;
        isMapCard = true;

        SetVisual(option.previewImage, option.displayName, option.distanceText, option.locked);
        BindClick(option.locked);
    }

    public void SetupCar(LobbyRoomController owner, int index, LobbyCarOption option)
    {
        BindMissingReferences();
        if (option == null)
        {
            gameObject.SetActive(false);
            return;
        }

        controller = owner;
        optionIndex = index;
        isMapCard = false;

        SetVisual(option.previewImage, option.displayName, option.classLabel, option.locked);
        BindClick(option.locked);
    }

    public void SetSelected(bool selected)
    {
        if (selectedMark != null)
            selectedMark.SetActive(selected);

        if (selectionFrame != null)
            selectionFrame.color = selected ? selectedFrameColor : normalFrameColor;
    }

    void SetVisual(Sprite preview, string title, string subtitle, bool locked)
    {
        if (previewImage != null && preview != null)
            previewImage.sprite = preview;

        if (titleText != null)
            titleText.text = title;

        if (subtitleText != null)
            subtitleText.text = subtitle;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(locked);

        if (button != null)
            button.interactable = !locked;
    }

    void BindClick(bool locked)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();

        if (locked)
            return;

        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (controller == null || optionIndex < 0)
            return;

        if (isMapCard)
            controller.SelectMap(optionIndex);
        else
            controller.SelectCar(optionIndex);
    }

    void BindMissingReferences()
    {
        button = button != null ? button : GetComponent<Button>();
        previewImage = previewImage != null ? previewImage : FindImage("PreviewImage", "ThumbnailImage", "MapImage", "CarImage", "VehicleImage");
        selectionFrame = selectionFrame != null ? selectionFrame : FindImage("SelectionFrame", "SelectedFrame", "Border", "Outline");
        selectedMark = selectedMark != null ? selectedMark : FindObject("SelectedMark", "CheckMark", "CheckIcon", "SelectedIcon");
        lockedOverlay = lockedOverlay != null ? lockedOverlay : FindObject("LockedOverlay", "LockOverlay", "LockedIcon", "LockIcon");

        if (titleText == null || subtitleText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            if (titleText == null && texts.Length > 0)
                titleText = texts[0];

            if (subtitleText == null && texts.Length > 1)
                subtitleText = texts[1];
        }
    }

    Image FindImage(params string[] names)
    {
        GameObject found = FindObject(names);
        return found != null ? found.GetComponent<Image>() : null;
    }

    GameObject FindObject(params string[] names)
    {
        foreach (string objectName in names)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName)
                    return child.gameObject;
            }
        }

        return null;
    }
}
