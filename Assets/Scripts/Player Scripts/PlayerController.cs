using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    
    [SerializeField] GameObject shockwavePrefab;
    [SerializeField] GameObject blackHolePrefab;
    

    public bool canBecomeBlackHole;



    void Awake()
    {
        Instance = this;

        GetComponent<SpriteRenderer>().enabled = true;

        foreach (Transform child in transform)
        {
            SpriteRenderer sprite = child.gameObject.GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.enabled = true;
        }

        gameObject.SetActive(false);
    }

    // void Start() => StartCoroutine(Die());


    public IEnumerator Die()
    {
        GetComponent<FireMissile>().StopFiring();
        Debug.Break();

        foreach (Transform child in transform)
            child.gameObject.SetActive(false);

        
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        // yield return new WaitForSeconds(3f);

        // handle particles.
        Quaternion flippedRotation = Quaternion.Euler(0f, 0f, 90f);

        ParticleSystem shockWave1 = Instantiate(shockwavePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ParticleSystem shockWave2 = Instantiate(shockwavePrefab, transform.position, flippedRotation).GetComponent<ParticleSystem>();
        shockWave1.Play();
        shockWave2.Play();

        yield return new WaitForSeconds(shockWave1.main.duration);

        // yield return new WaitForSeconds(3f);
        // Debug.Log($"Die() waited for shockwave for {shockWave1.main.duration}");


        if (canBecomeBlackHole)
        {
            GameManager.Instance.blackHoleAteAllEnemies = false;
            GameObject blackHole = Instantiate(blackHolePrefab, transform.position, Quaternion.identity);

            ParticleSystem blackHoleParticle = blackHolePrefab.GetComponent<ParticleSystem>();
            blackHoleParticle.Play();
            
            // wait a bit while particle plays... 
            yield return new WaitForSeconds(blackHoleParticle.main.duration);

            // .. before pulling things in
            blackHole.transform.GetChild(3).gameObject.SetActive(true);

            // If a black hole is spawned, Singularity.cs will determine when this flag occurs.
            yield return new WaitUntil(() => GameManager.Instance.blackHoleAteAllEnemies == true);

            Debug.Log("black hole ended");
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("player fully died.");
        yield break;
    }

    // void Start() =>
        // Invoke("TestMultiplier", 3f);


    // void TestMultiplier() =>
        // FireMissile.Instance.fireRateMultiplier = 10;
}
