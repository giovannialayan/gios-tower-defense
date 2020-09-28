using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyObj;

    //continuous spawning variables
    private bool spawnContinuously;
    private float spawnSpeed;
    private float timeSinceSpawn;
    private float timeSinceStartSpawn;

    // Start is called before the first frame update
    void Start()
    {
        spawnContinuously = false;
        spawnSpeed = 1;
        timeSinceSpawn = spawnSpeed;
        timeSinceStartSpawn = 0;
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3Int> thePath = transform.GetComponent<EnemyPathing>().BestPath;

        //start spawning continuously
        if (Input.GetKeyDown(KeyCode.E) && !spawnContinuously && thePath != null)
        {
            spawnContinuously = true;
            timeSinceStartSpawn = 0;
            timeSinceSpawn = spawnSpeed;
        }
        else if(Input.GetKeyDown(KeyCode.E) && spawnContinuously && thePath != null)
        {
            spawnContinuously = false;
        }

        //spawn enemies while i want them to be
        if (spawnContinuously)
        {
            if (timeSinceSpawn >= spawnSpeed)
            {
                SpawnEnemy();
                timeSinceSpawn = 0;
            }

            timeSinceSpawn += Time.fixedDeltaTime;
            timeSinceStartSpawn += Time.fixedDeltaTime;
        }
    }

    //spawn an enemy and give it the path to follow
    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyObj, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        enemy.GetComponent<EnemyAI>().Path = transform.GetComponent<EnemyPathing>().BestPath;
        enemy.GetComponent<EnemyAI>().IncreaseHealth(timeSinceStartSpawn);
    }
}
