using UnityEngine;
using System.Collections;

public class GameStatsManager : MonoBehaviour
{
    private static GameStatsManager instance;
    public static GameStatsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameStatsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameStatsManager");
                    instance = go.AddComponent<GameStatsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Current Session Stats")]
    public int sessionShotsFired = 0;
    public int sessionHits = 0;
    public int sessionHeadshots = 0;
    public int sessionEnemiesKilled = 0;
    public int sessionScore = 0;
    public float sessionStartTime;
    public int currentKillStreak = 0;
    public int sessionBestKillStreak = 0;
    public bool perfectWave = true;

    [Header("References")]
    public GameManager gameManager;

    private PlayerStats playerStats;
    private bool sessionActive = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (SaveSystem.Instance != null)
        {
            playerStats = SaveSystem.Instance.GetPlayerStats();
        }

        StartNewSession();
    }

    public void StartNewSession()
    {
        sessionStartTime = Time.time;
        sessionShotsFired = 0;
        sessionHits = 0;
        sessionHeadshots = 0;
        sessionEnemiesKilled = 0;
        sessionScore = 0;
        currentKillStreak = 0;
        sessionBestKillStreak = 0;
        perfectWave = true;
        sessionActive = true;

        Debug.Log("New game session started!");
    }

    public void EndSession()
    {
        if (!sessionActive) return;

        sessionActive = false;
        float sessionTime = Time.time - sessionStartTime;

        playerStats.totalShotsFired += sessionShotsFired;
        playerStats.totalHits += sessionHits;
        playerStats.totalHeadshots += sessionHeadshots;
        playerStats.totalEnemiesKilled += sessionEnemiesKilled;
        playerStats.totalScore += sessionScore;
        playerStats.totalTimePlayed += sessionTime;
        playerStats.totalGamesPlayed++;

        if (currentKillStreak > playerStats.bestKillStreak)
            playerStats.bestKillStreak = currentKillStreak;

        if (sessionTime > playerStats.bestSurvivalTime)
            playerStats.bestSurvivalTime = sessionTime;

        if (gameManager != null)
        {
            int currentWave = gameManager.GetCurrentWave();
            if (currentWave > playerStats.highestWaveReached)
                playerStats.highestWaveReached = currentWave;
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.UpdatePlayerStats(playerStats);
        }

        Debug.Log($"Session ended. Time played: {sessionTime:F1}s, Score: {sessionScore}");
    }

    public void OnShotFired()
    {
        if (!sessionActive) return;
        sessionShotsFired++;
    }

    public void OnHit(bool isHeadshot = false)
    {
        if (!sessionActive) return;
        sessionHits++;
        if (isHeadshot)
            sessionHeadshots++;
    }

    public void OnEnemyKilled(int scoreAwarded)
    {
        if (!sessionActive) return;
        sessionEnemiesKilled++;
        sessionScore += scoreAwarded;
        currentKillStreak++;
        if (currentKillStreak > sessionBestKillStreak)
            sessionBestKillStreak = currentKillStreak;

        CheckKillStreakAchievements();
    }

    public void OnPlayerTookDamage()
    {
        if (!sessionActive) return;
        currentKillStreak = 0;
        perfectWave = false;
    }

    public void OnWaveCompleted()
    {
        if (!sessionActive) return;
        if (perfectWave)
        {
            playerStats.perfectWaves++;
            CheckPerfectWaveAchievements();
        }
        perfectWave = true;
    }

    public void AddScore(int points)
    {
        if (!sessionActive) return;
        sessionScore += points;
    }

    private void CheckKillStreakAchievements()
    {
        if (SaveSystem.Instance == null) return;

        switch (currentKillStreak)
        {
            case 5:
                SaveSystem.Instance.UnlockAchievement("KILLSTREAK_5");
                break;
            case 10:
                SaveSystem.Instance.UnlockAchievement("KILLSTREAK_10");
                break;
            case 25:
                SaveSystem.Instance.UnlockAchievement("KILLSTREAK_25");
                break;
            case 50:
                SaveSystem.Instance.UnlockAchievement("KILLSTREAK_50");
                break;
        }

        if (playerStats.totalEnemiesKilled + sessionEnemiesKilled >= 100 && !SaveSystem.Instance.IsAchievementUnlocked("KILLS_100"))
            SaveSystem.Instance.UnlockAchievement("KILLS_100");
        if (playerStats.totalEnemiesKilled + sessionEnemiesKilled >= 1000 && !SaveSystem.Instance.IsAchievementUnlocked("KILLS_1000"))
            SaveSystem.Instance.UnlockAchievement("KILLS_1000");
    }

    private void CheckPerfectWaveAchievements()
    {
        if (SaveSystem.Instance == null) return;

        if (playerStats.perfectWaves >= 5)
            SaveSystem.Instance.UnlockAchievement("PERFECT_WAVES_5");
        if (playerStats.perfectWaves >= 10)
            SaveSystem.Instance.UnlockAchievement("PERFECT_WAVES_10");
    }

    public void CheckScoreAchievements()
    {
        if (SaveSystem.Instance == null) return;

        int totalScore = playerStats.totalScore + sessionScore;
        if (totalScore >= 10000 && !SaveSystem.Instance.IsAchievementUnlocked("SCORE_10K"))
            SaveSystem.Instance.UnlockAchievement("SCORE_10K");
        if (totalScore >= 100000 && !SaveSystem.Instance.IsAchievementUnlocked("SCORE_100K"))
            SaveSystem.Instance.UnlockAchievement("SCORE_100K");
    }

    public void CheckAccuracyAchievements()
    {
        if (SaveSystem.Instance == null) return;

        float currentAccuracy = sessionShotsFired > 0 ? (float)sessionHits / sessionShotsFired * 100f : 0f;
        if (currentAccuracy >= 90f && sessionShotsFired >= 50)
            SaveSystem.Instance.UnlockAchievement("ACCURACY_90");
        if (currentAccuracy >= 95f && sessionShotsFired >= 100)
            SaveSystem.Instance.UnlockAchievement("ACCURACY_95");
    }

    public float GetSessionAccuracy()
    {
        return sessionShotsFired > 0 ? (float)sessionHits / sessionShotsFired * 100f : 0f;
    }

    public float GetLifetimeAccuracy()
    {
        return playerStats.AccuracyPercentage;
    }

    public float GetSessionTime()
    {
        return sessionActive ? Time.time - sessionStartTime : 0f;
    }

    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

    public void OnGameEnd()
    {
        CheckScoreAchievements();
        CheckAccuracyAchievements();
        if (SaveSystem.Instance != null && gameManager != null)
        {
            float sessionTime = Time.time - sessionStartTime;
            string playerName = SaveSystem.Instance.GetPlayerName();
            if (SaveSystem.Instance.IsHighScore(sessionScore))
            {
                SaveSystem.Instance.AddHighScore(
                    playerName,
                    sessionScore,
                    gameManager.GetCurrentWave(),
                    sessionEnemiesKilled,
                    sessionTime
                );
                Debug.Log("New high score achieved!");
            }
        }

        EndSession();
    }

    void OnApplicationQuit()
    {
        if (sessionActive)
            EndSession();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && sessionActive)
            EndSession();
    }
}
