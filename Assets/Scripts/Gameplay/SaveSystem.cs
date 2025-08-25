using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    public static SaveSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SaveSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveSystem");
                    instance = go.AddComponent<SaveSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Save Settings")]
    public string saveFileName = "gamedata.json";
    public bool autoSave = true;
    public float autoSaveInterval = 60f;
    private SaveData currentSaveData;
    private string savePath;
    private float lastAutoSaveTime;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadGame();
    }

    void Update()
    {
        if (autoSave && Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            SaveGame();
            lastAutoSaveTime = Time.time;
        }
    }

    void Initialize()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        currentSaveData = new SaveData();
        Debug.Log($"Save system initialized. Save path: {savePath}");
    }

    public void SaveGame()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(savePath, jsonData);
            Debug.Log("Game saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (File.Exists(savePath))
            {
                string jsonData = File.ReadAllText(savePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);
                Debug.Log("Game loaded successfully!");
            }
            else
            {
                currentSaveData = new SaveData();
                Debug.Log("No save file found, creating new save data.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            currentSaveData = new SaveData();
        }
    }

    public void DeleteSave()
    {
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                currentSaveData = new SaveData();
                Debug.Log("Save file deleted successfully!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }

    public bool SaveExists()
    {
        return File.Exists(savePath);
    }

    public void AddHighScore(string playerName, int score, int wave, int enemiesKilled, float timePlayed)
    {
        HighScoreEntry newEntry = new HighScoreEntry(playerName, score, wave, enemiesKilled, timePlayed);
        currentSaveData.highScores.Add(newEntry);
        currentSaveData.highScores.Sort((a, b) => b.score.CompareTo(a.score));
        if (currentSaveData.highScores.Count > 10)
        {
            currentSaveData.highScores.RemoveRange(10, currentSaveData.highScores.Count - 10);
        }
        SaveGame();
    }

    public List<HighScoreEntry> GetHighScores()
    {
        return new List<HighScoreEntry>(currentSaveData.highScores);
    }

    public bool IsHighScore(int score)
    {
        if (currentSaveData.highScores.Count < 10) return true;
        return score > currentSaveData.highScores[currentSaveData.highScores.Count - 1].score;
    }

    public int GetHighScoreRank(int score)
    {
        for (int i = 0; i < currentSaveData.highScores.Count; i++)
        {
            if (score > currentSaveData.highScores[i].score)
            {
                return i + 1;
            }
        }
        return currentSaveData.highScores.Count + 1;
    }

    public PlayerStats GetPlayerStats()
    {
        return currentSaveData.playerStats;
    }

    public void UpdatePlayerStats(PlayerStats newStats)
    {
        currentSaveData.playerStats = newStats;
        SaveGame();
    }

    public void IncrementStat(string statName, int amount = 1)
    {
        switch (statName.ToLower())
        {
            case "enemiesKilled":
                currentSaveData.playerStats.totalEnemiesKilled += amount;
                break;
            case "shotsFired":
                currentSaveData.playerStats.totalShotsFired += amount;
                break;
            case "hits":
                currentSaveData.playerStats.totalHits += amount;
                break;
            case "headshots":
                currentSaveData.playerStats.totalHeadshots += amount;
                break;
            case "gamesPlayed":
                currentSaveData.playerStats.totalGamesPlayed += amount;
                break;
        }
    }

    public void UpdateHighestWave(int wave)
    {
        if (wave > currentSaveData.playerStats.highestWaveReached)
        {
            currentSaveData.playerStats.highestWaveReached = wave;
        }
    }

    public void AddTimePlayed(float time)
    {
        currentSaveData.playerStats.totalTimePlayed += time;
    }

    public void UpdateBestSurvivalTime(float time)
    {
        if (time > currentSaveData.playerStats.bestSurvivalTime)
        {
            currentSaveData.playerStats.bestSurvivalTime = time;
        }
    }

    public GameSettings GetGameSettings()
    {
        return currentSaveData.gameSettings;
    }

    public void UpdateGameSettings(GameSettings newSettings)
    {
        currentSaveData.gameSettings = newSettings;
        SaveGame();
    }

    public void SetPlayerName(string name)
    {
        currentSaveData.gameSettings.lastPlayerName = name;
        SaveGame();
    }

    public string GetPlayerName()
    {
        return currentSaveData.gameSettings.lastPlayerName;
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!currentSaveData.unlockedAchievements.Contains(achievementId))
        {
            currentSaveData.unlockedAchievements.Add(achievementId);
            SaveGame();
            Debug.Log($"Achievement unlocked: {achievementId}");
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return currentSaveData.unlockedAchievements.Contains(achievementId);
    }

    public List<string> GetUnlockedAchievements()
    {
        return new List<string>(currentSaveData.unlockedAchievements);
    }

    public void ResetAllData()
    {
        currentSaveData = new SaveData();
        SaveGame();
        Debug.Log("All save data has been reset!");
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
