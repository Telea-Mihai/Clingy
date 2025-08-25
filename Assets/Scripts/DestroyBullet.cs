using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBullet : MonoBehaviour
{
    public int secondstilldestruct;
    public int bouncestilldestroyed;
    public float maxAngle = 95;
    private Rigidbody rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        StartCoroutine(AutoDestruct());
    }

    IEnumerator AutoDestruct()
    {
        yield return new WaitForSeconds(secondstilldestruct);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;
        Vector3 vel = rigidbody.linearVelocity;
        if (Vector3.Angle(vel, -normal) > maxAngle)
        {
            rigidbody.linearVelocity = Vector3.Reflect(vel, normal);
            print("Richoche");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
