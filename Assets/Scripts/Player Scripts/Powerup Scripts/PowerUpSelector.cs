using System.Linq.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Needs huge refactor later.

public class PowerUpSelector : MonoBehaviour
{
    GameManager gameManager;
    FireMissile playerMissiles;

    public List<GameObject> powerUpsUIList;
    [SerializeField] float baseDroneRespawnTime = 3f;
    [SerializeField] Vector3 powerUpAttachPoint;

    [Space(15)]
    [SerializeField] GameObject leftDrone;
    [SerializeField] GameObject rightDrone;
    GameObject leftDronePowerUpUI, rightDronePowerUpUI;    
    Animator leftDroneAnim,rightDroneAnim;
    PowerUp leftDronePowerUp, rightDronePowerUp;

    List<PowerUp> availablePowerUps;

    public string Name { get; set; }

    bool allowPowerUpChoice;

    int totalPowerUpsAcquired;


    void Start()
    {
        gameManager = GameManager.Instance;
        playerMissiles = FireMissile.Instance;

        leftDroneAnim = leftDrone.GetComponent<Animator>();
        rightDroneAnim = rightDrone.GetComponent<Animator>();

        AddPowerUps();
    }


    void Update()
    {
        if (!allowPowerUpChoice || gameManager.gameHasEnded)
            return;

        if (Input.GetMouseButton(0))
            StartCoroutine(ExecuteChosenPowerUp(leftDrone, leftDronePowerUp, leftDronePowerUpUI));
        
        if (Input.GetMouseButton(1))
            StartCoroutine(ExecuteChosenPowerUp(rightDrone, rightDronePowerUp, rightDronePowerUpUI));
    }


    public void AddPowerUps()
    {
        availablePowerUps = new List<PowerUp>();
        availablePowerUps.Add(new PowerUp("FireRateBoost"));
        availablePowerUps.Add(new PowerUp("MissilesPierce"));
        availablePowerUps.Add(new PowerUp("ExplodeEnemies"));

        availablePowerUps.Add(new PowerUp("BlackHoleOnDeath"));

        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.tag == "PowerUp")
                powerUpsUIList.Add(child.gameObject);
        }
    }


    public IEnumerator SpawnDrones()
    {
        playerMissiles = FireMissile.Instance;

        ResetDroneAnimationTriggers();
        PowerUp currentPowerUpLeft, currentPowerUpRight;
        GetRandomPowerUp(out currentPowerUpLeft, out currentPowerUpRight);

        leftDronePowerUp = currentPowerUpLeft;
        rightDronePowerUp = currentPowerUpRight;

        leftDronePowerUpUI = GetUIOfPowerUp(leftDronePowerUp);
        rightDronePowerUpUI = GetUIOfPowerUp(rightDronePowerUp);

        yield return SetupUI(leftDronePowerUpUI, leftDrone);
        yield return SetupUI(rightDronePowerUpUI, rightDrone);


        leftDroneAnim.SetTrigger("engage");
        rightDroneAnim.SetTrigger("engage");

        float droneToPlayerEngageLengths = leftDroneAnim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(droneToPlayerEngageLengths);

        // Player can finally now choose a power-up.
        allowPowerUpChoice = true;
    }

    void GetRandomPowerUp(out PowerUp currentPowerUpLeft, out PowerUp currentPowerUpRight)
    {
        int randomIndex = Random.Range(0, availablePowerUps.Count);        
        currentPowerUpLeft = availablePowerUps[randomIndex];

        // (1) Quickly prevent second drone from from selecting first one's powerup.
        availablePowerUps.Remove(currentPowerUpLeft);

        randomIndex = Random.Range(0, availablePowerUps.Count);
        currentPowerUpRight = availablePowerUps[randomIndex];

        // (2) Add it back.
        availablePowerUps.Add(currentPowerUpLeft);
    }        


    // Not how I wanted to do it, but works.
    GameObject GetUIOfPowerUp(PowerUp powerUp)
    {
        foreach (GameObject powerUpUI in powerUpsUIList)
        {            
            switch (powerUp.Name)
            {
                case "FireRateBoost":
                    var currentFireRate = playerMissiles.fireRateMultiplier;

                    if (currentFireRate == 1 && powerUpUI.name == "FireRateBoost UI (2x)")
                        return powerUpUI;
                    break;

                case "FireRateSuperBoost":
                    var currentFireRate_super = playerMissiles.fireRateMultiplier;

                    if (currentFireRate_super == 2 && powerUpUI.name == "FireRateBoost UI (3x)")
                        return powerUpUI;
                    break;            


                case "ExplodeEnemies":
                    var explodeChains = playerMissiles.explosionChains;

                    if (explodeChains == 0 && powerUpUI.name == "ExplodeEnemies UI")
                        return powerUpUI;
                    break;
                    
                case "ChainExplodeEnemies":
                    var explodeChains_super = playerMissiles.explosionChains;

                    if (explodeChains_super == 1 && powerUpUI.name == "ExplodeEnemies (More) UI")
                        return powerUpUI;
                    break;


                case "MissilesPierce":                                           
                    if (powerUpUI.name == "MissilesPierce UI")
                        return powerUpUI;
                    break;


                case "BlackHoleOnDeath":
                    if (powerUpUI.name == "BlackHole UI")
                        return powerUpUI;
                    break;
            }
        }

        return null;
    }        


    void ResetDroneAnimationTriggers()
    {
        leftDroneAnim.ResetTrigger("engage");
        leftDroneAnim.ResetTrigger("disengage");
        rightDroneAnim.ResetTrigger("engage");
        rightDroneAnim.ResetTrigger("disengage");
    }



    IEnumerator SetupUI(GameObject powerUpUI, GameObject drone)
    {
        powerUpUI.SetActive(true);
        powerUpUI.transform.SetParent(drone.transform);

        powerUpUI.transform.localPosition = new Vector3(powerUpAttachPoint.x, powerUpAttachPoint.y, powerUpAttachPoint.z);            
        yield break;
    }



    IEnumerator ExecuteChosenPowerUp(GameObject drone, PowerUp powerUp, GameObject powerUpUI)
    {
        allowPowerUpChoice = false;
        totalPowerUpsAcquired++;

        MissilePiercingWasSelected(powerUp);
        MissileFireRateWasSelected(powerUp);
        EnemyExplosionWasSelected(powerUp);
        BlackHoleWasSelected(powerUp);

        Vector3 detachPointWithOffset = new Vector3(drone.transform.position.x, drone.transform.position.y - 0.6f, drone.transform.position.z);        
        yield return StartCoroutine(powerUpUI.GetComponent<DroppedPowerUpMovesToPlayer>().Move(detachPointWithOffset, this.gameObject));
        GetComponent<AudioSource>().Play();
        

        leftDroneAnim.SetTrigger("disengage");
        rightDroneAnim.SetTrigger("disengage");
        float droneDisengageLength = leftDroneAnim.GetCurrentAnimatorStateInfo(0).length;

        yield return StartCoroutine(ReturnPowerUpUIsToPowerUpUIList(droneDisengageLength));

        if (totalPowerUpsAcquired < 5)
        {
            yield return new WaitForSeconds(baseDroneRespawnTime);
            StartCoroutine(SpawnDrones());
        }
        else
            StopAllCoroutines();

        yield break;
    }


    IEnumerator ReturnPowerUpUIsToPowerUpUIList(float droneDisengageLength)
    {
        yield return StartCoroutine(WaitForSomething(droneDisengageLength - 0.1f));
        leftDronePowerUpUI.transform.SetParent(gameObject.transform);
        rightDronePowerUpUI.transform.SetParent(gameObject.transform);
        yield break;
    }


    # region Determine power-up actions.
    void MissilePiercingWasSelected(PowerUp selectedPowerUp)
    {
        if (selectedPowerUp.Name == "MissilesPierce")
        {
            playerMissiles.NewMissilesNowPierce();
            availablePowerUps.Remove(selectedPowerUp);
        }
    }


    void MissileFireRateWasSelected(PowerUp selectedPowerUp)
    {
        if (selectedPowerUp.Name == "FireRateBoost" || selectedPowerUp.Name == "FireRateSuperBoost")
        {
            playerMissiles.fireRateMultiplier++;
            availablePowerUps.Remove(selectedPowerUp);

            if (selectedPowerUp.Name == "FireRateBoost")
            {
                GameObject fireEvenFaster = GameObject.Find("FireRateBoost UI (3x)");
                fireEvenFaster.transform.SetParent(gameObject.transform);

                powerUpsUIList.Add(fireEvenFaster);
                availablePowerUps.Add(new PowerUp("FireRateSuperBoost"));
            }
        }               
    }


    void EnemyExplosionWasSelected(PowerUp selectedPowerUp)
    {
        if (selectedPowerUp.Name == "ExplodeEnemies" || selectedPowerUp.Name == "ChainExplodeEnemies")
        {
            playerMissiles.explosionChains++;
            availablePowerUps.Remove(selectedPowerUp);

            if (selectedPowerUp.Name == "ExplodeEnemies")
            {
                GameObject explodeMore = GameObject.Find("ExplodeEnemies (More) UI");
                explodeMore.transform.SetParent(gameObject.transform);

                powerUpsUIList.Add(explodeMore);
                availablePowerUps.Add(new PowerUp("ChainExplodeEnemies"));
            }
        }
    }


    void BlackHoleWasSelected(PowerUp selectedPowerUp)
    {
        if (selectedPowerUp.Name == "BlackHoleOnDeath")
        {
            PlayerController.Instance.canBecomeBlackHole = true;
            availablePowerUps.Remove(selectedPowerUp);
        }
    }

    #endregion


    IEnumerator WaitForSomething(float length)    
    {
        yield return new WaitForSeconds(length);        
    }


    public int TotalPowerUpsAcquired {get { return totalPowerUpsAcquired; }}
    
}


    public class PowerUp
    {
        public string Name { get; set; }


        public PowerUp(string name) =>
            Name = name;
    }