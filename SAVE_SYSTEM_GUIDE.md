# Save System Integration Guide

## Overview
This save system provides comprehensive high score tracking, player statistics, and achievement management for your Unity first-person shooter game.

## Files Created

### Core System Files:
1. **SaveData.cs** - Data structures for all save information
2. **SaveSystem.cs** - Main save/load functionality using JSON
3. **GameStatsManager.cs** - Session tracking and statistics management
4. **HighScoreUI.cs** - UI components for displaying high scores and stats
5. **SaveSystemExample.cs** - Example usage and testing script

## Features

### High Score System
- Top 10 high scores with player name, score, wave, kills, and time
- Automatic rank calculation
- Date tracking for each score
- Persistent storage between game sessions

### Player Statistics
- Total games played
- Total score across all games
- Total enemies killed
- Shot accuracy tracking (shots fired vs hits)
- Headshot percentage
- Best wave reached
- Best survival time
- Best kill streak
- Perfect waves (no damage taken)

### Achievement System
- Unlockable achievements for various milestones
- Kill streak achievements (5, 10, 25, 50 kills)
- Score achievements (10K, 100K points)
- Accuracy achievements (90%, 95%)
- Perfect wave achievements

### Game Settings
- Audio volume settings
- Graphics preferences
- Mouse sensitivity
- Last used player name

## How to Integrate

### Step 1: Setup Save System
1. Add a SaveSystem prefab to your scene or create an empty GameObject
2. Attach the SaveSystem script to it
3. The system will automatically initialize and create save files

### Step 2: Basic Integration
Add these calls to your existing scripts:

#### In GunController.cs (Fire method):
```csharp
private void Fire()
{
    // Existing fire code...
    
    // Track shot fired
    if (GameStatsManager.Instance != null)
        GameStatsManager.Instance.OnShotFired();
    
    if (TryGetTarget(out RaycastHit hit, out Enemy enemy))
    {
        // Track hit
        if (GameStatsManager.Instance != null)
            GameStatsManager.Instance.OnHit(isHeadshot: false); // Set true for headshots
        
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            
            // Track kill if enemy dies
            if (enemy.health <= 0 && GameStatsManager.Instance != null)
                GameStatsManager.Instance.OnEnemyKilled(100); // Pass score awarded
        }
    }
}
```

#### In PlayerHealth.cs (TakeDamage method):
```csharp
public void TakeDamage(float damage)
{
    // Existing damage code...
    
    // Track damage taken
    if (GameStatsManager.Instance != null)
        GameStatsManager.Instance.OnPlayerTookDamage();
}
```

#### In GameManager.cs (PlayerDied method):
```csharp
public void PlayerDied()
{
    gameActive = false;
    
    // End stats session and check for high scores
    if (GameStatsManager.Instance != null)
        GameStatsManager.Instance.OnGameEnd();
    
    // Existing game over code...
}
```

#### In GameManager.cs (WaveCompleted method):
```csharp
public void WaveCompleted()
{
    // Existing wave completion code...
    
    // Track wave completion
    if (GameStatsManager.Instance != null)
        GameStatsManager.Instance.OnWaveCompleted();
}
```

### Step 3: UI Integration
1. Create UI panels for high scores and statistics
2. Add the HighScoreUI script to manage these panels
3. Create prefabs for high score entries
4. Connect UI buttons to the HighScoreUI methods

### Step 4: Testing
1. Add the SaveSystemExample script to a GameObject
2. Create UI buttons linked to the example methods
3. Test saving/loading high scores and viewing statistics

## Save File Location
- Windows: `%USERPROFILE%/AppData/LocalLow/[CompanyName]/[ProductName]/gamedata.json`
- Mac: `~/Library/Application Support/[CompanyName]/[ProductName]/gamedata.json`
- Linux: `~/.config/unity3d/[CompanyName]/[ProductName]/gamedata.json`

## Example Usage

### Saving a High Score:
```csharp
SaveSystem.Instance.AddHighScore("PlayerName", 5000, 10, 50, 300f);
```

### Checking Statistics:
```csharp
PlayerStats stats = SaveSystem.Instance.GetPlayerStats();
Debug.Log($"Accuracy: {stats.AccuracyPercentage:F1}%");
```

### Unlocking Achievements:
```csharp
SaveSystem.Instance.UnlockAchievement("KILLSTREAK_10");
```

## Customization

### Adding New Statistics:
1. Add new fields to the PlayerStats class in SaveData.cs
2. Create methods in SaveSystem.cs to update these stats
3. Add tracking calls in your game scripts

### Adding New Achievements:
1. Define achievement IDs as constants
2. Add check conditions in GameStatsManager.cs
3. Call UnlockAchievement when conditions are met

### Modifying Save Data:
- Edit the SaveData.cs file to add new data structures
- SaveSystem.cs will automatically handle serialization
- Existing save files will be compatible (new fields get default values)

## Performance Notes
- Auto-save runs every 60 seconds by default
- Manual saves are recommended at game over or major milestones
- JSON serialization is lightweight for small to medium datasets
- Save operations are performed on the main thread (consider async for large datasets)

## Troubleshooting

### Common Issues:
1. **Save file not found**: Normal on first run, system creates new file
2. **Permissions error**: Check write permissions to Application.persistentDataPath
3. **Compilation errors**: Ensure all scripts are in correct folders and referenced properly

### Debug Tips:
- Check Console for save/load messages
- Use SaveSystemExample.cs to test functionality
- Verify save file exists at the expected location
- Use SaveSystem.Instance.SaveExists() to check for existing saves
