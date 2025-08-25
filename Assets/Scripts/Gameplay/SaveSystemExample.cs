using UnityEngine;
using UnityEngine.UI;





public class SaveSystemExample : MonoBehaviour
{
    [Header("UI Elements for Testing")]
    public Button saveHighScoreButton;
    public Button loadHighScoresButton;
    public Button showStatsButton;
    public Button resetDataButton;
    public InputField playerNameInput;
    public InputField scoreInput;
    public Text displayText;

    [Header("Sample Game Data")]
    public int sampleScore = 1000;
    public int sampleWave = 5;
    public int sampleKills = 25;
    public float sampleTime = 120f;

    void Start()
    {
        SetupButtons();
        TestSaveSystem();
    }

    void SetupButtons()
    {
        if (saveHighScoreButton != null)
            saveHighScoreButton.onClick.AddListener(SaveSampleHighScore);
        if (loadHighScoresButton != null)
            loadHighScoresButton.onClick.AddListener(DisplayHighScores);
        if (showStatsButton != null)
            showStatsButton.onClick.AddListener(DisplayPlayerStats);
        if (resetDataButton != null)
            resetDataButton.onClick.AddListener(ResetAllData);
    }

    void TestSaveSystem()
    {
        if (SaveSystem.Instance != null)
        {
            Debug.Log("Save System is working!");
            var stats = SaveSystem.Instance.GetPlayerStats();
            Debug.Log($"Player has played {stats.totalGamesPlayed} games");
            var highScores = SaveSystem.Instance.GetHighScores();
            Debug.Log($"Found {highScores.Count} high scores");
            UpdateDisplay("Save System loaded successfully!\nReady to track scores and stats.");
        }
        else
        {
            Debug.LogError("Save System not found!");
            UpdateDisplay("Error: Save System not found!");
        }
    }

    public void SaveSampleHighScore()
    {
        if (SaveSystem.Instance == null) return;

        string playerName = playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text)
            ? playerNameInput.text
            : "TestPlayer";
        int score = sampleScore;
        if (scoreInput != null && int.TryParse(scoreInput.text, out int inputScore))
        {
            score = inputScore;
        }

        SaveSystem.Instance.AddHighScore(playerName, score, sampleWave, sampleKills, sampleTime);
        SaveSystem.Instance.IncrementStat("gamesPlayed");
        SaveSystem.Instance.IncrementStat("enemiesKilled", sampleKills);
        SaveSystem.Instance.UpdateHighestWave(sampleWave);
        SaveSystem.Instance.AddTimePlayed(sampleTime);
        UpdateDisplay($"High score saved!\nPlayer: {playerName}\nScore: {score:N0}");
        Debug.Log($"Saved high score: {playerName} - {score:N0}");
    }

    public void DisplayHighScores()
    {
        if (SaveSystem.Instance == null) return;

        var highScores = SaveSystem.Instance.GetHighScores();
        string display = "=== HIGH SCORES ===\n\n";
        if (highScores.Count == 0)
        {
            display += "No high scores yet!\nPlay the game to set some records.";
        }
        else
        {
            for (int i = 0; i < highScores.Count; i++)
            {
                var entry = highScores[i];
                display += $"{i + 1}. {entry.playerName}\n";
                display += $"   Score: {entry.score:N0}\n";
                display += $"   Wave: {entry.wave}\n";
                display += $"   Kills: {entry.enemiesKilled}\n";
                display += $"   Time: {FormatTime(entry.timePlayed)}\n\n";
            }
        }
        UpdateDisplay(display);
    }

    public void DisplayPlayerStats()
    {
        if (SaveSystem.Instance == null) return;

        var stats = SaveSystem.Instance.GetPlayerStats();
        string display = "=== PLAYER STATISTICS ===\n\n";
        display += $"Games Played: {stats.totalGamesPlayed:N0}\n";
        display += $"Total Score: {stats.totalScore:N0}\n";
        display += $"Total Time: {FormatTime(stats.totalTimePlayed)}\n";
        display += $"Enemies Killed: {stats.totalEnemiesKilled:N0}\n";
        display += $"Shots Fired: {stats.totalShotsFired:N0}\n";
        display += $"Accuracy: {stats.AccuracyPercentage:F1}%\n";
        display += $"Headshot %: {stats.HeadshotPercentage:F1}%\n";
        display += $"Best Wave: {stats.highestWaveReached}\n";
        display += $"Best Kill Streak: {stats.bestKillStreak}\n";
        display += $"Best Survival: {FormatTime(stats.bestSurvivalTime)}\n";
        display += $"Average Score: {stats.AverageScore:F0}\n";
        UpdateDisplay(display);
    }

    public void ResetAllData()
    {
        if (SaveSystem.Instance == null) return;

        SaveSystem.Instance.ResetAllData();
        UpdateDisplay("All save data has been reset!");
        Debug.Log("All save data reset");
    }

    void UpdateDisplay(string text)
    {
        if (displayText != null)
        {
            displayText.text = text;
        }
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

    public void OnPlayerShoot()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.IncrementStat("shotsFired");
        }
    }

    public void OnShotHit(bool isHeadshot = false)
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.IncrementStat("hits");
            if (isHeadshot)
            {
                SaveSystem.Instance.IncrementStat("headshots");
            }
        }
    }

    public void OnEnemyKilled()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.IncrementStat("enemiesKilled");
        }
    }

    public void OnGameEnd(int finalScore, int waveReached, int enemiesKilled, float timePlayed)
    {
        if (SaveSystem.Instance == null) return;

        SaveSystem.Instance.IncrementStat("gamesPlayed");
        SaveSystem.Instance.UpdateHighestWave(waveReached);
        SaveSystem.Instance.AddTimePlayed(timePlayed);
        SaveSystem.Instance.UpdateBestSurvivalTime(timePlayed);

        if (SaveSystem.Instance.IsHighScore(finalScore))
        {
            string playerName = SaveSystem.Instance.GetPlayerName();
            SaveSystem.Instance.AddHighScore(playerName, finalScore, waveReached, enemiesKilled, timePlayed);
            Debug.Log("NEW HIGH SCORE!");
            UpdateDisplay($"NEW HIGH SCORE!\nScore: {finalScore:N0}\nRank: #{SaveSystem.Instance.GetHighScoreRank(finalScore)}");
        }
    }
}
