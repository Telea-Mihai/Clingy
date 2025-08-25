using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnRadius = 2f;
    public LayerMask groundLayer;
    [Header("Wave Settings")]
    public int enemiesPerWave = 5;
    public float timeBetweenSpawns = 1f;
    public float timeBetweenWaves = 5f;
    public int maxEnemiesAlive = 10;
    private List<GameObject> aliveEnemies = new List<GameObject>();
    private GameManager gameManager;
    private bool isSpawning = false;
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (spawnPoints.Length == 0)
        {
            spawnPoints = new Transform[] { transform };
        }
    }
    public void StartWave(int waveNumber)
    {
        if (isSpawning) return;
        int enemiesToSpawn = enemiesPerWave + (waveNumber - 1) * 2;
        StartCoroutine(SpawnWave(enemiesToSpawn));
    }
    IEnumerator SpawnWave(int enemyCount)
    {
        isSpawning = true;
        for (int i = 0; i < enemyCount; i++)
        {
            if (aliveEnemies.Count >= maxEnemiesAlive)
            {
                yield return new WaitForSeconds(0.5f);
                CleanUpDeadEnemies();
                i--;
                continue;
            }
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        isSpawning = false;
    }
    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = FindValidSpawnPosition(spawnPoint.position);
        if (spawnPosition != Vector3.zero)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            aliveEnemies.Add(enemy);
            SpawnEffect(spawnPosition);
        }
    }
    Vector3 FindValidSpawnPosition(Vector3 centerPosition)
    {
        for (int attempts = 0; attempts < 10; attempts++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 testPosition = centerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            RaycastHit hit;
            if (Physics.Raycast(testPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
            {
                Collider[] nearbyColliders = Physics.OverlapSphere(hit.point, 1.5f);
                bool positionClear = true;
                foreach (Collider col in nearbyColliders)
                {
                    if (col.CompareTag("Enemy"))
                    {
                        positionClear = false;
                        break;
                    }
                }
                if (positionClear)
                {
                    return hit.point;
                }
            }
        }
        return centerPosition;
    }
    void SpawnEffect(Vector3 position)
    {
        Debug.Log($"Enemy spawned at {position}");
    }
    public void EnemyDied(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Remove(enemy);
        }
        if (!isSpawning && aliveEnemies.Count == 0)
        {
            if (gameManager != null)
            {
                gameManager.WaveCompleted();
            }
        }
    }
    void CleanUpDeadEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
    public int GetAliveEnemyCount()
    {
        CleanUpDeadEnemies();
        return aliveEnemies.Count;
    }
    public bool IsWaveComplete()
    {
        return !isSpawning && aliveEnemies.Count == 0;
    }
    void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                }
            }
        }
    }
}
