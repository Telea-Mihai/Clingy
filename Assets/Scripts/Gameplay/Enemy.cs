using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float health = 100f;
    public float maxHealth = 100f;
    public float damage = 20f;
    public float attackRange = 15f;
    public float shootCooldown = 2f;
    public float moveSpeed = 3.5f;
    [Header("Shooting Prediction")]
    public float predictionStrength = 0.5f;
    public float bulletSpeed = 20f;
    [Header("References")]
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public GameObject deathEffect;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip deathSound;
    public AudioClip hitSound;
    private Transform player;
    private Rigidbody playerRigidbody;
    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;
    private NavMeshAgent agent;
    private Animator animator;
    private bool canShoot = true;
    private bool isDead = false;
    private GameManager gameManager;
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
            lastPlayerPosition = player.position;
        }
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
        }
        health = maxHealth;
    }
    void Update()
    {
        if (isDead || player == null) return;
        if (playerRigidbody != null)
        {
            playerVelocity = playerRigidbody.linearVelocity;
        }
        else
        {
            playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
            lastPlayerPosition = player.position;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (agent != null && distanceToPlayer > attackRange)
        {
            agent.SetDestination(player.position);
            if (animator != null)
                animator.SetBool("isWalking", true);
        }
        else
        {
            if (agent != null)
                agent.SetDestination(transform.position);
            if (animator != null)
                animator.SetBool("isWalking", false);
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);
            if (distanceToPlayer <= attackRange && canShoot)
            {
                StartCoroutine(ShootAtPlayer());
            }
        }
    }
    IEnumerator ShootAtPlayer()
    {
        canShoot = false;
        if (animator != null)
            animator.SetTrigger("Shoot");
        if (audioSource != null && shootSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shootSound);
        }
        if (bulletPrefab != null && shootPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
            Vector3 targetPosition = GetPredictedPlayerPosition();
            Vector3 shootDirection = (targetPosition - shootPoint.position).normalized;
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = shootDirection * bulletSpeed;
                bulletRb.useGravity = true;
            }
            if (enemyBullet != null)
            {
                enemyBullet.damage = damage;
                enemyBullet.speed = bulletSpeed;
            }
        }
        else
        {
            RaycastHit hit;
            Vector3 targetPosition = GetPredictedPlayerPosition();
            Vector3 shootDirection = (targetPosition - shootPoint.position).normalized;
            if (Physics.Raycast(shootPoint.position, shootDirection, out hit, attackRange))
            {
                PlayerHealth playerHealth = GetPlayerHealthFromHit(hit.collider.gameObject);
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        if (gameManager != null)
        {
            bool willDie = health <= 0;
            gameManager.OnEnemyHit(transform.position, (int)damageAmount, willDie);
        }
        StartCoroutine(FlashRed());
        if(audioSource != null && hitSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.1f);
            audioSource.PlayOneShot(hitSound);
        }
        if (health <= 0)
        {
            Die();
        }
    }
    IEnumerator FlashRed()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }
    void Die()
    {
        isDead = true;
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        if (agent != null)
            agent.enabled = false;
        if (animator != null)
            animator.SetTrigger("Die");
        if (gameManager != null)
        {
            gameManager.EnemyKilled();
        }
        Destroy(gameObject);
    }
    Vector3 GetPredictedPlayerPosition()
    {
        if (player == null) return Vector3.zero;
        float distanceToPlayer = Vector3.Distance(shootPoint.position, player.position);
        float timeToReach = distanceToPlayer / bulletSpeed;
        Vector3 predictedPosition = player.position + (playerVelocity * timeToReach * predictionStrength);
        return predictedPosition;
    }
    PlayerHealth GetPlayerHealthFromHit(GameObject hitObject)
    {
        PlayerHealth playerHealth = hitObject.GetComponent<PlayerHealth>();
        if (playerHealth != null) return playerHealth;
        if (hitObject.CompareTag("Player"))
        {
            playerHealth = hitObject.GetComponent<PlayerHealth>();
            if (playerHealth != null) return playerHealth;
        }
        Transform current = hitObject.transform;
        while (current != null)
        {
            playerHealth = current.GetComponent<PlayerHealth>();
            if (playerHealth != null) return playerHealth;
            if (current.CompareTag("Player"))
            {
                playerHealth = current.GetComponent<PlayerHealth>();
                if (playerHealth != null) return playerHealth;
            }
            current = current.parent;
        }
        return null;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
