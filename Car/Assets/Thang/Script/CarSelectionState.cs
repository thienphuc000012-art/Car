using UnityEngine;

public static class CarSelectionState
{
    public static bool HasMenuSelection = false;
    public static int SelectedCarIndex = 0;
    public static string SelectedCarId = "";
    public static string SelectedCarDisplayName = "";
    public static string SelectedCarClass = "";
    public static GameObject SelectedCarPrefab;
    public static int SelectedTopSpeed = 0;
    public static int SelectedAcceleration = 0;
    public static int SelectedHandling = 0;
    public static int SelectedBraking = 0;
    public static int SelectedNitro = 0;

    public static int SelectedMapIndex = 0;
    public static string SelectedMapSceneName = "";
    public static string SelectedMapDisplayName = "";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticState()
    {
        HasMenuSelection = false;
        SelectedCarIndex = 0;
        SelectedCarId = "";
        SelectedCarDisplayName = "";
        SelectedCarClass = "";
        SelectedCarPrefab = null;
        SelectedTopSpeed = 0;
        SelectedAcceleration = 0;
        SelectedHandling = 0;
        SelectedBraking = 0;
        SelectedNitro = 0;
        SelectedMapIndex = 0;
        SelectedMapSceneName = "";
        SelectedMapDisplayName = "";
    }
}
