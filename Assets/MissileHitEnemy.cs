using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileHitEnemy : MonoBehaviour
{
    
    ParticleSystem explosionParticle;

    void Awake()
    {
        explosionParticle = GetComponentInChildren<ParticleSystem>();
        Invoke("DisableAfterSeconds", 3f);
    }

    void OnTriggerEnter2D(Collider2D enemy) {
        if (enemy.gameObject.tag == "Enemy")
        {
            Debug.Log($"{gameObject} hit enemy");

            enemy.gameObject.SetActive(false);
            GetComponent<MeshRenderer>().enabled = false;

            StartCoroutine("DisableAfterParticleEnds");
        }
    }

    IEnumerator DisableAfterParticleEnds()
    {
        explosionParticle.Play();
        yield return new WaitUntil(() => !explosionParticle.isPlaying);
        gameObject.SetActive(false);
    }


    // if hit nothing.
    void DisableAfterSeconds() =>
        gameObject.SetActive(false);
}
