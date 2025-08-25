using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float timeBetweenWaves = 10f;
    public int startingWave = 1;
    [Header("UI References")]
    public Text waveText;
    public Text scoreText;
    public Text enemiesLeftText;
    public Text gameOverText;
    public Button restartButton;
    public GameObject gameOverPanel;
    public GameObject hudPanel;
    public GameObject helpPanel;
    public GameObject pausedPanel;
    private bool helpPanelActive = false;
    [Header("References")]
    public EnemySpawner enemySpawner;
    public PlayerHealth playerHealth;
    public PlayerInput playerInput;
    public HitFeedbackManager hitFeedback;
    private int currentWave = 1;
    private int score = 0;
    private int enemiesKilled = 0;
    private bool gameActive = true;
    private bool waveInProgress = false;
    private float gameStartTime;
    private const int ENEMY_KILL_SCORE = 100;
    private const int WAVE_BONUS_SCORE = 500;
    void Start()
    {
        currentWave = startingWave;
        score = 0;
        gameActive = true;
        gameStartTime = Time.time;
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (hitFeedback == null)
            hitFeedback = FindFirstObjectByType<HitFeedbackManager>();
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
        UpdateUI();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        SetGameplayCursor();
        StartCoroutine(StartWaveAfterDelay(3f));
        helpPanel.SetActive(false);
    }
    void Update()
    {
        if (!gameActive) return;
        if (waveInProgress && enemySpawner != null && enemySpawner.IsWaveComplete())
        {
            WaveCompleted();
        }
        UpdateUI();
    }
    IEnumerator StartWaveAfterDelay(float delay)
    {
        if (waveText != null)
            waveText.text = $"Wave {currentWave} starting in {delay:F0}...";
        float countdown = delay;
        while (countdown > 0)
        {
            if (waveText != null)
                waveText.text = $"Wave {currentWave} starting in {countdown:F0}...";
            countdown -= Time.deltaTime;
            yield return null;
        }
        StartWave();
    }
    public void StartWave()
    {
        if (!gameActive || enemySpawner == null) return;
        waveInProgress = true;
        enemySpawner.StartWave(currentWave);
        if (waveText != null)
            waveText.text = $"Wave {currentWave}";
        Debug.Log($"Wave {currentWave} started!");
    }

    public void Quit()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void WaveCompleted()
    {
        if (!waveInProgress) return;

        waveInProgress = false;

        AddScore(WAVE_BONUS_SCORE);

        Debug.Log($"Wave {currentWave} completed!");

        currentWave++;
        StartCoroutine(StartWaveAfterDelay(timeBetweenWaves));
    }
    public void EnemyKilled()
    {
        enemiesKilled++;
        AddScore(ENEMY_KILL_SCORE);
        Debug.Log($"Enemy killed! Total: {enemiesKilled}");
    }
    public void PlayerDied()
    {
        gameActive = false;
        Debug.Log("Game Over!");
        float sessionTime = Time.time - gameStartTime;
        Debug.Log($"Session lasted {sessionTime:F1} seconds");
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (hudPanel != null)
            hudPanel.SetActive(false);
        if (gameOverText != null)
            gameOverText.text = $"Game Over! \n Wave Reached: {currentWave}\n Score: {score} \n Enemies Killed: {enemiesKilled}";
        Time.timeScale = 0f;

        SetUICursor();
    }
    public void RestartGame()
    {
        SetGameplayCursor();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToggleHelp()
    {
        helpPanelActive = !helpPanelActive;
        if (helpPanel != null)
        {
            helpPanel.SetActive(helpPanelActive);
            pausedPanel.SetActive(!helpPanelActive);
        }
    }
    void AddScore(int points)
    {
        score += points;
    }
    public void OnEnemyHit(Vector3 hitPosition, int damage, bool isKill)
    {
        int scoreValue = damage * 2;
        if (isKill) scoreValue += 100;
        AddScore(scoreValue);
        if (hitFeedback != null)
        {
            hitFeedback.ShowHitFeedback(hitPosition, damage, isKill);
        }
    }
    void UpdateUI()
    {
        if (waveText != null && waveInProgress)
            waveText.text = $"Wave {currentWave}";
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
        if (enemiesLeftText != null && enemySpawner != null)
        {
            int enemiesLeft = enemySpawner.GetAliveEnemyCount();
            enemiesLeftText.text = $"Enemies: {enemiesLeft}";
        }
    }
    public int GetCurrentWave() { return currentWave; }
    public int GetScore() { return score; }
    public int GetEnemiesKilled() { return enemiesKilled; }
    public bool IsGameActive() { return gameActive; }
    public void SetGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Gameplay");
        }
    }
    public void SetUICursor()
    {
        Debug.Log("Switching to UI cursor");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerInput != null)
        {
            playerInput.actions.FindActionMap("UI").Enable();
            var gameplay = playerInput.actions.FindActionMap("Gameplay");
            gameplay.Enable();
            gameplay.FindAction("Move")?.Disable();
            gameplay.FindAction("Look")?.Disable();
            gameplay.FindAction("Fire")?.Disable();
            gameplay.FindAction("Jump")?.Disable();
        }
    }
}
