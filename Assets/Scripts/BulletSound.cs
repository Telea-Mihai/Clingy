using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSound : MonoBehaviour
{

    public AudioSource source;

    private void Start()
    {
        source.pitch = Random.Range(1, 2);
        source.PlayOneShot(source.clip);
    }
}
