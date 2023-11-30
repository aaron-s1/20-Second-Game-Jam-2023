using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMissile : MonoBehaviour
{
    public static FireMissile Instance { get; private set; }
    
    [SerializeField] GameObject missilePrefab;
    [SerializeField] float timeBetweenMissileFirings = 1f;
    [SerializeField] int poolSize = 300;
    [Space(10)]
    [SerializeField] Transform leftMissileOriginPoint;
    [SerializeField] Transform rightMissileOriginPoint;

    List<GameObject> missilePool;



    private int _missileFireMultiplier = 1;
    private int _explosionChains = 0;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        missilePool = new List<GameObject>();
        InitializeMissilePool();

        InvokeRepeating("PlayerStartsFiring", 2f, timeBetweenMissileFirings / fireRateMultiplier);
        // Invoke("NewMissilesNowPierce", 4f);
        fireRateMultiplier = 5;
    }


    public int fireRateMultiplier
    {
        get { return _missileFireMultiplier; }

        set
        {
            if (_missileFireMultiplier != value)
            {
                _missileFireMultiplier = value;
                OnMissileFireRateMultiplierChanged(_missileFireMultiplier);
            }
        }
    }

    public int explosionChains
    {
        get { return _explosionChains; }

        set
        {
            if (_explosionChains != value)
            {
                _explosionChains = value;
                OnExplodeChainIncrementation();
            }
        }
    }    



    void InitializeMissilePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject missile = Instantiate(missilePrefab, Vector3.zero, Quaternion.identity);
            missile.SetActive(false);
            missilePool.Add(missile);
        }
    }


    // Get missiles from the pool for both turrets.
    public void PlayerStartsFiring()
    {
        GameObject missileLeft = GetPooledMissile();

        if (missileLeft != null)
        {
            missileLeft.transform.position = leftMissileOriginPoint.position;
            missileLeft.transform.rotation = leftMissileOriginPoint.rotation;
            missileLeft.SetActive(true);
        }

        GameObject missileRight = GetPooledMissile();

        if (missileRight != null)
        {
            missileRight.transform.position = rightMissileOriginPoint.position;
            missileRight.transform.rotation = rightMissileOriginPoint.rotation;
            missileRight.SetActive(true);
        }
    }

    GameObject GetPooledMissile()
    {
        // Find the first inactive missile in the pool
        return missilePool.Find(missile => !missile.activeInHierarchy);
    }


    void OnMissileFireRateMultiplierChanged(int newMultiplier)
    {
        if (newMultiplier != 0)
        {
            ScaleUpTurrets(newMultiplier);
            CancelInvoke("PlayerStartsFiring");
            InvokeRepeating("PlayerStartsFiring", 0, timeBetweenMissileFirings / fireRateMultiplier);
        }
    }

    void OnExplodeChainIncrementation()
    {
        NewMissilesExplodeMoreEnemies();
    }



    
    public void ScaleUpTurrets(int multiplier)
    {
        // clean up later.
        Debug.Log("scaled up turrets");
        Animator leftTurretAnim = PlayerController.Instance.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        Animator rightTurretAnim = PlayerController.Instance.gameObject.transform.GetChild(1).gameObject.GetComponent<Animator>();


        if (multiplier == 2)
        {
            leftTurretAnim.SetTrigger("level2");
            rightTurretAnim.SetTrigger("level2");
        }
        if (multiplier >= 3)
        {
            leftTurretAnim.SetTrigger("level3");
            rightTurretAnim.SetTrigger("level3");
        }



        // Prevent model from getting too large. 
        // 2 = scaleMultiplier of 1.5f. 3 or greater is 1.75f.
        // float scaleMultiplier = 1.25f;
        // scaleMultiplier += (0.25f * (multiplier - 1));
        // scaleMultiplier = Mathf.Min(scaleMultiplier, 1.75f);


        // var leftTurret = leftMissileOriginPoint.transform.parent.gameObject.transform;
        // var rightTurret = rightMissileOriginPoint.transform.parent.gameObject.transform;

        // leftTurret.localScale *= scaleMultiplier;
        // rightTurret.localScale *= scaleMultiplier;
    }


    public void NewMissilesNowPierce()
    {
        Debug.Log("missiles now pierce");
        foreach (GameObject bullet in missilePool)
            bullet.GetComponent<MissileHitEnemy>().StartPiercingWhenEnabledAgain();
    }

    public void NewMissilesExplodeMoreEnemies()
    {
        foreach (GameObject bullet in missilePool)
            bullet.GetComponent<MissileHitEnemy>().IncrementChainExplosionsWhenEnabledAgain(_explosionChains);
    }    
}
