using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    //enemy prefabs
    public GameObject enemyObj;
    public GameObject knightObj;
    public GameObject monkObj;
    public GameObject paladinObj;
    public GameObject assassinObj;
    public GameObject succubusObj;
    public GameObject armorerObj;
    public GameObject warlockObj;

    //spawning variables
    //private bool spawnContinuously;
    private float spawnSpeed;
    private float timeSinceSpawn;
    private int numPerSpawn;
    //private float timeSinceStartSpawn;
    private int numToSpawn;
    private Queue<Wave> currentWave;
    private List<Vector3Int> thePath;

    //gamemanager
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        //spawnContinuously = false;
        spawnSpeed = 1;
        timeSinceSpawn = spawnSpeed;
        //timeSinceStartSpawn = 0;
        thePath = null;

        currentWave = new Queue<Wave>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.IsPaused)
        {
            return;
        }

        //spawn enemies
        //Debug.Log(numToSpawn + " , " + thePath);
        if (currentWave.Count > 0 && thePath != null)
        {
            //Debug.Log("things are happening, i swear");
            if (timeSinceSpawn >= spawnSpeed)
            {
                SpawnEnemy();
                timeSinceSpawn = 0;
                numToSpawn++;

                if (numToSpawn == currentWave.Peek().enemies)
                {
                    currentWave.Dequeue();
                    numToSpawn = 0;
                }
            }

            timeSinceSpawn += Time.fixedDeltaTime;
            //timeSinceStartSpawn += Time.fixedDeltaTime;
        }
    }

    //spawn an enemy and give it the path to follow
    private void SpawnEnemy()
    {
        GameObject enemy;

        switch (currentWave.Peek().enemyClass)
        {
            case EnemyClass.Cur:
                enemy = Instantiate(enemyObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Knight:
                enemy = Instantiate(knightObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Monk:
                enemy = Instantiate(monkObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Paladin:
                enemy = Instantiate(paladinObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Assassin:
                enemy = Instantiate(assassinObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Succubus:
                enemy = Instantiate(succubusObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Armorer:
                enemy = Instantiate(armorerObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            case EnemyClass.Warlock:
                enemy = Instantiate(warlockObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;

            default:
                enemy = Instantiate(enemyObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
                break;
        }

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        enemyAI.Path = thePath;
        enemyAI.SetEnemyStats(currentWave.Peek().health, currentWave.Peek().attack, currentWave.Peek().speed, currentWave.Peek().worth, currentWave.Peek().waveNumber, currentWave.Peek().ChooseType(), currentWave.Peek().enemyClass);

        //Debug.Log("new enemy spawned");
    }

    //start spawning the input wave
    public void StartSpawning(Wave wave)
    {
        if (thePath == null)
        {
            thePath = transform.GetComponent<EnemyPathing>().SetUpPath();
        }

        spawnSpeed = 30f / wave.enemies;

        if (spawnSpeed > 1)
        {
            spawnSpeed = 1;
        }

        //spawnSpeed = -1 * wave.enemies + 1;

        //numPerSpawn = Mathf.Abs(Mathf.FloorToInt(spawnSpeed)) + 1;

        //if (spawnSpeed <= 0)
        //{
        //    spawnSpeed = .01f;
        //}
        
        timeSinceSpawn = spawnSpeed;
        currentWave.Enqueue(wave);
    }
}
