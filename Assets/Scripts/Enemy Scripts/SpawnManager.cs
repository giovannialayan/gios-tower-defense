using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyObj;

    //spawning variables
    //private bool spawnContinuously;
    private float spawnSpeed;
    private float timeSinceSpawn;
    //private float timeSinceStartSpawn;
    private int numToSpawn;
    private Wave currentWave;
    private List<Vector3Int> thePath;

    //gamemanager
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        //spawnContinuously = false;
        spawnSpeed = 1;
        timeSinceSpawn = spawnSpeed;
        //timeSinceStartSpawn = 0;
        thePath = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.IsPaused)
        {
            return;
        }

        //start spawning continuously
        //if (Input.GetKeyDown(KeyCode.E) && !spawnContinuously && thePath != null)
        //{
        //    spawnContinuously = true;
        //    timeSinceStartSpawn = 0;
        //    timeSinceSpawn = spawnSpeed;
        //}
        //else if(Input.GetKeyDown(KeyCode.E) && spawnContinuously && thePath != null)
        //{
        //    spawnContinuously = false;
        //}

        //spawn enemies
        //Debug.Log(numToSpawn + " , " + thePath);
        if (numToSpawn > 0 && thePath != null)
        {
            //Debug.Log("things are happening, i swear");
            if (timeSinceSpawn >= spawnSpeed)
            {
                SpawnEnemy();
                timeSinceSpawn = 0;
                numToSpawn--;
            }

            timeSinceSpawn += Time.fixedDeltaTime;
            //timeSinceStartSpawn += Time.fixedDeltaTime;
        }
    }

    //spawn an enemy and give it the path to follow
    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        //enemy.GetComponent<EnemyAI>().Path = transform.GetComponent<EnemyPathing>().BestPath;
        //enemy.GetComponent<EnemyAI>().IncreaseHealth(timeSinceStartSpawn);

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        enemyAI.Path = thePath;
        enemyAI.SetEnemyStats(currentWave.health, currentWave.attack, currentWave.speed, currentWave.worth, currentWave.ChooseType());
        enemyAI.gamemanager = gameManager;

        //Debug.Log("new enemy spawned");
    }

    //start spawning the input wave
    public void StartSpawning(Wave wave)
    {
        if (thePath == null)
        {
            thePath = transform.GetComponent<EnemyPathing>().SetUpPath();
        }

        numToSpawn = wave.enemies;
        timeSinceSpawn = spawnSpeed;
        currentWave = wave;
    }
}
