using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Race Settings")]
    public int totalLaps = 2;

    [Header("UI")]
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI leaderboardText;



    private int playerLapCount = 0;
    private int nextCheckpointIndex = 0;
    private float raceTime = 0f;

    public List<GameObject> racers = new List<GameObject>();
    [Header("UI")]
    public GameObject winPanel;         
    [Header("Results UI")]
    public Transform resultsContainer;   
    public GameObject resultRowPrefab;

    [Header("Win Panel Buttons")]
    public Button exitButton;
    public Button mainMenuButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        UpdateLapUI();
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void Update()
    {
        raceTime += Time.deltaTime;
        UpdateTimeUI();
        UpdateLeaderboard();
    }

    public void PlayerCrossCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex == nextCheckpointIndex)
        {
            nextCheckpointIndex++;

            if (nextCheckpointIndex >= 3) 
            {
                nextCheckpointIndex = 0;
                playerLapCount++;
                UpdateLapUI();

                if (playerLapCount >= totalLaps)
                {
                    var player = racers.Find(r => r.CompareTag("Player"));
                    if (player != null)
                    {
                        var rp = player.GetComponent<RacerProgress>();
                        rp.FinishRace(raceTime);
                    }

                    ShowResults();
                }

            }
        }
    }

    void UpdateLapUI()
    {
        if (lapText != null)
            lapText.text = "Lap: " + playerLapCount + "/" + totalLaps;
    }

    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(raceTime / 60f);
            int seconds = Mathf.FloorToInt(raceTime % 60f);
            int milliseconds = Mathf.FloorToInt((raceTime * 1000f) % 1000f);

            timeText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds / 10);

        }
    }


    void UpdateLeaderboard()
    {
        if (leaderboardText != null && racers.Count > 0)
        {
            racers.Sort((a, b) =>
            {
                var ra = a.GetComponent<RacerProgress>();
                var rb = b.GetComponent<RacerProgress>();

                int lapCompare = rb.lapCount.CompareTo(ra.lapCount);
                if (lapCompare != 0) return lapCompare;

                return rb.distanceTravelled.CompareTo(ra.distanceTravelled);
            });
            int playerRank = -1;
            for (int i = 0; i < racers.Count; i++)
            {
                if (racers[i].CompareTag("Player"))
                {
                    playerRank = i + 1; 
                    break;
                }
            }
            if (playerRank != -1)
            {
                leaderboardText.text = "Your Rank: " + playerRank + "/" + racers.Count;
            }
        }
    }



    void ShowResults()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true); 

            foreach (Transform child in resultsContainer)
            {
                Destroy(child.gameObject);
            }

            GameObject headerRow = Instantiate(resultRowPrefab, resultsContainer);
            TextMeshProUGUI[] headerTexts = headerRow.GetComponentsInChildren<TextMeshProUGUI>();
            headerTexts[0].text = "Position";
            headerTexts[1].text = "Car";
            headerTexts[2].text = "Race Time";
            foreach (var t in headerTexts) { t.fontStyle = FontStyles.Bold; }

            for (int i = 0; i < racers.Count; i++)
            {
                var rp = racers[i].GetComponent<RacerProgress>();
                string timeStr;
                if (rp.finishTime >= 0f)
                {
                    int minutes = Mathf.FloorToInt(rp.finishTime / 60f);
                    int seconds = Mathf.FloorToInt(rp.finishTime % 60f);
                    int milliseconds = Mathf.FloorToInt((rp.finishTime * 1000f) % 1000f);

                    timeStr = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds / 10);
                }
                else
                {
                    timeStr = "--:--";
                }

                GameObject row = Instantiate(resultRowPrefab, resultsContainer);
                TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                texts[0].text = (i + 1).ToString();   
                texts[1].text = racers[i].name;       
                texts[2].text = timeStr;              

                if (racers[i].CompareTag("Player"))
                {
                    foreach (var t in texts) { t.color = Color.red; }
                }
            }

            AudioSettingsManager asm = FindFirstObjectByType<AudioSettingsManager>();
            if (asm != null && asm.bgmSource != null) asm.bgmSource.Stop();

            Time.timeScale = 0f;
        }
    }
    public void AICrossCheckpoint(GameObject aiCar, int checkpointIndex)
    {
        var rp = aiCar.GetComponent<RacerProgress>();
        if (rp == null) return;
        if (rp.lapCount >= totalLaps)
        {
            rp.FinishRace(raceTime);
        }
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void RestartRace()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

}

