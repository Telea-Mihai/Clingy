using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 20f;
    public float speed = 20f;
    public float lifetime = 5f;
    public GameObject impactEffect;
    [Header("Fast Movement Detection")]
    public float spherecastRadius = 0.1f;
    public LayerMask collisionLayers = -1;
    private Vector3 lastPosition;
    private Rigidbody rb;

    void Start()
    {
        Destroy(gameObject, lifetime);
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
        }
        lastPosition = transform.position;
    }
    void FixedUpdate()
    {
        CheckSphereCastCollision();
        lastPosition = transform.position;
    }
    void CheckSphereCastCollision()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;
        if (distance > 0.01f)
        {
            RaycastHit hit;
            if (Physics.SphereCast(lastPosition, spherecastRadius, direction.normalized, out hit, distance, collisionLayers))
            {
                HandleCollision(hit.collider.gameObject, hit.point);
            }
        }
    }
    void HandleCollision(GameObject hitObject, Vector3 hitPoint)
    {
        PlayerHealth playerHealth = GetPlayerHealthFromHit(hitObject);
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        if (impactEffect != null)
        {
            GameObject impact = Instantiate(impactEffect, hitPoint, Quaternion.identity);
            Destroy(impact, 2f);
        }
        Destroy(gameObject);
    }
    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject, collision.contacts[0].point);
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
}
