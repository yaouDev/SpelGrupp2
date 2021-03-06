using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour {
    [Header("What to Spawn")]
    [SerializeField] private string[] spawnThis;
    [SerializeField] private GameObject[] spawnThisGO;
    private int maxSpawnThisMany;
    private int minSpawnThisMany;
    private int spawnThisMany;
    private string spawnedEnemy;
    private GameObject spawnedEnemyGO;
    [Header("Normal Spawn")]
    [SerializeField] private int nMaxSpawnCount = 25;
    [SerializeField] private int nMaxSpawnCooldown = 7;
    [SerializeField] private int nMinSpawnCooldown = 4;
    [SerializeField] private int nMaxSpawnThisMany = 2;
    [SerializeField] private int nMinSpawnThisMany = 1;
    [SerializeField] private int nSpawnDistanceMax = 35;
    [SerializeField] private int nSpawnDistanceMin = 5;
    [SerializeField] private int nEnemyShootRange;
    [SerializeField] private int nEnemyMeeleRange;
    [SerializeField] private int nEnemyShieldRange;
    private int nSpawnCooldown;
    [Header("Night Spawn")]
    [SerializeField] private int nightMaxSpawnCount = 30;
    [SerializeField] private int nightMaxSpawnCooldown = 5;
    [SerializeField] private int nightMinSpawnCooldown = 1;
    [SerializeField] private int nightMaxSpawnThisMany = 3;
    [SerializeField] private int nightMinSpawnThisMany = 1;
    [SerializeField] private int nightSpawnDistanceMax = 35;
    [SerializeField] private int nightSpawnDistanceMin = 5;
    [SerializeField] private int nightEnemyShootRange;
    [SerializeField] private int nightEnemyMeeleRange;
    [SerializeField] private int nightEnemyShieldRange;
    private int nightSpawnCooldown;
    [Header("Generator Spawn")]
    [SerializeField] private int genMaxSpawnCount = 40;
    [SerializeField] private int genMaxSpawnCooldown = 3;
    [SerializeField] private int genMinSpawnCooldown = 1;
    [SerializeField] private int genMaxSpawnThisMany = 5;
    [SerializeField] private int genMinSpawnThisMany = 3;
    [SerializeField] private int genSpawnDistanceMax = 50;
    [SerializeField] private int genSpawnDistanceMin = 15;
    [SerializeField] private int genEnemyShootRange;
    [SerializeField] private int genEnemyMeeleRange;
    [SerializeField] private int genEnemyShieldRange;
    private int genSpawnCooldown;
    private int maxSpawnCount;
    private int spawnCooldown;
    private int spawnDistanceMax;
    private int spawnDistanceMin;
    private int spawnCount;
    private int enemyShootRange;
    private int enemyMeeleRange;
    private int enemyShieldRange;
    private int index;
    private float distanceP1;
    private float distanceP2;
    private int dropRoll;

    private CallbackSystem.PlayerHealth[] players;
    private GameObject[] spawnLocations;
    private GameObject activeSpawner;
    private List<GameObject> nearbySpawners = new List<GameObject>();
    private Transform spawnPos;
    private DayNightSystem dayNightSystem;
    private Vector2Int playerOneModule;
    private Vector2Int playerTwoModule;
    private Vector2Int spwanPosModule;


    void Start() {
        dayNightSystem = FindObjectOfType<DayNightSystem>();
        spawnLocations = GameObject.FindGameObjectsWithTag("SpawnLocation");
        players = new CallbackSystem.PlayerHealth[2];
        GameObject[] tmp = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < tmp.Length; i++) players[i] = tmp[i].GetComponent<CallbackSystem.PlayerHealth>();
        spawnedEnemyGO = spawnThisGO[0];
        StartCoroutine(SpawnObject());
    }
    private void FixedUpdate() {
        IsNight();
    }

    public Vector3 ClosestPlayer {
        get {
            CallbackSystem.PlayerHealth closestTarget = Vector3.Distance(players[0].transform.position, transform.position) >
            Vector3.Distance(players[1].transform.position, transform.position) ? closestTarget = players[1] : players[0];
            return closestTarget.transform.position;
        }
    }

    public IEnumerator SpawnObject() {
        while (true) {
            nSpawnCooldown = Random.Range(nMinSpawnCooldown, nMaxSpawnCooldown);
            nightSpawnCooldown = Random.Range(nightMinSpawnCooldown, nightMinSpawnCooldown);
            genSpawnCooldown = Random.Range(genMinSpawnCooldown, genMaxSpawnCooldown);
            spawnThisMany = Random.Range(minSpawnThisMany, maxSpawnThisMany);


            for (int i = 0; i < spawnLocations.Length; i++) {
                distanceP1 = Vector3.Distance(players[0].transform.position, spawnLocations[i].transform.position);
                distanceP2 = Vector3.Distance(players[1].transform.position, spawnLocations[i].transform.position);
                if (((distanceP1 < spawnDistanceMax && distanceP1 > spawnDistanceMin) || (distanceP2 < spawnDistanceMax && distanceP2 > spawnDistanceMin))
                && CheckModuleAdjacent(spawnLocations[i].transform.position)) {
                    nearbySpawners.Add(spawnLocations[i]);
                }
            }
            if (nearbySpawners.Count > 0 && spawnCount < maxSpawnCount) {
                for (int i = 0; i < spawnThisMany; i++) {
                    if (i == nearbySpawners.Count) activeSpawner = nearbySpawners[i % nearbySpawners.Count];
                    else activeSpawner = nearbySpawners[i];
                    spawnPos = activeSpawner.transform;
                    spawnPos.rotation = Quaternion.LookRotation((ClosestPlayer - spawnPos.position).normalized);
                    EnemyRange();
                    Instantiate(spawnedEnemyGO, spawnPos.position, spawnPos.rotation);
                    spawnCount++;
                }
                nearbySpawners.Clear();
            }
            /*        if (nearbySpawners.Count > 0 && spawnCount < maxSpawnCount) {
                       index = Random.Range(0, nearbySpawners.Count);
                       activeSpawner = nearbySpawners[index];
                       spawnPos = activeSpawner.transform;
                       spawnPos.rotation = Quaternion.LookRotation((ClosestPlayer - spawnPos.position).normalized);
                       EnemyRange();
                       for (int i = 0; i < spawnThisMany; i++) {
                           Instantiate(spawnedEnemyGO, spawnPos.position, spawnPos.rotation);
                           //ObjectPool.Instance.GetFromPool(spawnedEnemy, spawnPos.position, spawnPos.rotation, null, true);
                           nearbySpawners.Clear();
                           spawnCount += 1;
                       }
                   } */
            yield return new WaitForSeconds(spawnCooldown);
        }
    }

    public void reduceSpawnCount(int amount) {
        spawnCount -= amount;
    }

    public void GeneratorRunning(bool on) {
        if (on) {
            maxSpawnThisMany = genMaxSpawnThisMany;
            minSpawnThisMany = genMinSpawnThisMany;
            spawnCooldown = genSpawnCooldown;
            maxSpawnCount = genMaxSpawnCount;
            spawnDistanceMax = genSpawnDistanceMax;
            spawnDistanceMin = genSpawnDistanceMin;
            enemyShootRange = genEnemyShootRange;
            enemyMeeleRange = genEnemyMeeleRange;
            enemyShieldRange = genEnemyShieldRange;
        }
        if (!on) {
            maxSpawnThisMany = nMaxSpawnThisMany;
            minSpawnThisMany = nMinSpawnThisMany;
            spawnCooldown = nSpawnCooldown;
            maxSpawnCount = nMaxSpawnCount;
            spawnDistanceMax = nSpawnDistanceMax;
            spawnDistanceMin = nSpawnDistanceMin;
            enemyShootRange = nEnemyShootRange;
            enemyMeeleRange = nEnemyMeeleRange;
            enemyShieldRange = nEnemyShieldRange;
        }
    }
    private void IsNight() {
        if (!dayNightSystem.Isday) {
            maxSpawnThisMany = nightMaxSpawnThisMany;
            minSpawnThisMany = nightMinSpawnThisMany;
            spawnCooldown = nightSpawnCooldown;
            maxSpawnCount = nightMaxSpawnCount;
            spawnDistanceMax = nightSpawnDistanceMax;
            spawnDistanceMin = nightSpawnDistanceMin;
            enemyShootRange = nightEnemyShootRange;
            enemyMeeleRange = nightEnemyMeeleRange;
            enemyShieldRange = nightEnemyShieldRange;
        } else {
            maxSpawnThisMany = nMaxSpawnThisMany;
            minSpawnThisMany = nMinSpawnThisMany;
            spawnCooldown = nSpawnCooldown;
            maxSpawnCount = nMaxSpawnCount;
            spawnDistanceMax = nSpawnDistanceMax;
            spawnDistanceMin = nSpawnDistanceMin;
            enemyShootRange = nEnemyShootRange;
            enemyMeeleRange = nEnemyMeeleRange;
            enemyShieldRange = nEnemyShieldRange;
        }
    }
    private void EnemyRange() {

        for (int i = 0; i < spawnThisMany; i++) {
            dropRoll = Random.Range(0, 100);

            if (dropRoll <= enemyShootRange) {
                spawnedEnemyGO = spawnThisGO[0];
                spawnedEnemy = spawnThis[0];
            } else if (dropRoll <= enemyMeeleRange) {
                spawnedEnemyGO = spawnThisGO[1];
                spawnedEnemy = spawnThis[1];
            } else if (dropRoll <= enemyShieldRange) {
                spawnedEnemyGO = spawnThisGO[2];
                spawnedEnemy = spawnThis[2];
            }
        }

    }

    private bool CheckModuleAdjacent(Vector3 position) {
        // nuvarande modul
        playerOneModule = DynamicGraph.Instance.GetModulePosFromWorldPos(players[0].transform.position);
        playerTwoModule = DynamicGraph.Instance.GetModulePosFromWorldPos(players[1].transform.position);

        // spawnpos modul
        spwanPosModule = DynamicGraph.Instance.GetModulePosFromWorldPos(position);

        // kolla om nuvarande modul har en entrance mot spwanpos
        return DynamicGraph.Instance.ModulesAdjacent(playerOneModule, spwanPosModule)
            || DynamicGraph.Instance.ModulesAdjacent(playerTwoModule, spwanPosModule);
    }


}
