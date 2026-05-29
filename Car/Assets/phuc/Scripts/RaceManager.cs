using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Race Settings")]
    public int totalLaps = 2;

    [Header("UI")]
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI leaderboardText;
    public GameObject winPanel;
    public TextMeshProUGUI resultsText; 

    private int playerLapCount = 0;
    private int nextCheckpointIndex = 0;
    private float raceTime = 0f;

    public List<GameObject> racers = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        UpdateLapUI();
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
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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

            string board = "Leaderboard:\n";
            int topCount = Mathf.Min(3, racers.Count);
            for (int i = 0; i < topCount; i++)
            {
                string racerName = racers[i].name;
                if (racers[i].CompareTag("Player"))
                {
                    racerName = "<color=red>" + racerName + "</color>";
                }

                board += (i + 1) + ". " + racerName + "\n";
            }
            leaderboardText.text = board;
        }
    }


    void ShowResults()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
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
                    var rp = racers[i].GetComponent<RacerProgress>();
                    rp.FinishRace(raceTime);

                    int minutes = Mathf.FloorToInt(rp.finishTime / 60f);
                    int seconds = Mathf.FloorToInt(rp.finishTime % 60f);

                    if (resultsText != null)
                    {
                        resultsText.text = "Top " + playerRank +
                                           "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
                    }
                    break;
                }

            }

            Time.timeScale = 0f; // dừng game
        }
    }

}
