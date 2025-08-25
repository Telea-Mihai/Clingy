using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HighScoreUI : MonoBehaviour
{
    [Header("High Score UI")]
    public GameObject highScorePanel;
    public Transform highScoreContainer;
    public GameObject highScoreEntryPrefab;
    public Button showHighScoresButton;
    public Button closeHighScoresButton;
    [Header("Stats UI")]
    public GameObject statsPanel;
    public Button showStatsButton;
    public Button closeStatsButton;
    [Header("Stats Text References")]
    public Text totalGamesText;
    public Text totalScoreText;
    public Text totalTimeText;
    public Text totalKillsText;
    public Text accuracyText;
    public Text headshotPercentageText;
    public Text bestWaveText;
    public Text bestKillStreakText;
    public Text bestSurvivalTimeText;
    public Text averageScoreText;
    [Header("New High Score UI")]
    public GameObject newHighScorePanel;
    public InputField playerNameInput;
    public Button submitHighScoreButton;
    public Text newHighScoreText;
    private List<GameObject> highScoreEntries = new List<GameObject>();
    private int pendingScore;
    private int pendingWave;
    private int pendingKills;
    private float pendingTime;

    void Start()
    {
        SetupButtons();
        if (highScorePanel != null) highScorePanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (newHighScorePanel != null) newHighScorePanel.SetActive(false);
    }

    void SetupButtons()
    {
        if (showHighScoresButton != null)
            showHighScoresButton.onClick.AddListener(ShowHighScores);
        if (closeHighScoresButton != null)
            closeHighScoresButton.onClick.AddListener(HideHighScores);
        if (showStatsButton != null)
            showStatsButton.onClick.AddListener(ShowStats);
        if (closeStatsButton != null)
            closeStatsButton.onClick.AddListener(HideStats);
        if (submitHighScoreButton != null)
            submitHighScoreButton.onClick.AddListener(SubmitHighScore);
    }

    public void ShowHighScores()
    {
        if (highScorePanel == null || SaveSystem.Instance == null) return;
        RefreshHighScoreDisplay();
        highScorePanel.SetActive(true);
    }

    public void HideHighScores()
    {
        if (highScorePanel != null)
            highScorePanel.SetActive(false);
    }

    public void ShowStats()
    {
        if (statsPanel == null || SaveSystem.Instance == null) return;
        RefreshStatsDisplay();
        statsPanel.SetActive(true);
    }

    public void HideStats()
    {
        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    void RefreshHighScoreDisplay()
    {
        foreach (GameObject entry in highScoreEntries)
        {
            if (entry != null)
                DestroyImmediate(entry);
        }
        highScoreEntries.Clear();

        List<HighScoreEntry> highScores = SaveSystem.Instance.GetHighScores();
        for (int i = 0; i < highScores.Count; i++)
        {
            CreateHighScoreEntry(highScores[i], i + 1);
        }
    }

    void CreateHighScoreEntry(HighScoreEntry scoreEntry, int rank)
    {
        if (highScoreEntryPrefab == null || highScoreContainer == null) return;

        GameObject entry = Instantiate(highScoreEntryPrefab, highScoreContainer);
        highScoreEntries.Add(entry);

        Text[] texts = entry.GetComponentsInChildren<Text>();
        if (texts.Length >= 6)
        {
            texts[0].text = rank.ToString();
            texts[1].text = scoreEntry.playerName;
            texts[2].text = scoreEntry.score.ToString("N0");
            texts[3].text = scoreEntry.wave.ToString();
            texts[4].text = scoreEntry.enemiesKilled.ToString();
            texts[5].text = FormatTime(scoreEntry.timePlayed);
        }

        Image background = entry.GetComponent<Image>();
        if (background != null)
        {
            switch (rank)
            {
                case 1:
                    background.color = new Color(1f, 0.8f, 0f, 0.3f);
                    break;
                case 2:
                    background.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
                    break;
                case 3:
                    background.color = new Color(0.8f, 0.5f, 0.2f, 0.3f);
                    break;
            }
        }
    }

    void RefreshStatsDisplay()
    {
        PlayerStats stats = SaveSystem.Instance.GetPlayerStats();
        if (totalGamesText != null)
            totalGamesText.text = stats.totalGamesPlayed.ToString("N0");
        if (totalScoreText != null)
            totalScoreText.text = stats.totalScore.ToString("N0");
        if (totalTimeText != null)
            totalTimeText.text = FormatTime(stats.totalTimePlayed);
        if (totalKillsText != null)
            totalKillsText.text = stats.totalEnemiesKilled.ToString("N0");
        if (accuracyText != null)
            accuracyText.text = stats.AccuracyPercentage.ToString("F1") + "%";
        if (headshotPercentageText != null)
            headshotPercentageText.text = stats.HeadshotPercentage.ToString("F1") + "%";
        if (bestWaveText != null)
            bestWaveText.text = stats.highestWaveReached.ToString();
        if (bestKillStreakText != null)
            bestKillStreakText.text = stats.bestKillStreak.ToString();
        if (bestSurvivalTimeText != null)
            bestSurvivalTimeText.text = FormatTime(stats.bestSurvivalTime);
        if (averageScoreText != null)
            averageScoreText.text = stats.AverageScore.ToString("F0");
    }

    public void ShowNewHighScoreDialog(int score, int wave, int kills, float time)
    {
        if (newHighScorePanel == null || SaveSystem.Instance == null) return;

        pendingScore = score;
        pendingWave = wave;
        pendingKills = kills;
        pendingTime = time;

        if (newHighScoreText != null)
        {
            int rank = SaveSystem.Instance.GetHighScoreRank(score);
            newHighScoreText.text = $"NEW HIGH SCORE!\nRank #{rank}\nScore: {score:N0}";
        }

        if (playerNameInput != null)
        {
            playerNameInput.text = SaveSystem.Instance.GetPlayerName();
        }

        newHighScorePanel.SetActive(true);
    }

    public void SubmitHighScore()
    {
        if (SaveSystem.Instance == null || playerNameInput == null) return;

        string playerName = string.IsNullOrEmpty(playerNameInput.text) ? "Anonymous" : playerNameInput.text;
        SaveSystem.Instance.SetPlayerName(playerName);
        SaveSystem.Instance.AddHighScore(playerName, pendingScore, pendingWave, pendingKills, pendingTime);
        if (newHighScorePanel != null)
            newHighScorePanel.SetActive(false);
        ShowHighScores();
    }

    string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 60)
            return $"{timeInSeconds:F0}s";
        else if (timeInSeconds < 3600)
            return $"{timeInSeconds / 60:F0}m {timeInSeconds % 60:F0}s";
        else
            return $"{timeInSeconds / 3600:F0}h {(timeInSeconds % 3600) / 60:F0}m";
    }

    public void CheckForNewHighScore(int score, int wave, int kills, float time)
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsHighScore(score))
        {
            ShowNewHighScoreDialog(score, wave, kills, time);
        }
    }

    public void ResetAllData()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.ResetAllData();
            RefreshHighScoreDisplay();
            RefreshStatsDisplay();
        }
    }
}
