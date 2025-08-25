using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    public PlayerStats playerStats = new PlayerStats();
    public GameSettings gameSettings = new GameSettings();
    public List<string> unlockedAchievements = new List<string>();
    public SaveData()
    {
        playerStats = new PlayerStats();
        gameSettings = new GameSettings();
        highScores = new List<HighScoreEntry>();
        unlockedAchievements = new List<string>();
    }
}

[System.Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;
    public int wave;
    public int enemiesKilled;
    public DateTime dateAchieved;
    public float timePlayed;
    public HighScoreEntry(string name, int score, int wave, int enemies, float time)
    {
        this.playerName = name;
        this.score = score;
        this.wave = wave;
        this.enemiesKilled = enemies;
        this.timePlayed = time;
        this.dateAchieved = DateTime.Now;
    }
}

[System.Serializable]
public class PlayerStats
{

    public int totalEnemiesKilled = 0;
    public int totalShotsFired = 0;
    public int totalHits = 0;
    public int totalHeadshots = 0;
    public int totalGamesPlayed = 0;
    public int highestWaveReached = 0;
    public int totalScore = 0;

    public float totalTimePlayed = 0f;
    public float bestSurvivalTime = 0f;

    public int killStreak = 0;
    public int bestKillStreak = 0;
    public int consecutive100PlusScoreHits = 0;
    public int perfectWaves = 0;
    public float AccuracyPercentage => totalShotsFired > 0 ? (float)totalHits / totalShotsFired * 100f : 0f;
    public float HeadshotPercentage => totalHits > 0 ? (float)totalHeadshots / totalHits * 100f : 0f;
    public float AverageScore => totalGamesPlayed > 0 ? (float)totalScore / totalGamesPlayed : 0f;
}

[System.Serializable]
public class GameSettings
{

    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public int qualityLevel = 2;
    public bool fullscreen = true;
    public int targetFrameRate = 60;

    public float mouseSensitivity = 1f;
    public bool invertMouseY = false;
    public string lastPlayerName = "Player";
    public float difficultyMultiplier = 1f;
}
